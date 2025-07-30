using System.Globalization;

namespace TCBExchangeRate.Application.Helpers
{
	public static class ParseHelper
	{
        public static decimal ParseDecimal(string? input) =>
            decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0;

        public static DateTime ParseSnapshotDateTime(DateOnly date, string time)
        {
            return DateTime.SpecifyKind(
                DateTime.ParseExact($"{date:yyyy-MM-dd} {time}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                DateTimeKind.Utc
            );
        }
    }
}

