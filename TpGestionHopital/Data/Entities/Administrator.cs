using System.ComponentModel.DataAnnotations;

namespace TpGestionHopital.Data.Entities;

public class Administrator : MedicalStaff
{
    [Required]
    public string Function { get; set; } = null!;
}
