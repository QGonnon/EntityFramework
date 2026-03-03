using Microsoft.AspNetCore.Mvc;
using TpGestionHopital.Data.Entities;

[ApiController]
[Route("api/[controller]")]
public class ConsultationsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    public ConsultationsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _unitOfWork.Consultations.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _unitOfWork.Consultations.GetByIdAsync(id);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpGet("bypatient/{patientId}")]
    public async Task<IActionResult> GetByPatient(int patientId, DateTime? from = null, int page = 1, int pageSize = 20)
    {
        IEnumerable<Consultation> list;
        if (from.HasValue)
            list = await _unitOfWork.Consultations.GetUpcomingByPatientAsync(patientId, from.Value);
        else
            list = await _unitOfWork.Consultations.GetByPatientAsync(patientId);
        var paged = list.Skip((page - 1) * pageSize).Take(pageSize);
        return Ok(paged);
    }

    [HttpGet("bydoctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctor(int doctorId, DateTime? date = null, int page = 1, int pageSize = 20)
    {
        IEnumerable<Consultation> list;
        if (date.HasValue)
            list = await _unitOfWork.Consultations.GetByDoctorForDateAsync(doctorId, date.Value);
        else
            list = await _unitOfWork.Consultations.GetByDoctorAsync(doctorId);
        var paged = list.Skip((page - 1) * pageSize).Take(pageSize);
        return Ok(paged);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Consultation consultation)
    {
        // check for overlapping consultation
        var existingForPatient = await _unitOfWork.Consultations.GetByPatientAsync(consultation.PatientId);
        if (existingForPatient.Any(c => c.DoctorId == consultation.DoctorId && c.Date == consultation.Date))
            return BadRequest("There is already a consultation for this patient, doctor and date.");

        await _unitOfWork.Consultations.AddAsync(consultation);
        await _unitOfWork.CompleteAsync();
        return CreatedAtAction(nameof(GetById), new { id = consultation.Id }, consultation);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Consultation consultation)
    {
        if (id != consultation.Id) return BadRequest();
        var existing = await _unitOfWork.Consultations.GetByIdAsync(id);
        if (existing == null) return NotFound();
        var existingForPatient = await _unitOfWork.Consultations.GetByPatientAsync(consultation.PatientId);
        if (existingForPatient.Any(c => c.Id != id && c.DoctorId == consultation.DoctorId && c.Date == consultation.Date))
            return BadRequest("Another consultation conflicts with the same patient, doctor and date.");

        _unitOfWork.Consultations.Update(consultation);
        await _unitOfWork.CompleteAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _unitOfWork.Consultations.GetByIdAsync(id);
        if (existing == null) return NotFound();
        _unitOfWork.Consultations.Delete(existing);
        await _unitOfWork.CompleteAsync();
        return NoContent();
    }
}