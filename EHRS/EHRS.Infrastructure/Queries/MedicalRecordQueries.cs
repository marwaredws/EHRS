using EHRS.Core.Abstractions.Queries;
using EHRS.Core.Common;
using EHRS.Core.DTOs.MedicalRecords;
using EHRS.Core.Requests.MedicalRecords;
using EHRS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EHRS.Infrastructure.Queries
{
    public sealed class MedicalRecordQueries : IMedicalRecordQueries
    {
        private readonly EHRSContext _db;

        public MedicalRecordQueries(EHRSContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<MedicalRecordListItemDto>> GetDoctorMedicalRecordsAsync(
            int doctorId,
            MedicalRecordQuery query,
            CancellationToken ct)
        {
            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize < 1 ? 10 : query.PageSize;

            var q = _db.MedicalRecords
                .AsNoTracking()
                .Where(r => r.DoctorId == doctorId);

            if (query.DateFrom.HasValue)
                q = q.Where(r => r.RecordDateTime >= query.DateFrom.Value);

            if (query.DateTo.HasValue)
                q = q.Where(r => r.RecordDateTime <= query.DateTo.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim();

                
                q = q.Where(r => r.Patient.FullName.Contains(s));

               
            }
            var totalCount = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(r => r.RecordDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new MedicalRecordListItemDto
                {
                    RecordId = r.RecordId,
                    PatientId = r.PatientId,
                    PatientName = r.Patient.FullName,
                    RecordDateTime = r.RecordDateTime,
                    Diagnosis = r.Diagnosis,
                    Treatment = r.Treatment
                })
                .ToListAsync(ct);

            return new PagedResult<MedicalRecordListItemDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        public async Task<List<MedicalRecordDetailsDto>> GetByPatientAsync(
    int doctorId,
    int patientId,
    CancellationToken ct)
        {
            return await _db.MedicalRecords
                .AsNoTracking()
                .Where(r => r.PatientId == patientId && r.DoctorId == doctorId)
                .OrderByDescending(r => r.RecordDateTime)
                .Select(r => new MedicalRecordDetailsDto
                {
                    RecordId = r.RecordId,
                    PatientId = r.PatientId,
                    PatientName = r.Patient.FullName,
                    DoctorId = r.DoctorId,
                    AppointmentId = r.AppointmentId,
                    RecordDateTime = r.RecordDateTime,
                    ChiefComplaint = r.ChiefComplaint,
                    Diagnosis = r.Diagnosis,
                    ClinicalNotes = r.ClinicalNotes,
                    Treatment = r.Treatment,
                    Radiology = r.Radiology,
                    PrescriptionImagePath = r.PrescriptionImagePath
                })
                .ToListAsync(ct);
        }

        public async Task<MedicalRecordDetailsDto?> GetByIdAsync(int recordId, CancellationToken ct)
        {
            return await _db.MedicalRecords
                .AsNoTracking()
                .Where(r => r.RecordId == recordId)
                .Select(r => new MedicalRecordDetailsDto
                {
                    RecordId = r.RecordId,
                    PatientId = r.PatientId,
                    PatientName = r.Patient.FullName,
                    DoctorId = r.DoctorId,
                    AppointmentId = r.AppointmentId,
                    RecordDateTime = r.RecordDateTime,
                    ChiefComplaint = r.ChiefComplaint,
                    Diagnosis = r.Diagnosis,
                    ClinicalNotes = r.ClinicalNotes,
                    Treatment = r.Treatment,

                    //  FIX: include Radiology
                    Radiology = r.Radiology,

                    PrescriptionImagePath = r.PrescriptionImagePath
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<MedicalRecordDetailsDto?> GetByAppointmentAsync(int appointmentId, CancellationToken ct)
        {
            return await _db.MedicalRecords
                .AsNoTracking()
                .Where(r => r.AppointmentId == appointmentId)
                .Select(r => new MedicalRecordDetailsDto
                {
                    RecordId = r.RecordId,
                    PatientId = r.PatientId,
                    PatientName = r.Patient.FullName,
                    DoctorId = r.DoctorId,
                    AppointmentId = r.AppointmentId,
                    RecordDateTime = r.RecordDateTime,
                    ChiefComplaint = r.ChiefComplaint,
                    Diagnosis = r.Diagnosis,
                    ClinicalNotes = r.ClinicalNotes,
                    Treatment = r.Treatment,
                    Radiology = r.Radiology,

                    PrescriptionImagePath = r.PrescriptionImagePath
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
