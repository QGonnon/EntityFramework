using Microsoft.EntityFrameworkCore;
using TpGestionHopital.Data;
using TpGestionHopital.Data.Entities;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;
    public DepartmentRepository(ApplicationDbContext context) => _context = context;

    /// <summary>
    /// Gets a department by ID with all related entities loaded.
    /// Includes: Doctors, Manager, MedicalStaff, SubDepartments, ParentDepartment.
    /// </summary>
    public async Task<Department?> GetByIdAsync(int id) =>
        await _context.Departments
            .Include(d => d.Doctors)
            .Include(d => d.Manager)
            .Include(d => d.MedicalStaff)
            .Include(d => d.SubDepartments)
            .Include(d => d.ParentDepartment)
            .FirstOrDefaultAsync(d => d.Id == id);

    /// <summary>
    /// Gets all departments with related doctors and hierarchy.
    /// </summary>
    public async Task<IEnumerable<Department>> GetAllAsync() =>
        await _context.Departments
            .Include(d => d.Doctors)
            .Include(d => d.Manager)
            .Include(d => d.MedicalStaff)
            .Include(d => d.SubDepartments)
            .Include(d => d.ParentDepartment)
            .OrderBy(d => d.Name)
            .ToListAsync();

    public async Task AddAsync(Department entity) => await _context.Departments.AddAsync(entity);
    public void Update(Department entity) => _context.Departments.Update(entity);
    public void Delete(Department entity) => _context.Departments.Remove(entity);

    /// <summary>
    /// Gets a department with all doctors included.
    /// </summary>
    public async Task<Department?> GetWithDoctorsAsync(int id) =>
        await _context.Departments
            .Include(d => d.Doctors)
            .Include(d => d.Manager)
            .Include(d => d.SubDepartments)
            .Include(d => d.MedicalStaff)
            .FirstOrDefaultAsync(d => d.Id == id);

    /// <summary>
    /// Gets department statistics: doctor count and consultation count.
    /// </summary>
    public async Task<DepartmentStatistics> GetStatisticsAsync(int departmentId)
    {
        var docCount = await _context.Doctors
            .Where(d => d.DepartmentId == departmentId)
            .CountAsync();
        var consultCount = await _context.Consultations
            .Where(c => c.Doctor.DepartmentId == departmentId)
            .CountAsync();
        return new DepartmentStatistics { DoctorCount = docCount, ConsultationCount = consultCount };
    }
}