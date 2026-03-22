using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHRS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Doctor",
                columns: table => new
                {
                    DoctorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Salary = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ProfilePicture = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    Certificates = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AffiliatedHospital = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    About = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MedicalLicense = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovalStatus = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Doctor__2DC00EBFD60A7CF8", x => x.DoctorId);
                });

            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BloodType = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    HeightCm = table.Column<short>(type: "smallint", nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ProfilePicture = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SSN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "UITTDWSEBvCJTfAVqwVjQBNmIztzc8jpEWH5sRCGZGI="),
                    Diseases = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Patient__970EC36617F9ED89", x => x.PatientId);
                });

            migrationBuilder.CreateTable(
                name: "Appointment",
                columns: table => new
                {
                    AppointmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    AppointmentDateTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ReasonForVisit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Appointm__8ECDFCC2783685B3", x => x.AppointmentId);
                    table.ForeignKey(
                        name: "FK_Appointment_Doctor",
                        column: x => x.DoctorId,
                        principalTable: "Doctor",
                        principalColumn: "DoctorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointment_Patient",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SensorData",
                columns: table => new
                {
                    SensorDataId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    HeartRate = table.Column<short>(type: "smallint", nullable: true),
                    SpO2 = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Temperature = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ActivityLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    pressure_heart = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SensorDa__14C8841083793CAD", x => x.SensorDataId);
                    table.ForeignKey(
                        name: "FK_SensorData_Patient",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SurgeryHistory",
                columns: table => new
                {
                    SurgeryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    SurgeryType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SurgeryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DoctorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurgeryHistory", x => x.SurgeryId);
                    table.ForeignKey(
                        name: "FK_SurgeryHistory_Doctor_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctor",
                        principalColumn: "DoctorId");
                    table.ForeignKey(
                        name: "FK_SurgeryHistory_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCredential",
                columns: table => new
                {
                    CredentialId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    DoctorId = table.Column<int>(type: "int", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserCred__2C58F9CC9A9B9E42", x => x.CredentialId);
                    table.ForeignKey(
                        name: "FK_UserCredential_Doctor",
                        column: x => x.DoctorId,
                        principalTable: "Doctor",
                        principalColumn: "DoctorId");
                    table.ForeignKey(
                        name: "FK_UserCredential_Patient",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "PatientId");
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecord",
                columns: table => new
                {
                    RecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    RecordDateTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ClinicalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Treatment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Radiology = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrescriptionImagePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MedicalR__FBDF78E942C14556", x => x.RecordId);
                    table.ForeignKey(
                        name: "FK_MedicalRecord_Appointment",
                        column: x => x.AppointmentId,
                        principalTable: "Appointment",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalRecord_Doctor",
                        column: x => x.DoctorId,
                        principalTable: "Doctor",
                        principalColumn: "DoctorId");
                    table.ForeignKey(
                        name: "FK_MedicalRecord_Patient",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "PatientId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_DoctorDate",
                table: "Appointment",
                columns: new[] { "DoctorId", "AppointmentDateTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_PatientDate",
                table: "Appointment",
                columns: new[] { "PatientId", "AppointmentDateTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_Area",
                table: "Doctor",
                column: "Area");

            migrationBuilder.CreateIndex(
                name: "UX_Doctor_Email",
                table: "Doctor",
                column: "Email",
                unique: true,
                filter: "([Email] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecord_DoctorDate",
                table: "MedicalRecord",
                columns: new[] { "DoctorId", "RecordDateTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecord_PatientDate",
                table: "MedicalRecord",
                columns: new[] { "PatientId", "RecordDateTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "UX_MedicalRecord_Appointment",
                table: "MedicalRecord",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Patient_SSN",
                table: "Patient",
                column: "SSN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Patient_Email",
                table: "Patient",
                column: "Email",
                unique: true,
                filter: "([Email] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_SensorData_PatientTime",
                table: "SensorData",
                columns: new[] { "PatientId", "Timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_SurgeryHistory_DoctorId",
                table: "SurgeryHistory",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_SurgeryHistory_PatientId",
                table: "SurgeryHistory",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "UX_UserCredential_DoctorId",
                table: "UserCredential",
                column: "DoctorId",
                unique: true,
                filter: "([DoctorId] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "UX_UserCredential_PatientId",
                table: "UserCredential",
                column: "PatientId",
                unique: true,
                filter: "([PatientId] IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalRecord");

            migrationBuilder.DropTable(
                name: "SensorData");

            migrationBuilder.DropTable(
                name: "SurgeryHistory");

            migrationBuilder.DropTable(
                name: "UserCredential");

            migrationBuilder.DropTable(
                name: "Appointment");

            migrationBuilder.DropTable(
                name: "Doctor");

            migrationBuilder.DropTable(
                name: "Patient");
        }
    }
}
