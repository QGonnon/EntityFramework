using TpGestionHopital.Data.Entities;

namespace TpGestionHopital.Data;

/// <summary>
/// Seeds the database with sample data for testing and demonstration.
/// </summary>
public class DataSeeder
{
    /// <summary>
    /// Seeds sample data if the database is empty.
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Only seed if database is empty
        if (context.Patients.Any() || context.Departments.Any())
            return;

        // Create Departments
        var cardiology = new Department
        {
            Name = "Cardiologie",
            Location = "Building A, Floor 3",
            Address = new Address
            {
                Street = "123 Medical St",
                City = "Paris",
                PostalCode = "75001",
                Country = "France"
            }
        };

        var neurology = new Department
        {
            Name = "Neurologie",
            Location = "Building B, Floor 2",
            Address = new Address
            {
                Street = "456 Hospital Ave",
                City = "Paris",
                PostalCode = "75002",
                Country = "France"
            }
        };

        var pediatrics = new Department
        {
            Name = "Pédiatrie",
            Location = "Building C, Floor 1",
            Address = new Address
            {
                Street = "789 Care Blvd",
                City = "Lyon",
                PostalCode = "69000",
                Country = "France"
            }
        };

        // Create sub-department hierarchy
        var pediatricCardiology = new Department
        {
            Name = "Cardiologie Pédiatrique",
            Location = "Building A, Floor 4",
            Address = new Address
            {
                Street = "123 Medical St",
                City = "Paris",
                PostalCode = "75001",
                Country = "France"
            },
            ParentDepartment = cardiology
        };

        await context.Departments.AddAsync(cardiology);
        await context.Departments.AddAsync(neurology);
        await context.Departments.AddAsync(pediatrics);
        await context.Departments.AddAsync(pediatricCardiology);
        await context.SaveChangesAsync();

        // Create Doctors
        var drSmith = new Doctor
        {
            FirstName = "Jean",
            LastName = "Dupont",
            Specialty = "Cardiologie",
            LicenseNumber = "MD-001-FR",
            DepartmentId = cardiology.Id
        };

        var drJones = new Doctor
        {
            FirstName = "Marie",
            LastName = "Martin",
            Specialty = "Neurologie",
            LicenseNumber = "MD-002-FR",
            DepartmentId = neurology.Id
        };

        var drBrown = new Doctor
        {
            FirstName = "Pierre",
            LastName = "Bernard",
            Specialty = "Pédiatrie",
            LicenseNumber = "MD-003-FR",
            DepartmentId = pediatrics.Id
        };

        var drWilson = new Doctor
        {
            FirstName = "Sophie",
            LastName = "Leclerc",
            Specialty = "Cardiologie Pédiatrique",
            LicenseNumber = "MD-004-FR",
            DepartmentId = pediatricCardiology.Id
        };

        // Set department managers
        cardiology.Manager = drSmith;
        cardiology.ManagerId = drSmith.Id;

        await context.Doctors.AddRangeAsync(drSmith, drJones, drBrown, drWilson);
        await context.SaveChangesAsync();

        // Create Patients
        var patient1 = new Patient
        {
            FirstName = "Alice",
            LastName = "Leblanc",
            BirthDate = new DateTime(1975, 3, 15),
            FileNumber = "PAT-001",
            Email = "alice.leblanc@email.com",
            PhoneNumber = "01 23 45 67 89",
            Address = new Address
            {
                Street = "100 Rue de la Paix",
                City = "Paris",
                PostalCode = "75003",
                Country = "France"
            }
        };

        var patient2 = new Patient
        {
            FirstName = "Bob",
            LastName = "Moreau",
            BirthDate = new DateTime(1982, 7, 22),
            FileNumber = "PAT-002",
            Email = "bob.moreau@email.com",
            PhoneNumber = "02 34 56 78 90",
            Address = new Address
            {
                Street = "200 Boulevard Saint-Germain",
                City = "Paris",
                PostalCode = "75005",
                Country = "France"
            }
        };

        var patient3 = new Patient
        {
            FirstName = "Claire",
            LastName = "Laurent",
            BirthDate = new DateTime(2010, 11, 5),
            FileNumber = "PAT-003",
            Email = "claire.laurent@email.com",
            PhoneNumber = "03 45 67 89 01",
            Address = new Address
            {
                Street = "300 Avenue des Champs",
                City = "Lyon",
                PostalCode = "69001",
                Country = "France"
            }
        };

        await context.Patients.AddRangeAsync(patient1, patient2, patient3);
        await context.SaveChangesAsync();

        // Create Consultations
        var consultation1 = new Consultation
        {
            PatientId = patient1.Id,
            DoctorId = drSmith.Id,
            Date = DateTime.Now.AddDays(5),
            Status = ConsultationStatus.Planned
        };

        var consultation2 = new Consultation
        {
            PatientId = patient2.Id,
            DoctorId = drJones.Id,
            Date = DateTime.Now.AddDays(3),
            Status = ConsultationStatus.Planned
        };

        var consultation3 = new Consultation
        {
            PatientId = patient3.Id,
            DoctorId = drWilson.Id,
            Date = DateTime.Now.AddDays(7),
            Status = ConsultationStatus.Planned
        };

        await context.Consultations.AddRangeAsync(consultation1, consultation2, consultation3);
        await context.SaveChangesAsync();

        // Create Hospital Stays
        var stay1 = new HospitalStay
        {
            PatientId = patient1.Id,
            DepartmentId = cardiology.Id,
            AdmissionDate = DateTime.Now.AddDays(-10),
            DischargeDate = DateTime.Now.AddDays(-5),
            Reason = "Suivi cardiaque"
        };

        var stay2 = new HospitalStay
        {
            PatientId = patient3.Id,
            DepartmentId = pediatricCardiology.Id,
            AdmissionDate = DateTime.Now.AddDays(-3),
            DischargeDate = null, // Still admitted
            Reason = "Évaluation pédiatrique"
        };

        await context.HospitalStays.AddRangeAsync(stay1, stay2);
        await context.SaveChangesAsync();

        // Create Medical Staff (Nurses and Administrators)
        var nurse1 = new Nurse
        {
            FirstName = "Isabelle",
            LastName = "Rousseau",
            HireDate = new DateTime(2020, 1, 15),
            Salary = 2500m,
            DepartmentId = cardiology.Id,
            Service = "Soins Intensifs",
            Grade = "Infirmière Senior"
        };

        var nurse2 = new Nurse
        {
            FirstName = "Thomas",
            LastName = "Petit",
            HireDate = new DateTime(2022, 6, 1),
            Salary = 2000m,
            DepartmentId = neurology.Id,
            Service = "Neurologie",
            Grade = "Infirmier Junior"
        };

        var admin1 = new Administrator
        {
            FirstName = "Catherine",
            LastName = "Durand",
            HireDate = new DateTime(2019, 3, 10),
            Salary = 2200m,
            DepartmentId = pediatrics.Id,
            Function = "Administratrice de Département"
        };

        await context.MedicalStaff.AddRangeAsync(nurse1, nurse2, admin1);
        await context.SaveChangesAsync();
    }
}
