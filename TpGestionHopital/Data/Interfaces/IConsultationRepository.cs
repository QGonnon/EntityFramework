using TpGestionHopital.Data.Entities;

public interface IConsultationRepository : IRepository<Consultation>
{
    Task<IEnumerable<Consultation>> GetByPatientAsync(int patientId);
    Task<IEnumerable<Consultation>> GetByDoctorAsync(int doctorId);

    // additional queries for reporting
    Task<IEnumerable<Consultation>> GetByDoctorForDateAsync(int doctorId, DateTime date);
    Task<IEnumerable<Consultation>> GetUpcomingByPatientAsync(int patientId, DateTime from);
}