using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartOpsProject.Migrations
{
    /// <inheritdoc />
    public partial class FinalCustomerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TIN",
                table: "Customers",
                newName: "VatStatus");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Customers",
                newName: "TaxIdentificationNumber");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Customers",
                newName: "PostalCode");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerCategory",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerCode",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerCategory",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerCode",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "VatStatus",
                table: "Customers",
                newName: "TIN");

            migrationBuilder.RenameColumn(
                name: "TaxIdentificationNumber",
                table: "Customers",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "Customers",
                newName: "Email");
        }
    }
}
