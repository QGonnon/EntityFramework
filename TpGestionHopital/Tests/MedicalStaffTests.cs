using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TpGestionHopital.Data;
using TpGestionHopital.Data.Entities;
using Xunit;

public class MedicalStaffTests
{
    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CanCreateNurseWithInheritance()
    {
        using var context = CreateContext();
        
        var dept = new Department { Name = "Pediatrics" };
        await context.Departments.AddAsync(dept, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var nurse = new Nurse
        {
            FirstName = "Jane",
            LastName = "Williams",
            HireDate = new DateTime(2020, 1, 15),
            Salary = 45000,
            DepartmentId = dept.Id,
            Service = "Pediatric ICU",
            Grade = "Senior"
        };
        
        await context.Nurses.AddAsync(nurse, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var loaded = await context.Nurses.FirstAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Jane", loaded.FirstName);
        Assert.Equal("Senior", loaded.Grade);
        Assert.Equal("Pediatric ICU", loaded.Service);
    }

    [Fact]
    public async Task CanCreateAdministratorWithInheritance()
    {
        using var context = CreateContext();
        
        var dept = new Department { Name = "Administration" };
        await context.Departments.AddAsync(dept, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var admin = new Administrator
        {
            FirstName = "John",
            LastName = "Admin",
            HireDate = new DateTime(2019, 5, 1),
            Salary = 55000,
            DepartmentId = dept.Id,
            Function = "HR Manager"
        };
        
        await context.Administrators.AddAsync(admin, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var loaded = await context.Administrators.FirstAsync(TestContext.Current.CancellationToken);
        Assert.Equal("John", loaded.FirstName);
        Assert.Equal("HR Manager", loaded.Function);
    }

    [Fact]
    public async Task CanQueryAllMedicalStaffPolymorphically()
    {
        using var context = CreateContext();
        
        var dept = new Department { Name = "General" };
        await context.Departments.AddAsync(dept, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var nurse = new Nurse
        {
            FirstName = "Nurse",
            LastName = "One",
            HireDate = DateTime.Now.AddYears(-2),
            Salary = 40000,
            DepartmentId = dept.Id,
            Service = "General",
            Grade = "Junior"
        };
        
        var admin = new Administrator
        {
            FirstName = "Admin",
            LastName = "One",
            HireDate = DateTime.Now.AddYears(-3),
            Salary = 50000,
            DepartmentId = dept.Id,
            Function = "Coordinator"
        };
        
        await context.MedicalStaff.AddRangeAsync(new MedicalStaff[] { nurse, admin }, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var allStaff = await context.MedicalStaff.ToListAsync(TestContext.Current.CancellationToken);
        
        Assert.Equal(2, allStaff.Count);
        Assert.Contains(allStaff, s => s is Nurse);
        Assert.Contains(allStaff, s => s is Administrator);
    }
}
