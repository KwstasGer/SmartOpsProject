using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartOpsProject.Migrations
{
    /// <inheritdoc />
    public partial class NewProductTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLines_Products_ProductId",
                table: "InvoiceLines");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoicesItems_Products_ProductId",
                table: "InvoicesItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Product",
                newName: "WholesalePrice");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Product",
                newName: "Unit");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Product",
                newName: "ProductCode");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Product",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RetailPrice",
                table: "Product",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VAT",
                table: "Product",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLines_Product_ProductId",
                table: "InvoiceLines",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicesItems_Product_ProductId",
                table: "InvoicesItems",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLines_Product_ProductId",
                table: "InvoiceLines");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoicesItems_Product_ProductId",
                table: "InvoicesItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "RetailPrice",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "VAT",
                table: "Product");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameColumn(
                name: "WholesalePrice",
                table: "Products",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "Products",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ProductCode",
                table: "Products",
                newName: "Code");

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLines_Products_ProductId",
                table: "InvoiceLines",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicesItems_Products_ProductId",
                table: "InvoicesItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
