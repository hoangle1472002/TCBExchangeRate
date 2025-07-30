namespace TCBExchangeRate.Domain.Entities
{
    public class ExchangeRateSnapshot
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime SnapshotDateTime { get; set; }

        public ICollection<ExchangeRate> ExchangeRates { get; set; } = new List<ExchangeRate>();
    }
}