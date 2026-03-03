using Microsoft.EntityFrameworkCore;
using TpGestionHopital.Data.Entities;
using TpGestionHopital.Data;

public class PatientRepository : IPatientRepository
{
    private readonly ApplicationDbContext _context;
    public PatientRepository(ApplicationDbContext context) => _context = context;

    /// <summary>
    /// Gets a patient by ID with all related entities (Consultations, HospitalStays, Address).
    /// </summary>
    public async Task<Patient?> GetByIdAsync(int id) =>
        await _context.Patients
            .Include(p => p.Consultations)
            .Include(p => p.HospitalStays)
            .FirstOrDefaultAsync(p => p.Id == id);

    /// <summary>
    /// Gets all patients with related Consultations and HospitalStays.
    /// </summary>
    public async Task<IEnumerable<Patient>> GetAllAsync() =>
        await _context.Patients
            .Include(p => p.Consultations)
            .Include(p => p.HospitalStays)
            .OrderBy(p => p.LastName)
            .ToListAsync();

    public async Task AddAsync(Patient entity) => await _context.Patients.AddAsync(entity);

    public void Update(Patient entity) => _context.Patients.Update(entity);

    public void Delete(Patient entity) => _context.Patients.Remove(entity);

    /// <summary>
    /// Searches patients by name with related entities included.
    /// </summary>
    public async Task<IEnumerable<Patient>> SearchByNameAsync(string name) =>
        await _context.Patients
            .Where(p => p.LastName.Contains(name))
            .Include(p => p.Consultations)
            .Include(p => p.HospitalStays)
            .ToListAsync();

    /// <summary>
    /// Gets a patient with all consultations (including doctors) and hospital stays included.
    /// </summary>
    public async Task<Patient?> GetPatientWithConsultationsAsync(int id) =>
        await _context.Patients
            .Include(p => p.Consultations)
                .ThenInclude(c => c.Doctor)
            .Include(p => p.HospitalStays)
            .FirstOrDefaultAsync(p => p.Id == id);
}