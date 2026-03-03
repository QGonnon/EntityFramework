using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TpGestionHopital.Data;
using TpGestionHopital.Data.Entities;
using Xunit;

public class ConsultationTests
{
    private static DbContextOptions<ApplicationDbContext> CreateSqliteOptions(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        return options;
    }

    [Fact]
    public async Task SchedulingDuplicateConsultationShouldBeDetected()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        var options = CreateSqliteOptions(connection);
        using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        // add patient and doctor
        var patient = new Patient
        {
            FirstName = "Bob",
            LastName = "Jones",
            BirthDate = new DateTime(1985, 6, 15),
            FileNumber = "F555",
            Email = "bob@example.com"
        };
        var dept = new Department { Name = "Test" };
        var doctor = new Doctor
        {
            FirstName = "Doc",
            LastName = "Tor",
            Specialty = "General",
            LicenseNumber = "L-123",
            Department = dept
        };
        await context.Patients.AddAsync(patient, TestContext.Current.CancellationToken);
        await context.Departments.AddAsync(dept, TestContext.Current.CancellationToken);
        await context.Doctors.AddAsync(doctor, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var date = DateTime.Today.AddHours(9);
        var consult1 = new Consultation
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            Date = date
        };
        await context.Consultations.AddAsync(consult1, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var consult2 = new Consultation
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            Date = date
        };
        await context.Consultations.AddAsync(consult2, TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
            await context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }
}
