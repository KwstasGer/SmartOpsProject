using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartOpsProject.Migrations
{
    /// <inheritdoc />
    public partial class AddUserScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_UserId_Series_Year_Number",
                table: "Invoices",
                columns: new[] { "UserId", "Series", "Year", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId_Name",
                table: "Customers",
                columns: new[] { "UserId", "Name" });


            migrationBuilder.Sql(@"
                DECLARE @uid INT = (SELECT TOP 1 Id FROM dbo.Users ORDER BY Id);
                IF @uid IS NULL
                    THROW 51000, 'No users exist in dbo.Users. Create at least one user before applying this migration.', 1;

                IF COL_LENGTH('dbo.Customers', 'UserId') IS NOT NULL
                BEGIN
                    UPDATE C
                    SET C.UserId = @uid
                    FROM dbo.Customers AS C
                    WHERE C.UserId IS NULL
                       OR C.UserId = 0
                       OR NOT EXISTS (SELECT 1 FROM dbo.Users U WHERE U.Id = C.UserId);
                END

                IF COL_LENGTH('dbo.Invoices', 'UserId') IS NOT NULL
                BEGIN
                    UPDATE I
                    SET I.UserId = @uid
                    FROM dbo.Invoices AS I
                    WHERE I.UserId IS NULL
                       OR I.UserId = 0
                       OR NOT EXISTS (SELECT 1 FROM dbo.Users U WHERE U.Id = I.UserId);
                END
                ");


            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_UserId",
                table: "Customers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Users_UserId",
                table: "Invoices",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Users_UserId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Users_UserId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_UserId_Series_Year_Number",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Customers_UserId_Name",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
