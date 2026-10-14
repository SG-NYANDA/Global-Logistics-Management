namespace GLMS.Web.Models;

public sealed class DashboardViewModel
{
    public int ClientCount { get; init; }
    public int ActiveContractCount { get; init; }
    public int OpenRequestCount { get; init; }
    public IReadOnlyList<ServiceRequest> RecentRequests { get; init; } = [];
}