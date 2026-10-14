using GLMS.Web.Models;

namespace GLMS.Web.Services;

public static class ContractWorkflow
{
    public static ContractStatus GetEffectiveStatus(Contract contract, DateTime today)
    {
        return contract.Status == ContractStatus.Active && contract.EndDate.Date < today.Date
            ? ContractStatus.Expired
            : contract.Status;
    }

    public static bool CanCreateServiceRequest(Contract contract, DateTime today)
    {
        return contract.Status == ContractStatus.Active
            && contract.StartDate.Date <= today.Date
            && contract.EndDate.Date >= today.Date;
    }
}