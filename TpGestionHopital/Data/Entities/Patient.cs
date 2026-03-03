using System.ComponentModel.DataAnnotations;
using TpGestionHopital.Data.Validation;

namespace TpGestionHopital.Data.Entities;

public class Patient
{
    public int Id { get; set; }
    [Required] 
    public string FirstName { get; set; } = null!;
    [Required] 
    public string LastName { get; set; } = null!;
    [Required, PastDate] 
    public DateTime BirthDate { get; set; }
    [Required] 
    public string FileNumber { get; set; } = null!;
    [Required, EmailAddress] 
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }

    // composite address value object owned by EF
    public Address? Address { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<HospitalStay> HospitalStays { get; set; } = new List<HospitalStay>();
}