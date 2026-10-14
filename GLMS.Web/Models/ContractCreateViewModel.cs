using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Models;

public sealed class ContractCreateViewModel
{
    [Required, StringLength(100)]
    public string Reference { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string ServiceLevel { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);

    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Range(1, int.MaxValue)]
    public int ClientId { get; set; }

    public IFormFile? Agreement { get; set; }
    public IEnumerable<SelectListItem> Clients { get; set; } = [];
}