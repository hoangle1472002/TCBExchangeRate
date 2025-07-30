namespace TCBExchangeRate.Domain.Entities
{
    public class ExchangeRate
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SnapshotId { get; set; }
        public ExchangeRateSnapshot Snapshot { get; set; } = null!;

        public Guid CurrencyId { get; set; }
        public Currency Currency { get; set; } = null!;

        public decimal AskRate { get; set; }
        public decimal AskRateTM { get; set; }
        public decimal BidRateCK { get; set; }
        public decimal BidRateTM { get; set; }
    }
}

