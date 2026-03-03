using Microsoft.EntityFrameworkCore;
using TpGestionHopital.Data.Entities;

namespace TpGestionHopital.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<HospitalStay> HospitalStays => Set<HospitalStay>();
    public DbSet<MedicalStaff> MedicalStaff => Set<MedicalStaff>();
    public DbSet<Nurse> Nurses => Set<Nurse>();
    public DbSet<Administrator> Administrators => Set<Administrator>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unicité Patient
        modelBuilder.Entity<Patient>().HasIndex(p => p.FileNumber).IsUnique();
        modelBuilder.Entity<Patient>().HasIndex(p => p.Email).IsUnique();

        // Unicité département et médecin
        modelBuilder.Entity<Department>().HasIndex(d => d.Name).IsUnique();
        modelBuilder.Entity<Doctor>().HasIndex(d => d.LicenseNumber).IsUnique();

        // Relation Département -> Manager (Médecin)
        modelBuilder.Entity<Department>()
            .HasOne(d => d.Manager)
            .WithOne()
            .HasForeignKey<Department>(d => d.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Department hierarchy - self-referencing relationship
        modelBuilder.Entity<Department>()
            .HasOne(d => d.ParentDepartment)
            .WithMany(d => d.SubDepartments)
            .HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Explicit delete behavior for Doctor -> Department
        // If we delete a department, restrict deletion if it has doctors
        modelBuilder.Entity<Doctor>()
            .HasOne(d => d.Department)
            .WithMany(dept => dept.Doctors)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Explicit delete behavior for MedicalStaff -> Department
        modelBuilder.Entity<MedicalStaff>()
            .HasOne(ms => ms.Department)
            .WithMany(dept => dept.MedicalStaff)
            .HasForeignKey(ms => ms.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Contrainte : Pas deux consultations au même moment pour un patient avec le même médecin
        modelBuilder.Entity<Consultation>()
            .HasIndex(c => new { c.PatientId, c.DoctorId, c.Date })
            .IsUnique();

        // index to quickly find consultations for a doctor on a given date
        modelBuilder.Entity<Consultation>()
            .HasIndex(c => new { c.DoctorId, c.Date });

        // index for patient name search (Step 4/7 frequent query)
        modelBuilder.Entity<Patient>()
            .HasIndex(p => p.LastName);

        // index for patient consultation history ordered by date
        modelBuilder.Entity<Consultation>()
            .HasIndex(c => new { c.PatientId, c.Date });

        // configure concurrency token
        modelBuilder.Entity<Patient>()
            .Property(p => p.RowVersion)
            .IsRowVersion();

        // configure owned value object Address
        modelBuilder.Entity<Patient>().OwnsOne(p => p.Address);
        modelBuilder.Entity<Department>().OwnsOne(d => d.Address);

        // business rule: discharge date cannot be before admission date
        modelBuilder.Entity<HospitalStay>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_HospitalStay_DischargeAfterAdmission",
                "DischargeDate IS NULL OR DischargeDate >= AdmissionDate"));
    }
}