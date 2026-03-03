using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TpGestionHopital.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientSearchAndConsultationTimelineIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Patients_LastName",
                table: "Patients",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_PatientId_Date",
                table: "Consultations",
                columns: new[] { "PatientId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_LastName",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_PatientId_Date",
                table: "Consultations");
        }
    }
}
