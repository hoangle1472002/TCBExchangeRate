using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TCBExchangeRate.Application.Dtos;
using TCBExchangeRate.Application.Helpers;
using TCBExchangeRate.Application.Interfaces;
using TCBExchangeRate.Application.Models.Responses;
using TCBExchangeRate.Domain.Entities;
using TCBExchangeRate.Infrastructure.Interfaces;

namespace TCBExchangeRate.Infrastructure.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly ILogger<ExchangeRateService> _logger;

    private const string BaseUrlFormat = "https://techcombank.com/content/techcombank/web/vn/vi/cong-cu-tien-ich/ty-gia/_jcr_content.exchange-rates.{0}.{1}.integration.json";
    private const string DefaultSnapshotTime = "00-00-00";

    public ExchangeRateService(
        IHttpClientFactory httpClientFactory,
        IExchangeRateRepository exchangeRateRepository,
        ICurrencyRepository currencyRepository,
        ILogger<ExchangeRateService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _exchangeRateRepository = exchangeRateRepository;
        _currencyRepository = currencyRepository;
        _logger = logger;
    }

    public async Task<Result<int>> ImportExchangeRatesAsync(DateOnly date)
    {
        _logger.LogInformation("Starting import for date {Date}", date);

        var client = CreateConfiguredClient();
        var timeListUrl = string.Format(BaseUrlFormat, date.ToString("yyyy-MM-dd"), DefaultSnapshotTime);

        var updatedTimes = await GetUpdatedTimesAsync(client, timeListUrl);
        if (updatedTimes is null || updatedTimes.Count == 0)
        {
            _logger.LogInformation("No snapshots found for {Date}", date);
            return Result<int>.Ok(0, "No snapshots found for the specified date.");
        }

        var existingSnapshots = await _exchangeRateRepository.GetSnapshotTimesAsync(date);
        var existingSnapshotSet = new HashSet<DateTime>(existingSnapshots);

        var snapshotsToInsert = new List<ExchangeRateSnapshot>();
        int totalRatesSaved = 0;

        foreach (var time in updatedTimes)
        {
            var snapshotDateTime = ParseHelper.ParseSnapshotDateTime(date, time);
            if (existingSnapshotSet.Contains(snapshotDateTime))
            {
                _logger.LogDebug("Snapshot at {SnapshotTime} already exists. Skipping.", snapshotDateTime);
                continue;
            }

            var formattedDate = date.ToString("yyyy-MM-dd");
            var formattedTime = time.Replace(":", "-");
            var detailUrl = string.Format(BaseUrlFormat, formattedDate, formattedTime);

            var rateSnapshot = await BuildSnapshotFromUrlAsync(client, detailUrl, snapshotDateTime);
            if (rateSnapshot == null)
            {
                _logger.LogWarning("Failed to build snapshot from {Url}", detailUrl);
                continue;
            }

            snapshotsToInsert.Add(rateSnapshot);
            totalRatesSaved += rateSnapshot.ExchangeRates.Count;

            _logger.LogInformation("Built snapshot for {SnapshotTime} with {Count} rates", snapshotDateTime, rateSnapshot.ExchangeRates.Count);
        }

        if (snapshotsToInsert.Count > 0)
        {
            await _exchangeRateRepository.AddSnapshotsAsync(snapshotsToInsert);
            await _exchangeRateRepository.SaveChangesAsync();

            _logger.LogInformation("Saved {SnapshotCount} snapshots for date {Date}", snapshotsToInsert.Count, date);
        }

        return Result<int>.Ok(totalRatesSaved, "Snapshots imported successfully.");
    }

    public async Task<int> ImportExchangeRatesFromPastMonthAsync()
    {
        _logger.LogInformation("Starting import of exchange rates from past month");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var oneMonthAgo = today.AddMonths(-1);

        int total = 0;
        for (var date = oneMonthAgo; date <= today; date = date.AddDays(1))
        {
            var result = await ImportExchangeRatesAsync(date);
            if (result.Success)
            {
                total += result.Data;
                _logger.LogInformation("Imported {Count} rates on {Date}", result.Data, date);
            }
            else
            {
                _logger.LogWarning("Failed to import on {Date}: {Message}", date, result.Message);
            }
        }

        _logger.LogInformation("Completed monthly import. Total rates saved: {Total}", total);
        return total;
    }

    public async Task<Result<List<ExchangeRateResponse>>> GetExchangeRateSnapshotsAsync(DateOnly date, string currencyCode)
    {
        _logger.LogInformation("Getting exchange rate snapshots for {Date} and currency {CurrencyCode}", date, currencyCode);

        try
        {
            var startUtc = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

            var exchangeRates = _exchangeRateRepository.QueryExchangeRates()
                .Where(rate => rate.Currency.Code == currencyCode &&
                               rate.Snapshot.SnapshotDateTime >= startUtc &&
                               rate.Snapshot.SnapshotDateTime <= endUtc)
                .OrderBy(rate => rate.Snapshot.SnapshotDateTime)
                .Select(rate => new ExchangeRateResponse
                {
                    CurrencyCode = rate.Currency.Code,
                    SnapshotTime = rate.Snapshot.SnapshotDateTime,
                    PurchaseCashCheque = rate.BidRateCK,
                    PurchaseTransfer = rate.BidRateTM,
                    SellingCashCheque = rate.AskRate,
                    SellingTransfer = rate.AskRateTM
                });

            var result = await exchangeRates.ToListAsync();

            _logger.LogInformation("Found {Count} exchange rate records for {CurrencyCode} on {Date}", result.Count, currencyCode, date);
            return Result<List<ExchangeRateResponse>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching exchange rates for {Date} and currency {CurrencyCode}", date, currencyCode);
            return Result<List<ExchangeRateResponse>>.Fail("Error fetching exchange rates", new() { ex.Message });
        }
    }

    private HttpClient CreateConfiguredClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        client.DefaultRequestHeaders.Add("Referer", "https://techcombank.com/");
        return client;
    }

    private static async Task<List<string>?> GetUpdatedTimesAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<ExchangeRateWrapper>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return wrapper?.ExchangeRate?.UpdatedTimes;
    }

    private async Task<ExchangeRateSnapshot?> BuildSnapshotFromUrlAsync(HttpClient client, string url, DateTime snapshot)
    {
        try
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<ExchangeRateWrapper>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var rateItems = wrapper?.ExchangeRate?.Data;
            if (rateItems is null || rateItems.Count == 0) return null;

            var currencyCodes = rateItems.Select(x => x.Label).Distinct().ToList();

            var currencyList = await _currencyRepository.GetByCodesAsync(currencyCodes);
            var currencyDict = currencyList.ToDictionary(c => c.Code, c => c);

            var snapshotEntity = new ExchangeRateSnapshot { SnapshotDateTime = snapshot };

            foreach (var item in rateItems)
            {
                if (!currencyDict.TryGetValue(item.Label, out var currency)) continue;

                var rate = new ExchangeRate
                {
                    Currency = currency,
                    AskRate = ParseHelper.ParseDecimal(item.AskRate),
                    AskRateTM = ParseHelper.ParseDecimal(item.AskRateTM),
                    BidRateCK = ParseHelper.ParseDecimal(item.BidRateCK),
                    BidRateTM = ParseHelper.ParseDecimal(item.BidRateTM)
                };

                snapshotEntity.ExchangeRates.Add(rate);
            }

            return snapshotEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build snapshot from URL {Url}", url);
            return null;
        }
    }
}
