using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Models;

public sealed class ServiceRequestCreateViewModel
{
    [Range(1, int.MaxValue)]
    public int ContractId { get; set; }

    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal UsdCost { get; set; }

    public IEnumerable<SelectListItem> Contracts { get; set; } = [];
}