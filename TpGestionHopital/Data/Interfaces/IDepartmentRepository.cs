using TpGestionHopital.Data.Entities;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<Department?> GetWithDoctorsAsync(int id);

    // statistics for department
    Task<DepartmentStatistics> GetStatisticsAsync(int departmentId);
}