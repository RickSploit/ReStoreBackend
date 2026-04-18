using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanApplianceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Appliances");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Appliances",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
