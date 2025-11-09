using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartOpsProject.Migrations
{
    /// <inheritdoc />
    public partial class MakeCountryNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CustomerCode",
                table: "Customers",
                type: "varchar(16)",
                nullable: true,
                defaultValueSql: "('0' + RIGHT('00000' + CONVERT(varchar(10), NEXT VALUE FOR dbo.CustomerCodeSeq), 5))",
                oldClrType: typeof(string),
                oldType: "varchar(16)",
                oldNullable: true,
                oldComputedColumnSql: "('0' + RIGHT('00000' + CONVERT(varchar(10), [CodeNumber]), 5))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CustomerCode",
                table: "Customers",
                type: "varchar(16)",
                nullable: true,
                computedColumnSql: "('0' + RIGHT('00000' + CONVERT(varchar(10), [CodeNumber]), 5))",
                stored: true,
                oldClrType: typeof(string),
                oldType: "varchar(16)",
                oldNullable: true,
                oldDefaultValueSql: "('0' + RIGHT('00000' + CONVERT(varchar(10), NEXT VALUE FOR dbo.CustomerCodeSeq), 5))");
        }
    }
}
