using System.Text.Json.Serialization;

namespace TCBProxyApp.Models
{
    public class ExchangeRateData
    {
        public string ItemId { get; set; }
        public string Label { get; set; }
        public string AskRate { get; set; }
        public string BidRateCK { get; set; }
        public string BidRateTM { get; set; }
        public string AskRateTM { get; set; }
        public string SourceCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public string InputDate { get; set; }
    }

    public class ExchangeRateResult
    {
        public List<ExchangeRateData> Data { get; set; }
        public List<string> UpdatedTimes { get; set; }
    }

    public class RootExchangeRateResponse
    {
        [JsonPropertyName("exchangeRate")]
        public ExchangeRateResult ExchangeRate { get; set; }
    }
}

