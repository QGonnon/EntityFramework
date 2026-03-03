using System.ComponentModel.DataAnnotations;

namespace TpGestionHopital.Data.Entities;

public class Doctor
{
    public int Id { get; set; }
    [Required] 
    public string FirstName { get; set; } = null!;
    [Required] 
    public string LastName { get; set; } = null!;
    [Required] 
    public string Specialty { get; set; } = null!;
    [Required] 
    public string LicenseNumber { get; set; } = null!;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
}