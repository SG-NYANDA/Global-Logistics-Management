namespace GLMS.Web.Services;

public interface IExchangeRateProvider
{
    Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default);
}