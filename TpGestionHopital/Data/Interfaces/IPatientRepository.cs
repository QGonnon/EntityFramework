using TpGestionHopital.Data.Entities;

public interface IPatientRepository : IRepository<Patient>
{
    Task<IEnumerable<Patient>> SearchByNameAsync(string name);
    Task<Patient?> GetPatientWithConsultationsAsync(int id);
}