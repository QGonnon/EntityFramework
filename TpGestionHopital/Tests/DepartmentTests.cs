using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TpGestionHopital.Data;
using TpGestionHopital.Data.Entities;
using Xunit;

public class DepartmentTests
{
    private static DbContextOptions<ApplicationDbContext> CreateSqliteOptions(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        return options;
    }

    [Fact]
    public async Task CanCreateDepartmentWithHierarchy()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        var options = CreateSqliteOptions(connection);
        using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        
        var parentDept = new Department
        {
            Name = "Cardiologie",
            Location = "Building A"
        };
        
        await context.Departments.AddAsync(parentDept, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var childDept = new Department
        {
            Name = "Cardiologie Pédiatrique",
            Location = "Building A, Floor 2",
            ParentDepartmentId = parentDept.Id
        };
        
        await context.Departments.AddAsync(childDept, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var loaded = await context.Departments
            .Include(d => d.SubDepartments)
            .FirstAsync(d => d.Id == parentDept.Id, TestContext.Current.CancellationToken);
        
        Assert.Single(loaded.SubDepartments);
        Assert.Equal("Cardiologie Pédiatrique", loaded.SubDepartments.First().Name);
    }

    [Fact]
    public async Task CannotDeleteDepartmentWithDoctors()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        var options = CreateSqliteOptions(connection);
        using (var setupContext = new ApplicationDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            var dept = new Department { Name = "Test Department" };
            await setupContext.Departments.AddAsync(dept, TestContext.Current.CancellationToken);
            await setupContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var doctor = new Doctor
            {
                FirstName = "John",
                LastName = "Doe",
                Specialty = "General",
                LicenseNumber = "L-999",
                DepartmentId = dept.Id
            };
            await setupContext.Doctors.AddAsync(doctor, TestContext.Current.CancellationToken);
            await setupContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var deleteContext = new ApplicationDbContext(options);
        var persistedDept = await deleteContext.Departments
            .FirstAsync(d => d.Name == "Test Department", TestContext.Current.CancellationToken);

        deleteContext.Departments.Remove(persistedDept);

        await Assert.ThrowsAsync<DbUpdateException>(async () =>
            await deleteContext.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DepartmentStatisticsAreAccurate()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        var options = CreateSqliteOptions(connection);
        using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        
        var dept = new Department { Name = "Emergency" };
        await context.Departments.AddAsync(dept, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // Add 2 doctors
        var doctor1 = new Doctor
        {
            FirstName = "Alice",
            LastName = "Smith",
            Specialty = "Emergency",
            LicenseNumber = "L-001",
            DepartmentId = dept.Id
        };
        var doctor2 = new Doctor
        {
            FirstName = "Bob",
            LastName = "Jones",
            Specialty = "Emergency",
            LicenseNumber = "L-002",
            DepartmentId = dept.Id
        };
        
        await context.Doctors.AddRangeAsync(new[] { doctor1, doctor2 }, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var doctorCount = await context.Doctors
            .Where(d => d.DepartmentId == dept.Id)
            .CountAsync(TestContext.Current.CancellationToken);
        
        Assert.Equal(2, doctorCount);
    }
}
