namespace TCBExchangeRate.Domain.Entities
{
    public class Currency
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = null!;
        public string? Name { get; set; }

        public ICollection<ExchangeRate> ExchangeRates { get; set; } = new List<ExchangeRate>();
    }
}

