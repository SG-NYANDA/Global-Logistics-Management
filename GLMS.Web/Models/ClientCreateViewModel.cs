using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models;

public sealed class ClientCreateViewModel
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string ContactDetails { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Region { get; set; } = string.Empty;
}