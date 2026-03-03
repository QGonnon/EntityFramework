using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TpGestionHopital.Data;
using TpGestionHopital.Data.Entities;
using Xunit;

public class PatientTests
{
    private static DbContextOptions<ApplicationDbContext> CreateSqliteOptions(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        return options;
    }

    [Fact]
    public async Task CanCreatePatientAndEnforceConcurrency()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        var options = CreateSqliteOptions(connection);

        await using (var initContext = new ApplicationDbContext(options))
        {
            await initContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        }

        await using var context1 = new ApplicationDbContext(options);
        await using var context2 = new ApplicationDbContext(options);

        var patient = new Patient
        {
            FirstName = "Alice",
            LastName = "Smith",
            BirthDate = new DateTime(1990, 1, 1),
            FileNumber = "F123",
            Email = "alice@example.com"
        };
        await context1.Patients.AddAsync(patient, TestContext.Current.CancellationToken);
        await context1.SaveChangesAsync(TestContext.Current.CancellationToken);

        // load two copies in two distinct contexts to simulate concurrent edits
        var copy1 = await context1.Patients.FirstAsync(TestContext.Current.CancellationToken);
        var copy2 = await context2.Patients.FirstAsync(TestContext.Current.CancellationToken);

        // concurrent operation in context2 removes the entity
        context2.Patients.Remove(copy2);
        await context2.SaveChangesAsync(TestContext.Current.CancellationToken);

        // update stale entity in context1 - should trigger concurrency exception
        copy1.LastName = "Johnson";
        context1.Patients.Update(copy1);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            await context1.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void BirthDateMustBeInThePast()
    {
        var patient = new Patient
        {
            FirstName = "Future",
            LastName = "Baby",
            BirthDate = DateTime.Now.AddDays(1), // Future date
            FileNumber = "F-FUTURE",
            Email = "future@baby.com"
        };

        var validationContext = new ValidationContext(patient);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(patient, validationContext, validationResults, true);

        Assert.False(isValid);
        Assert.Contains(validationResults, vr => vr.ErrorMessage?.Contains("past") == true);
    }

    [Fact]
    public void BirthDateInPastIsValid()
    {
        var patient = new Patient
        {
            FirstName = "Valid",
            LastName = "Patient",
            BirthDate = new DateTime(1990, 5, 15),
            FileNumber = "F-VALID",
            Email = "valid@patient.com"
        };

        var validationContext = new ValidationContext(patient);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(patient, validationContext, validationResults, true);

        Assert.True(isValid);
    }
}