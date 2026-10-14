using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models;

public sealed class Contract
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Reference { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string ServiceLevel { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required]
    public string AgreementFileName { get; set; } = string.Empty;

    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}