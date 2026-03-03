using Microsoft.AspNetCore.Mvc;
using TpGestionHopital.Data.Entities;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    public DepartmentsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20)
    {
        var all = await _unitOfWork.Departments.GetAllAsync();
        var paged = all
            .OrderBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        return Ok(paged);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dept = await _unitOfWork.Departments.GetByIdAsync(id);
        return dept == null ? NotFound() : Ok(dept);
    }

    [HttpGet("{id}/doctors")]
    public async Task<IActionResult> GetWithDoctors(int id)
    {
        var dept = await _unitOfWork.Departments.GetWithDoctorsAsync(id);
        return dept == null ? NotFound() : Ok(dept);
    }

    [HttpGet("{id}/statistics")]
    public async Task<IActionResult> GetStatistics(int id)
    {
        var stats = await _unitOfWork.Departments.GetStatisticsAsync(id);
        return Ok(stats);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Department department)
    {
        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.CompleteAsync();
        return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Department department)
    {
        if (id != department.Id)
            return BadRequest();
        var existing = await _unitOfWork.Departments.GetByIdAsync(id);
        if (existing == null) return NotFound();
        _unitOfWork.Departments.Update(department);
        await _unitOfWork.CompleteAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _unitOfWork.Departments.GetByIdAsync(id);
        if (existing == null) return NotFound();
        _unitOfWork.Departments.Delete(existing);
        await _unitOfWork.CompleteAsync();
        return NoContent();
    }
}