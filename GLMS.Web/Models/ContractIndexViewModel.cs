namespace GLMS.Web.Models;

public sealed class ContractIndexViewModel
{
    public IReadOnlyList<Contract> Contracts { get; init; } = [];
    public ContractStatus? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}