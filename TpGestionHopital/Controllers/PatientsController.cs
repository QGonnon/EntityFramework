using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpGestionHopital.Data.Entities;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public PatientsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> GetAll(string? name, int page = 1, int pageSize = 20)
    {
        IEnumerable<Patient> list;
        if (!string.IsNullOrEmpty(name))
        {
            list = await _unitOfWork.Patients.SearchByNameAsync(name);
        }
        else
        {
            list = await _unitOfWork.Patients.GetAllAsync();
        }
        var paged = list
            .OrderBy(p => p.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        return Ok(paged);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(id);
        return patient == null ? NotFound() : Ok(patient);
    }

    [HttpGet("{id}/dashboard")]
    public async Task<IActionResult> GetDashboard(int id)
    {
        var patient = await _unitOfWork.Patients.GetPatientWithConsultationsAsync(id);
        return patient == null ? NotFound() : Ok(patient);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Patient patient)
    {
        await _unitOfWork.Patients.AddAsync(patient);
        await _unitOfWork.CompleteAsync();
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Patient patient)
    {
        if (id != patient.Id)
            return BadRequest();

        var existing = await _unitOfWork.Patients.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        _unitOfWork.Patients.Update(patient);
        try
        {
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("The patient record was modified by another user.");
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(id);
        if (patient == null)
            return NotFound();

        _unitOfWork.Patients.Delete(patient);
        await _unitOfWork.CompleteAsync();
        return NoContent();
    }
}