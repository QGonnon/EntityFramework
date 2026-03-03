using Microsoft.AspNetCore.Mvc;
using TpGestionHopital.Data.Entities;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    public DoctorsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> GetAll(string? specialty, int page = 1, int pageSize = 20)
    {
        IEnumerable<Doctor> list;
        if (!string.IsNullOrEmpty(specialty))
        {
            list = await _unitOfWork.Doctors.SearchBySpecialtyAsync(specialty);
        }
        else
        {
            list = await _unitOfWork.Doctors.GetAllAsync();
        }
        var paged = list
            .OrderBy(d => d.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        return Ok(paged);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var doc = await _unitOfWork.Doctors.GetByIdAsync(id);
        return doc == null ? NotFound() : Ok(doc);
    }

    [HttpGet("{id}/consultations")]
    public async Task<IActionResult> GetWithConsultations(int id)
    {
        var doc = await _unitOfWork.Doctors.GetWithConsultationsAsync(id);
        return doc == null ? NotFound() : Ok(doc);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Doctor doctor)
    {
        await _unitOfWork.Doctors.AddAsync(doctor);
        await _unitOfWork.CompleteAsync();
        return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, doctor);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Doctor doctor)
    {
        if (id != doctor.Id) return BadRequest();
        var existing = await _unitOfWork.Doctors.GetByIdAsync(id);
        if (existing == null) return NotFound();
        _unitOfWork.Doctors.Update(doctor);
        await _unitOfWork.CompleteAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _unitOfWork.Doctors.GetByIdAsync(id);
        if (existing == null) return NotFound();
        _unitOfWork.Doctors.Delete(existing);
        await _unitOfWork.CompleteAsync();
        return NoContent();
    }
}