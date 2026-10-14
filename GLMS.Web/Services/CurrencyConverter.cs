namespace GLMS.Web.Services;

public static class CurrencyConverter
{
    public static decimal ConvertUsdToZar(decimal usdAmount, decimal zarPerUsd)
    {
        if (usdAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(usdAmount), "Amount cannot be negative.");
        }

        if (zarPerUsd <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(zarPerUsd), "Exchange rate must be greater than zero.");
        }

        return decimal.Round(usdAmount * zarPerUsd, 2, MidpointRounding.AwayFromZero);
    }
}