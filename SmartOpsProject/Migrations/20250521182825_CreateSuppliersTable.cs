using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartOpsProject.Migrations
{
    /// <inheritdoc />
    public partial class CreateSuppliersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxId",
                table: "Suppliers",
                newName: "VatStatus");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Suppliers",
                newName: "TaxIdentificationNumber");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Suppliers",
                newName: "SupplierCode");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupplierCategory",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "SupplierCategory",
                table: "Suppliers");

            migrationBuilder.RenameColumn(
                name: "VatStatus",
                table: "Suppliers",
                newName: "TaxId");

            migrationBuilder.RenameColumn(
                name: "TaxIdentificationNumber",
                table: "Suppliers",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "SupplierCode",
                table: "Suppliers",
                newName: "Email");
        }
    }
}
