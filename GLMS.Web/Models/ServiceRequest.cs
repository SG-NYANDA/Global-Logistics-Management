using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models;

public sealed class ServiceRequest
{
    public int Id { get; set; }

    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal UsdCost { get; set; }

    [Range(typeof(decimal), "0.000001", "999999")]
    public decimal ExchangeRate { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal LocalCost { get; set; }

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Submitted;
    public DateTime CreatedUtc { get; set; }
    public int ContractId { get; set; }
    public Contract? Contract { get; set; }
}