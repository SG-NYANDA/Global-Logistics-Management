using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Tests;

public sealed class BusinessRuleTests
{
    [Fact]
    public void ConvertsUsdToZarAndRoundsToCents()
    {
        Assert.Equal(191.42m, CurrencyConverter.ConvertUsdToZar(10.5m, 18.23m));
    }

    [Fact]
    public void ConvertsZeroAmountToZero()
    {
        Assert.Equal(0m, CurrencyConverter.ConvertUsdToZar(0m, 18.23m));
    }

    [Theory]
    [InlineData(-1, 18.23)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void RejectsInvalidCurrencyInputs(decimal amount, decimal rate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CurrencyConverter.ConvertUsdToZar(amount, rate));
    }

    [Fact]
    public void RejectsRestrictedFileExtension()
    {
        using var content = new MemoryStream("%PDF-valid"u8.ToArray());

        Assert.Throws<InvalidDataException>(() => PdfAgreementValidator.Validate("malware.exe", content));
    }

    [Fact]
    public void RejectsPdfExtensionWithNonPdfContent()
    {
        using var content = new MemoryStream("not a pdf"u8.ToArray());

        Assert.Throws<InvalidDataException>(() => PdfAgreementValidator.Validate("agreement.pdf", content));
    }

    [Fact]
    public void AllowsRequestForActiveContractWithinDates()
    {
        var contract = new Contract
        {
            Status = ContractStatus.Active,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31)
        };

        Assert.True(ContractWorkflow.CanCreateServiceRequest(contract, new DateTime(2026, 10, 13)));
    }

    [Theory]
    [InlineData(ContractStatus.Draft)]
    [InlineData(ContractStatus.Expired)]
    [InlineData(ContractStatus.OnHold)]
    public void RejectsRequestForNonActiveContract(ContractStatus status)
    {
        var contract = new Contract
        {
            Status = status,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31)
        };

        Assert.False(ContractWorkflow.CanCreateServiceRequest(contract, new DateTime(2026, 10, 13)));
    }
}