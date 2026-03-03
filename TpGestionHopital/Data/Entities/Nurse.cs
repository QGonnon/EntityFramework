using System.ComponentModel.DataAnnotations;

namespace TpGestionHopital.Data.Entities;

public class Nurse : MedicalStaff
{
    [Required]
    public string Service { get; set; } = null!;
    
    [Required]
    public string Grade { get; set; } = null!;
}
