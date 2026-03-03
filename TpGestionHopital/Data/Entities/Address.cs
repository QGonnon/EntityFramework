using System.ComponentModel.DataAnnotations;

namespace TpGestionHopital.Data.Entities;

// simple value object representing an address
public class Address
{
    [Required]
    public string Street { get; set; } = null!;
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}
