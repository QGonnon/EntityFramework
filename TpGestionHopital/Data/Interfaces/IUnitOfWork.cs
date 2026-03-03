public interface IUnitOfWork : IDisposable
{
    IPatientRepository Patients { get; }
    IDepartmentRepository Departments { get; }
    IDoctorRepository Doctors { get; }
    IConsultationRepository Consultations { get; }
    Task<int> CompleteAsync();
}