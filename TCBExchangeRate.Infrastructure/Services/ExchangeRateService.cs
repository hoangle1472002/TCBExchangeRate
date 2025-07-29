using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using TCBExchangeRate.Application.Dtos;
using TCBExchangeRate.Application.Interfaces;
using TCBExchangeRate.Domain.Entities;
using TCBExchangeRate.Infrastructure.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TCBExchangeRate.Infrastructure.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly ICurrencyRepository _currencyRepository;

    private const string BaseUrlFormat = "https://techcombank.com/content/techcombank/web/vn/vi/cong-cu-tien-ich/ty-gia/_jcr_content.exchange-rates.{0}.{1}.integration.json";

    public ExchangeRateService(
        IHttpClientFactory httpClientFactory,
        IExchangeRateRepository exchangeRateRepository,
        ICurrencyRepository currencyRepository)
    {
        _httpClientFactory = httpClientFactory;
        _exchangeRateRepository = exchangeRateRepository;
        _currencyRepository = currencyRepository;
    }

    public async Task<int> ImportExchangeRatesAsync(DateOnly date)
    {
        var client = CreateConfiguredClient();
        var timeListUrl = string.Format(BaseUrlFormat, date.ToString("yyyy-MM-dd"), "00-00-00");

        var updatedTimes = await GetUpdatedTimesAsync(client, timeListUrl);
        if (updatedTimes is null || updatedTimes.Count == 0)
            return 0;

        var existingSnapshots = await _exchangeRateRepository.GetSnapshotTimesAsync(date);
        var existingSnapshotSet = new HashSet<DateTime>(existingSnapshots);

        var snapshotsToInsert = new List<ExchangeRateSnapshot>();
        int totalRatesSaved = 0;

        foreach (var time in updatedTimes)
        {
            var snapshotDateTime = ParseSnapshotDateTime(date, time);
            if (existingSnapshotSet.Contains(snapshotDateTime)) continue;

            var formattedDate = date.ToString("yyyy-MM-dd");
            var formattedTime = time.Replace(":", "-");
            var detailUrl = string.Format(BaseUrlFormat, formattedDate, formattedTime);

            var rateSnapshot = await BuildSnapshotFromUrlAsync(client, detailUrl, snapshotDateTime);
            if (rateSnapshot == null) continue;

            snapshotsToInsert.Add(rateSnapshot);
            totalRatesSaved += rateSnapshot.ExchangeRates.Count;
        }

        if (snapshotsToInsert.Count > 0)
        {
            await _exchangeRateRepository.AddSnapshotsAsync(snapshotsToInsert);
            await _exchangeRateRepository.SaveChangesAsync();
        }

        return totalRatesSaved;
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

    private async Task<List<string>?> GetUpdatedTimesAsync(HttpClient client, string url)
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

    private DateTime ParseSnapshotDateTime(DateOnly date, string time)
    {
        return DateTime.SpecifyKind(
            DateTime.ParseExact($"{date:yyyy-MM-dd} {time}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            DateTimeKind.Utc
        );
    }

    private async Task<ExchangeRateSnapshot?> BuildSnapshotFromUrlAsync(HttpClient client, string url, DateTime snapshot)
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
                AskRate = ParseDecimal(item.AskRate),
                AskRateTM = ParseDecimal(item.AskRateTM),
                BidRateCK = ParseDecimal(item.BidRateCK),
                BidRateTM = ParseDecimal(item.BidRateTM)
            };

            snapshotEntity.ExchangeRates.Add(rate);
        }

        return snapshotEntity;
    }

    private decimal ParseDecimal(string? input) =>
        decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0;
}
