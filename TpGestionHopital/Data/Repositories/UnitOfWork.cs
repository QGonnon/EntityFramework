using TpGestionHopital.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    public IPatientRepository Patients { get; private set; }
    public IDepartmentRepository Departments { get; private set; }
    public IDoctorRepository Doctors { get; private set; }
    public IConsultationRepository Consultations { get; private set; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Patients = new PatientRepository(_context);
        Departments = new DepartmentRepository(_context);
        Doctors = new DoctorRepository(_context);
        Consultations = new ConsultationRepository(_context);
    }

    public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
    public void Dispose() => _context.Dispose();
}