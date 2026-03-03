using System.ComponentModel.DataAnnotations;

namespace TpGestionHopital.Data.Entities;

public class Department
{
    public int Id { get; set; }
    [Required] 
    public string Name { get; set; } = null!;
    public string? Location { get; set; }

    // use address value object for department contact
    public Address? Address { get; set; }

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public ICollection<HospitalStay> HospitalStays { get; set; } = new List<HospitalStay>();
    public ICollection<MedicalStaff> MedicalStaff { get; set; } = new List<MedicalStaff>();

    // Department hierarchy
    public int? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }
    public ICollection<Department> SubDepartments { get; set; } = new List<Department>();

    public int? ManagerId { get; set; }
    public Doctor? Manager { get; set; }
}