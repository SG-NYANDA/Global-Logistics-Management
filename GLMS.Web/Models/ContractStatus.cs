using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models;

public enum ContractStatus
{
    Draft,
    Active,
    Expired,
    [Display(Name = "On Hold")]
    OnHold
}