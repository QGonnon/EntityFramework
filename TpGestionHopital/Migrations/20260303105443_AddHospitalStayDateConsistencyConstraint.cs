using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TpGestionHopital.Migrations
{
    /// <inheritdoc />
    public partial class AddHospitalStayDateConsistencyConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_HospitalStay_DischargeAfterAdmission",
                table: "HospitalStays",
                sql: "DischargeDate IS NULL OR DischargeDate >= AdmissionDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_HospitalStay_DischargeAfterAdmission",
                table: "HospitalStays");
        }
    }
}
