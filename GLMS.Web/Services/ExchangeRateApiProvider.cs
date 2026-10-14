using System.Text.Json;

namespace GLMS.Web.Services;

public sealed class ExchangeRateApiProvider(HttpClient httpClient) : IExchangeRateProvider
{
    public async Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("latest/USD", cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!document.RootElement.TryGetProperty("rates", out var rates)
            || !rates.TryGetProperty("ZAR", out var rate)
            || !rate.TryGetDecimal(out var value)
            || value <= 0)
        {
            throw new InvalidDataException("The exchange-rate service returned no valid USD-to-ZAR rate.");
        }

        return value;
    }
}