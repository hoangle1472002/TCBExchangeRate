namespace TCBExchangeRate.Application.Dtos
{
    public class ExchangeRateWrapper
    {
        public ExchangeRateData ExchangeRate { get; set; } = null!;
    }

    public class ExchangeRateData
    {
        public List<string>? UpdatedTimes { get; set; }
        public List<ExchangeRateItem> Data { get; set; } = new();
    }

    public class ExchangeRateItem
    {
        public string Label { get; set; } = null!;
        public string AskRate { get; set; } = null!;
        public string AskRateTM { get; set; } = null!;
        public string BidRateCK { get; set; } = null!;
        public string BidRateTM { get; set; } = null!;
    }
}