using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartOpsProject.Migrations
{
    /// <inheritdoc />
    public partial class CustomerCode_ToSequenceDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) SEQUENCE για αυτόματη αρίθμηση (αν δεν υπάρχει)
            migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = 'CustomerCodeSeq' AND SCHEMA_NAME(schema_id)='dbo')
            CREATE SEQUENCE dbo.CustomerCodeSeq AS INT START WITH 1 INCREMENT BY 1;
    ");

            // 2) Drop index στο CustomerCode αν υπάρχει
            migrationBuilder.Sql(@"
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customers_CustomerCode' AND object_id = OBJECT_ID('dbo.Customers'))
            DROP INDEX [IX_Customers_CustomerCode] ON [dbo].[Customers];
    ");

            // 3) Αν η CustomerCode υπάρχει:
            //    - ρίξε default constraint (αν υπάρχει)
            //    - αν είναι computed, ρίξε τη στήλη
            //    - αλλιώς ρίξε τη στήλη για να την ξαναφτιάξουμε σωστά
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

            -- αν είναι computed, πρέπει να πέσει ολόκληρη η στήλη
            IF EXISTS (
                SELECT 1 FROM sys.computed_columns 
                WHERE object_id = OBJECT_ID('dbo.Customers') AND name = 'CustomerCode'
            )
                ALTER TABLE dbo.Customers DROP COLUMN [CustomerCode];
            ELSE
                ALTER TABLE dbo.Customers DROP COLUMN [CustomerCode];
        END
    ");

            // 4) Δημιουργία στήλης με default από SEQUENCE (ΟΧΙ computed)
            migrationBuilder.AddColumn<string>(
                name: "CustomerCode",
                table: "Customers",
                type: "varchar(16)",
                nullable: true,
                defaultValueSql: "('0' + RIGHT('00000' + CONVERT(varchar(10), NEXT VALUE FOR dbo.CustomerCodeSeq), 5))"
            );

            // 5) Γέμισε υπάρχουσες εγγραφές χωρίς κωδικό με σταθερή τιμή από το Id (διατηρείς την παλιά μορφή)
            migrationBuilder.Sql(@"
        UPDATE dbo.Customers
        SET CustomerCode = '0' + RIGHT('00000' + CONVERT(varchar(10), Id), 5)
        WHERE CustomerCode IS NULL OR LTRIM(RTRIM(CustomerCode)) = '';
    ");

            // 6) Unique index στο CustomerCode
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
            migrationBuilder.Sql(@"
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customers_CustomerCode' AND object_id = OBJECT_ID('dbo.Customers'))
            DROP INDEX [IX_Customers_CustomerCode] ON [dbo].[Customers];
    ");

            migrationBuilder.DropColumn(
                name: "CustomerCode",
                table: "Customers"
            );

            // Προαιρετικά: αφαίρεση της sequence
            migrationBuilder.Sql(@"
        IF EXISTS (SELECT 1 FROM sys.sequences WHERE name = 'CustomerCodeSeq' AND SCHEMA_NAME(schema_id)='dbo')
            DROP SEQUENCE dbo.CustomerCodeSeq;
    ");
        }

    }
}
