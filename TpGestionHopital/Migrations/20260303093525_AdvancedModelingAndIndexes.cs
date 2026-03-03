using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TpGestionHopital.Migrations
{
    /// <inheritdoc />
    public partial class AdvancedModelingAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consultations_DoctorId",
                table: "Consultations");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Patients",
                newName: "Address_Street");

            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_PostalCode",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Patients",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "Departments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                table: "Departments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_PostalCode",
                table: "Departments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                table: "Departments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_DoctorId_Date",
                table: "Consultations",
                columns: new[] { "DoctorId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consultations_DoctorId_Date",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Address_PostalCode",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "Address_PostalCode",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "Address_Street",
                table: "Departments");

            migrationBuilder.RenameColumn(
                name: "Address_Street",
                table: "Patients",
                newName: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_DoctorId",
                table: "Consultations",
                column: "DoctorId");
        }
    }
}
