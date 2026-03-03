using Microsoft.EntityFrameworkCore;
using TpGestionHopital.Data;
using TpGestionHopital.Data.Entities;

public class DoctorRepository : IDoctorRepository
{
    private readonly ApplicationDbContext _context;
    public DoctorRepository(ApplicationDbContext context) => _context = context;

    /// <summary>
    /// Gets a doctor by ID with related Department and Consultations.
    /// </summary>
    public async Task<Doctor?> GetByIdAsync(int id) =>
        await _context.Doctors
            .Include(d => d.Department)
            .Include(d => d.Consultations)
            .FirstOrDefaultAsync(d => d.Id == id);

    /// <summary>
    /// Gets all doctors with related Department and Consultations.
    /// </summary>
    public async Task<IEnumerable<Doctor>> GetAllAsync() =>
        await _context.Doctors
            .Include(d => d.Department)
            .Include(d => d.Consultations)
            .OrderBy(d => d.LastName)
            .ToListAsync();

    public async Task AddAsync(Doctor entity) => await _context.Doctors.AddAsync(entity);
    public void Update(Doctor entity) => _context.Doctors.Update(entity);
    public void Delete(Doctor entity) => _context.Doctors.Remove(entity);

    /// <summary>
    /// Searches doctors by specialty with related entities included.
    /// </summary>
    public async Task<IEnumerable<Doctor>> SearchBySpecialtyAsync(string specialty) =>
        await _context.Doctors
            .Where(d => d.Specialty.Contains(specialty))
            .Include(d => d.Department)
            .Include(d => d.Consultations)
            .ToListAsync();

    /// <summary>
    /// Gets a doctor with all consultations and patients included.
    /// </summary>
    public async Task<Doctor?> GetWithConsultationsAsync(int id) =>
        await _context.Doctors
            .Include(d => d.Department)
            .Include(d => d.Consultations)
            .ThenInclude(c => c.Patient)
            .FirstOrDefaultAsync(d => d.Id == id);
}