using System.ComponentModel.DataAnnotations;

namespace TpGestionHopital.Data.Entities;

// Base class for all medical personnel
public abstract class MedicalStaff
{
    public int Id { get; set; }
    
    [Required]
    public string FirstName { get; set; } = null!;
    
    [Required]
    public string LastName { get; set; } = null!;
    
    [Required]
    public DateTime HireDate { get; set; }
    
    public decimal Salary { get; set; }
    
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
}
