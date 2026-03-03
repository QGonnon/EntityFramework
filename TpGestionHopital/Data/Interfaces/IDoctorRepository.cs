using TpGestionHopital.Data.Entities;

public interface IDoctorRepository : IRepository<Doctor>
{
    Task<IEnumerable<Doctor>> SearchBySpecialtyAsync(string specialty);
    Task<Doctor?> GetWithConsultationsAsync(int id);
}