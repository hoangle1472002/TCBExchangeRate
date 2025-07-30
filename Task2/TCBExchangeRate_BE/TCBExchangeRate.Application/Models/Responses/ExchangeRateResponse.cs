namespace TCBExchangeRate.Application.Models.Responses
{
	public class ExchangeRateResponse
	{
        public required string CurrencyCode { get; set; }
        public DateTime SnapshotTime { get; set; }

        public decimal? PurchaseCashCheque { get; set; }
        public decimal? PurchaseTransfer { get; set; }
        public decimal? SellingCashCheque { get; set; }
        public decimal? SellingTransfer { get; set; }
    }
}
