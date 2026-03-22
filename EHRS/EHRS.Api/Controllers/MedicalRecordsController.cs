using EHRS.Api.Contracts.MedicalRecords;
using EHRS.Api.Helpers;
using EHRS.Api.Localization;
using EHRS.Api.Services;
using EHRS.Core.Abstractions.Queries;
using EHRS.Core.Common;
using EHRS.Core.DTOs.MedicalRecords;
using EHRS.Core.Interfaces;
using EHRS.Core.Requests.MedicalRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EHRS.Api.Controllers
{
    [Authorize(Roles = "Doctor")]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordQueries _queries;
        private readonly IMedicalRecordService _service;
        private readonly IWebHostEnvironment _env;
        private readonly IAppLocalizer _loc;

        public MedicalRecordsController(
            IMedicalRecordQueries queries,
            IMedicalRecordService service,
            IWebHostEnvironment env,
            IAppLocalizer loc)
        {
            _queries = queries;
            _service = service;
            _env = env;
            _loc = loc;
        }

        // GET /api/MedicalRecords?page=1&pageSize=10&search=
        [HttpGet]
        public async Task<ActionResult<PagedResult<MedicalRecordListItemDto>>> Get(
            [FromQuery] MedicalRecordQuery query,
            CancellationToken ct)
        {
            var doctorId = ClaimsHelper.GetDoctorId(User);

            var result = await _queries.GetDoctorMedicalRecordsAsync(doctorId, query, ct);
            return Ok(result);
        }

        // GET /api/MedicalRecords/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MedicalRecordDetailsDto>> GetById(int id, CancellationToken ct)
        {
            var record = await _queries.GetByIdAsync(id, ct);
            if (record is null)
                return NotFound(new { message = _loc["MedicalRecords_RecordNotFound"] });

            return Ok(record);
        }

        // GET /api/MedicalRecords/by-appointment/{appointmentId}
        [HttpGet("by-appointment/{appointmentId:int}")]
        public async Task<ActionResult<MedicalRecordDetailsDto>> GetByAppointment(int appointmentId, CancellationToken ct)
        {
            var record = await _queries.GetByAppointmentAsync(appointmentId, ct);
            if (record is null)
                return NotFound(new { message = _loc["MedicalRecords_RecordNotFound"] });

            return Ok(record);
        }

        // GET /api/MedicalRecords/by-patient/{patientId}
        [HttpGet("by-patient/{patientId:int}")]
        public async Task<ActionResult<List<MedicalRecordDetailsDto>>> GetByPatient(int patientId, CancellationToken ct)
        {
            var doctorId = ClaimsHelper.GetDoctorId(User);

            var records = await _queries.GetByPatientAsync(doctorId, patientId, ct);

            if (records is null || records.Count == 0)
                return NotFound(new { message = _loc["MedicalRecords_RecordNotFound"] });

            return Ok(records);
        }

        // POST /api/MedicalRecords 
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<MedicalRecordDetailsDto>> Create(
            [FromForm] CreateMedicalRecordForm form,
            CancellationToken ct)
        {
             var doctorId = ClaimsHelper.GetDoctorId(User);
            
            try
            {
                // 1) Create Medical Record
                var request = new CreateMedicalRecordRequest
                {
                    PatientId = form.PatientId,
                    AppointmentId = form.AppointmentId,
                    RecordDateTime = form.RecordDateTime,
                    ChiefComplaint = form.ChiefComplaint,
                    Diagnosis = form.Diagnosis,
                    ClinicalNotes = form.ClinicalNotes,
                    Treatment = form.Treatment,
                    Radiology = null,
                    PrescriptionImagePath = null
                };

                var created = await _service.CreateAsync(doctorId, request, ct);
                if (form.PrescriptionFile is not null && form.PrescriptionFile.Length > 0)
                {
                    await using var stream = form.PrescriptionFile.OpenReadStream();

                    await _service.UploadPrescriptionAsync(
                        created.RecordId,
                        stream,
                        form.PrescriptionFile.FileName,
                        _env.WebRootPath,
                        ct);
                }
                if (form.RadiologyFile is not null && form.RadiologyFile.Length > 0)
                {
                    await using var stream = form.RadiologyFile.OpenReadStream();

                    await _service.UploadRadiologyAsync(
                        created.RecordId,
                        stream,
                        form.RadiologyFile.FileName,
                        _env.WebRootPath,
                        ct);
                }
                var finalRecord = await _queries.GetByIdAsync(created.RecordId, ct);
                if (finalRecord is null)
                    return CreatedAtAction(nameof(GetById), new { id = created.RecordId }, created);

                return CreatedAtAction(nameof(GetById), new { id = finalRecord.RecordId }, finalRecord);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = _loc["MedicalRecords_RelatedEntityNotFound"] });
            }
            catch (ArgumentException)
            {
                return BadRequest(new { message = _loc["MedicalRecords_InvalidData"] });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { message = _loc["MedicalRecords_InvalidOperation"] });
            }
            catch
            {
                return StatusCode(500, new { message = _loc["Common_UnexpectedError"] });
            }
        }
    }
}
