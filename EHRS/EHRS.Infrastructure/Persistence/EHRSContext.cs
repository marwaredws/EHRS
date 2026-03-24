using System;
using System.Collections.Generic;
using EHRS.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // 👈 
using EHRS.Core.Interfaces; // 👈 

namespace EHRS.Infrastructure.Persistence;

public partial class EHRSContext : DbContext
{
    private readonly IEncryptionService? _encryptionService; // 👈  (nullable)

    public EHRSContext()
    {
    }

    public EHRSContext(DbContextOptions<EHRSContext> options)
        : base(options)
    {
    }

    // 👇 كونستركتور جديد عشان التشفير
    public EHRSContext(DbContextOptions<EHRSContext> options, IEncryptionService encryptionService)
        : base(options)
    {
        _encryptionService = encryptionService;
    }

    public virtual DbSet<Appointment> Appointments { get; set; }
    public virtual DbSet<Doctor> Doctors { get; set; }
    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }
    public virtual DbSet<Patient> Patients { get; set; }
    public virtual DbSet<SensorDatum> SensorData { get; set; }
    public virtual DbSet<SurgeryHistory> SurgeryHistories { get; set; }
    public virtual DbSet<UserCredential> UserCredentials { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=EHR_Wearable_DB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 👇 لو في خدمة تشفير (يعني في وضع التشغيل العادي)
        if (_encryptionService != null)
        {
            var converter = new ValueConverter<string, string>(
                v => _encryptionService.Encrypt(v ?? string.Empty),
                v => _encryptionService.Decrypt(v ?? string.Empty)
            );

            // ------------------- إعدادات التشفير -------------------
            // 🔐 تشفير بيانات الأطباء
            modelBuilder.Entity<Doctor>(entity =>
            {
                // الحقول الحساسة
               // entity.Property(e => e.FullName).HasConversion(converter);
               // entity.Property(e => e.Email).HasConversion(converter);
              //  entity.Property(e => e.ContactNumber).HasConversion(converter);
                // الباقي زي ما هو (About, Area, Certificates, ...)
            });

            // 🔐 تشفير السجلات الطبية
            modelBuilder.Entity<MedicalRecord>(entity =>
            {
                entity.Property(e => e.ChiefComplaint).HasConversion(converter);
                entity.Property(e => e.Diagnosis).HasConversion(converter);
                entity.Property(e => e.ClinicalNotes).HasConversion(converter);
                entity.Property(e => e.Treatment).HasConversion(converter);
               // entity.Property(e => e.Radiology).HasConversion(converter);
                // PrescriptionImagePath مسار مش بنشفره
            });

            // 🔐 تشفير بيانات المرضى
            modelBuilder.Entity<Patient>(entity =>
            {
               // entity.Property(e => e.FullName).HasConversion(converter);
               // entity.Property(e => e.Ssn).HasConversion(converter); // SSN حساس
                entity.Property(e => e.Address).HasConversion(converter);
              //  entity.Property(e => e.ContactNumber).HasConversion(converter);
             //   entity.Property(e => e.Email).HasConversion(converter);
                // الباقي زي ما هو (BloodType, Gender, WeightKg, ...)
            });
        }

        // --------------------------------------
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC2783685B3");

            entity.ToTable("Appointment");

            entity.HasIndex(e => new { e.DoctorId, e.AppointmentDateTime }, "IX_Appointment_DoctorDate").IsDescending(false, true);

            entity.HasIndex(e => new { e.PatientId, e.AppointmentDateTime }, "IX_Appointment_PatientDate").IsDescending(false, true);

            entity.Property(e => e.AppointmentDateTime).HasPrecision(0);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ReasonForVisit).HasMaxLength(200);

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK_Appointment_Doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK_Appointment_Patient");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("PK__Doctor__2DC00EBFD60A7CF8");

            entity.ToTable("Doctor");

            entity.HasIndex(e => e.Area, "IX_Doctor_Area");

            entity.HasIndex(e => e.Email, "UX_Doctor_Email")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.Property(e => e.About).HasMaxLength(1000);
            entity.Property(e => e.AffiliatedHospital).HasMaxLength(200);
            entity.Property(e => e.Area).HasMaxLength(100);
            entity.Property(e => e.Certificates).HasMaxLength(500);
            entity.Property(e => e.ContactNumber).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(120);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.MedicalLicense).HasMaxLength(50);
            entity.Property(e => e.ProfilePicture).HasMaxLength(300);
            entity.Property(e => e.Salary).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Specialization).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__MedicalR__FBDF78E942C14556");

            entity.ToTable("MedicalRecord");

            entity.HasIndex(e => new { e.DoctorId, e.RecordDateTime }, "IX_MedicalRecord_DoctorDate").IsDescending(false, true);

            entity.HasIndex(e => new { e.PatientId, e.RecordDateTime }, "IX_MedicalRecord_PatientDate").IsDescending(false, true);

            entity.HasIndex(e => e.AppointmentId, "UX_MedicalRecord_Appointment").IsUnique();

            entity.Property(e => e.ChiefComplaint).HasMaxLength(300);
            entity.Property(e => e.Diagnosis).HasMaxLength(500);
            entity.Property(e => e.PrescriptionImagePath).HasMaxLength(300);
            entity.Property(e => e.Radiology).HasMaxLength(300);
            entity.Property(e => e.RecordDateTime)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Treatment).HasMaxLength(500);

            entity.HasOne(d => d.Appointment).WithOne(p => p.MedicalRecord)
                .HasForeignKey<MedicalRecord>(d => d.AppointmentId)
                .HasConstraintName("FK_MedicalRecord_Appointment");

            entity.HasOne(d => d.Doctor).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalRecord_Doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalRecord_Patient");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patient__970EC36617F9ED89");

            entity.ToTable("Patient");

            entity.HasIndex(e => e.Ssn, "UQ_Patient_SSN").IsUnique();

            entity.HasIndex(e => e.Email, "UX_Patient_Email")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.BloodType).HasMaxLength(5);
            entity.Property(e => e.ContactNumber).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(120);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.ProfilePicture).HasMaxLength(300);
            entity.Property(e => e.Ssn)
                .HasMaxLength(20)
                .HasDefaultValue("000-00-0000")
                .HasColumnName("SSN");
            entity.Property(e => e.WeightKg).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<SensorDatum>(entity =>
        {
            entity.HasKey(e => e.SensorDataId).HasName("PK__SensorDa__14C8841083793CAD");

            entity.HasIndex(e => new { e.PatientId, e.Timestamp }, "IX_SensorData_PatientTime").IsDescending(false, true);

            entity.Property(e => e.ActivityLevel).HasMaxLength(50);
            entity.Property(e => e.PressureHeart)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("pressure_heart");
            entity.Property(e => e.SpO2).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Temperature).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Timestamp).HasPrecision(0);

            entity.HasOne(d => d.Patient).WithMany(p => p.SensorData)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK_SensorData_Patient");
        });

        //modelBuilder.Entity<SurgeryHistory>(entity =>
        //{
        //    entity.HasKey(e => e.SurgeryId).HasName("PK__SurgeryH__08AD55FD37A0F35E");

        //    entity.ToTable("SurgeryHistory");

        //    entity.Property(e => e.Notes).HasMaxLength(300);
        //    entity.Property(e => e.SurgeryType).HasMaxLength(150);

        //    entity.HasOne(d => d.Patient).WithMany(p => p.SurgeryHistories)
        //        .HasForeignKey(d => d.PatientId)
        //        .HasConstraintName("FK_SurgeryHistory_Patient");
        //});

        modelBuilder.Entity<UserCredential>(entity =>
        {
            entity.HasKey(e => e.CredentialId).HasName("PK__UserCred__2C58F9CC9A9B9E42");

            entity.ToTable("UserCredential");

            entity.HasIndex(e => e.DoctorId, "UX_UserCredential_DoctorId")
                .IsUnique()
                .HasFilter("([DoctorId] IS NOT NULL)");

            entity.HasIndex(e => e.PatientId, "UX_UserCredential_PatientId")
                .IsUnique()
                .HasFilter("([PatientId] IS NOT NULL)");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Role).HasMaxLength(20);

            entity.HasOne(d => d.Doctor).WithOne(p => p.UserCredential)
                .HasForeignKey<UserCredential>(d => d.DoctorId)
                .HasConstraintName("FK_UserCredential_Doctor");

            entity.HasOne(d => d.Patient).WithOne(p => p.UserCredential)
                .HasForeignKey<UserCredential>(d => d.PatientId)
                .HasConstraintName("FK_UserCredential_Patient");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
