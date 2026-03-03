using System.ComponentModel.DataAnnotations;

namespace TpGestionHopital.Data.Entities;

public class HospitalStay
{
    public int Id { get; set; }
    
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    
    [Required]
    public DateTime AdmissionDate { get; set; }
    
    public DateTime? DischargeDate { get; set; }
    
    public string? Reason { get; set; }
    
    public string? Notes { get; set; }
}
