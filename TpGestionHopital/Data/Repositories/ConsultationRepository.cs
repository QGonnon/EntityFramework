using Microsoft.EntityFrameworkCore;
using TpGestionHopital.Data;
using TpGestionHopital.Data.Entities;

public class ConsultationRepository : IConsultationRepository
{
    private readonly ApplicationDbContext _context;
    public ConsultationRepository(ApplicationDbContext context) => _context = context;

    /// <summary>
    /// Gets a consultation by ID with related Patient and Doctor.
    /// Uses Include to avoid null navigation properties.
    /// </summary>
    public async Task<Consultation?> GetByIdAsync(int id) =>
        await _context.Consultations
            .Include(c => c.Patient)
            .Include(c => c.Doctor)
            .FirstOrDefaultAsync(c => c.Id == id);

    /// <summary>
    /// Gets all consultations with related Patient and Doctor.
    /// </summary>
    public async Task<IEnumerable<Consultation>> GetAllAsync() =>
        await _context.Consultations
            .Include(c => c.Patient)
            .Include(c => c.Doctor)
            .OrderBy(c => c.Date)
            .ToListAsync();

    public async Task AddAsync(Consultation entity) => await _context.Consultations.AddAsync(entity);
    public void Update(Consultation entity) => _context.Consultations.Update(entity);
    public void Delete(Consultation entity) => _context.Consultations.Remove(entity);

    /// <summary>
    /// Gets consultations for a specific patient with related entities.
    /// </summary>
    public async Task<IEnumerable<Consultation>> GetByPatientAsync(int patientId) =>
        await _context.Consultations
            .Where(c => c.PatientId == patientId)
            .Include(c => c.Doctor)
            .Include(c => c.Patient)
            .OrderBy(c => c.Date)
            .ToListAsync();

    /// <summary>
    /// Gets consultations for a specific doctor with related entities.
    /// </summary>
    public async Task<IEnumerable<Consultation>> GetByDoctorAsync(int doctorId) =>
        await _context.Consultations
            .Where(c => c.DoctorId == doctorId)
            .Include(c => c.Patient)
            .Include(c => c.Doctor)
            .OrderBy(c => c.Date)
            .ToListAsync();

    /// <summary>
    /// Gets consultations for a doctor on a specific date (indexed query).
    /// </summary>
    public async Task<IEnumerable<Consultation>> GetByDoctorForDateAsync(int doctorId, DateTime date) =>
        await _context.Consultations
            .Where(c => c.DoctorId == doctorId && c.Date.Date == date.Date)
            .Include(c => c.Patient)
            .Include(c => c.Doctor)
            .OrderBy(c => c.Date)
            .ToListAsync();

    /// <summary>
    /// Gets upcoming consultations for a patient from a given date.
    /// </summary>
    public async Task<IEnumerable<Consultation>> GetUpcomingByPatientAsync(int patientId, DateTime from) =>
        await _context.Consultations
            .Where(c => c.PatientId == patientId && c.Date >= from)
            .Include(c => c.Doctor)
            .Include(c => c.Patient)
            .OrderBy(c => c.Date)
            .ToListAsync();
}