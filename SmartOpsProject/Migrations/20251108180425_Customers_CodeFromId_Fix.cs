using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartOpsProject.Migrations
{
    /// <inheritdoc />
    public partial class Customers_CodeFromId_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 0) Drop τυχόν index πάνω στο CustomerCode
            migrationBuilder.Sql(@"
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customers_CustomerCode' AND object_id = OBJECT_ID('dbo.Customers'))
            DROP INDEX [IX_Customers_CustomerCode] ON [dbo].[Customers];
    ");

            // 1) Drop default constraint & τη στήλη CustomerCode (αν υπάρχει)
            migrationBuilder.Sql(@"
        IF COL_LENGTH('dbo.Customers', 'CustomerCode') IS NOT NULL
        BEGIN
            DECLARE @dc sysname;
            SELECT @dc = d.name
            FROM sys.default_constraints d
            JOIN sys.columns c
              ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
            WHERE d.parent_object_id = OBJECT_ID('dbo.Customers') AND c.name = 'CustomerCode';
            IF @dc IS NOT NULL EXEC('ALTER TABLE dbo.Customers DROP CONSTRAINT [' + @dc + ']');

            ALTER TABLE dbo.Customers DROP COLUMN [CustomerCode];
        END
    ");

            // 2) Drop παλιό CodeNumber (αν υπήρχε)
            migrationBuilder.Sql(@"
        IF COL_LENGTH('dbo.Customers', 'CodeNumber') IS NOT NULL
            ALTER TABLE dbo.Customers DROP COLUMN [CodeNumber];
    ");

            // 3) Δημιούργησε τη σωστή computed από το Id (PERSISTED)
            migrationBuilder.AddColumn<string>(
                name: "CustomerCode",
                table: "Customers",
                type: "varchar(16)",
                nullable: true,
                computedColumnSql: "('0' + RIGHT('00000' + CONVERT(varchar(10), [Id]), 5))",
                stored: true
            );

            // 4) Unique index
            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerCode",
                table: "Customers",
                column: "CustomerCode",
                unique: true
            );
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Ρίχνουμε index & στήλη CustomerCode
            migrationBuilder.Sql(@"
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customers_CustomerCode' AND object_id = OBJECT_ID('dbo.Customers'))
            DROP INDEX [IX_Customers_CustomerCode] ON [dbo].[Customers];
    ");

            migrationBuilder.Sql(@"
        IF COL_LENGTH('dbo.Customers', 'CustomerCode') IS NOT NULL
        BEGIN
            DECLARE @dc sysname;
            SELECT @dc = d.name
            FROM sys.default_constraints d
            JOIN sys.columns c
              ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
            WHERE d.parent_object_id = OBJECT_ID('dbo.Customers') AND c.name = 'CustomerCode';
            IF @dc IS NOT NULL EXEC('ALTER TABLE dbo.Customers DROP CONSTRAINT [' + @dc + ']');

            ALTER TABLE dbo.Customers DROP COLUMN [CustomerCode];
        END
    ");
        }

    }
}
