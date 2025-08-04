using TCBProxyApp.Models;

namespace TCBProxyApp.Services
{
    public class ExchangeRateService
    {
        private readonly IHttpClientFactory _httpClient;

        public ExchangeRateService(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ExchangeRateData>> GetExchangeRateByCurrencyAndDateAsync(string currencyCode, string date)
        {
            var baseApiUrl = $"https://techcombank.com/content/techcombank/web/vn/vi/cong-cu-tien-ich/ty-gia/_jcr_content.exchange-rates.{date}";
            var _httpClient = CreateConfiguredClient();

            var latestDataUrl = $"{baseApiUrl}.integration.json";
            var latestResponse = await _httpClient.GetFromJsonAsync<RootExchangeRateResponse>(latestDataUrl);

            if (latestResponse?.ExchangeRate?.UpdatedTimes == null)
                return new List<ExchangeRateData>();

            var result = new List<ExchangeRateData>();
            var latestMatched = latestResponse.ExchangeRate.Data
                ?.Where(x => string.Equals(x.Label, currencyCode, StringComparison.OrdinalIgnoreCase))
                ?.ToList();

            if (latestMatched != null && latestMatched.Count > 0)
                result.AddRange(latestMatched);

            var remainingTimes = latestResponse.ExchangeRate.UpdatedTimes
                .Where(t => !string.Equals(t, latestMatched?.FirstOrDefault()?.InputDate))
                .Distinct();

            foreach (var time in remainingTimes)
            {
                var timeFormatted = time.Replace(":", "-");
                var apiUrl = $"{baseApiUrl}.{timeFormatted}.integration.json";

                try
                {
                    var response = await _httpClient.GetFromJsonAsync<RootExchangeRateResponse>(apiUrl);
                    var matched = response?.ExchangeRate?.Data
                        ?.Where(x => string.Equals(x.Label, currencyCode, StringComparison.OrdinalIgnoreCase))
                        ?.ToList();

                    if (matched != null && matched.Count > 0)
                        result.AddRange(matched);
                }
                catch
                {
                    continue;
                }
            }

            return result
                .Where(x => x.InputDate != null)
                .OrderBy(x => x.InputDate)
                .ToList();
        }


        private HttpClient CreateConfiguredClient()
        {
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            client.DefaultRequestHeaders.Add("Referer", "https://techcombank.com/");
            return client;
        }
    }
}

