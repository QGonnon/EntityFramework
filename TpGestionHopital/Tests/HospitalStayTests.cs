using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TpGestionHopital.Data;
using TpGestionHopital.Data.Entities;
using Xunit;

public class HospitalStayTests
{
    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CanCreateHospitalStay()
    {
        using var context = CreateContext();
        
        var patient = new Patient
        {
            FirstName = "Test",
            LastName = "Patient",
            BirthDate = new DateTime(1980, 5, 15),
            FileNumber = "F-STAY-001",
            Email = "stay@test.com"
        };
        
        var dept = new Department { Name = "Intensive Care" };
        
        await context.Patients.AddAsync(patient, TestContext.Current.CancellationToken);
        await context.Departments.AddAsync(dept, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var stay = new HospitalStay
        {
            PatientId = patient.Id,
            DepartmentId = dept.Id,
            AdmissionDate = DateTime.Now.AddDays(-5),
            Reason = "Emergency admission",
            Notes = "Patient stable"
        };
        
        await context.HospitalStays.AddAsync(stay, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var loaded = await context.HospitalStays
            .Include(s => s.Patient)
            .Include(s => s.Department)
            .FirstAsync(TestContext.Current.CancellationToken);
        
        Assert.Equal(patient.Id, loaded.PatientId);
        Assert.Equal(dept.Id, loaded.DepartmentId);
        Assert.Equal("Emergency admission", loaded.Reason);
        Assert.Null(loaded.DischargeDate);
    }

    [Fact]
    public async Task CanTrackMultipleStaysForPatient()
    {
        using var context = CreateContext();
        
        var patient = new Patient
        {
            FirstName = "Multiple",
            LastName = "Stays",
            BirthDate = new DateTime(1975, 3, 20),
            FileNumber = "F-MULTI-001",
            Email = "multi@stays.com"
        };
        
        var dept1 = new Department { Name = "Cardiology" };
        var dept2 = new Department { Name = "Neurology" };
        
        await context.Patients.AddAsync(patient, TestContext.Current.CancellationToken);
        await context.Departments.AddRangeAsync(new[] { dept1, dept2 }, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var stay1 = new HospitalStay
        {
            PatientId = patient.Id,
            DepartmentId = dept1.Id,
            AdmissionDate = DateTime.Now.AddMonths(-6),
            DischargeDate = DateTime.Now.AddMonths(-6).AddDays(10),
            Reason = "Heart check"
        };
        
        var stay2 = new HospitalStay
        {
            PatientId = patient.Id,
            DepartmentId = dept2.Id,
            AdmissionDate = DateTime.Now.AddMonths(-2),
            DischargeDate = DateTime.Now.AddMonths(-2).AddDays(5),
            Reason = "Brain injury"
        };
        
        await context.HospitalStays.AddRangeAsync(new[] { stay1, stay2 }, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var patientWithStays = await context.Patients
            .Include(p => p.HospitalStays)
            .FirstAsync(p => p.Id == patient.Id, TestContext.Current.CancellationToken);
        
        Assert.Equal(2, patientWithStays.HospitalStays.Count);
    }
}
