using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRepairRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairRequests_AspNetUsers_BuyerId",
                table: "RepairRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairRequests_AspNetUsers_TechnicianId",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "RepairRequests");

            migrationBuilder.RenameColumn(
                name: "ProblemDescription",
                table: "RepairRequests",
                newName: "IssuesDescription");

            migrationBuilder.RenameColumn(
                name: "BuyerId",
                table: "RepairRequests",
                newName: "SellerId");

            migrationBuilder.RenameIndex(
                name: "IX_RepairRequests_BuyerId",
                table: "RepairRequests",
                newName: "IX_RepairRequests_SellerId");

            migrationBuilder.AddColumn<int>(
                name: "ApplianceId",
                table: "RepairRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequests_ApplianceId",
                table: "RepairRequests",
                column: "ApplianceId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairRequests_Appliances_ApplianceId",
                table: "RepairRequests",
                column: "ApplianceId",
                principalTable: "Appliances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairRequests_AspNetUsers_SellerId",
                table: "RepairRequests",
                column: "SellerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairRequests_AspNetUsers_TechnicianId",
                table: "RepairRequests",
                column: "TechnicianId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairRequests_Appliances_ApplianceId",
                table: "RepairRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairRequests_AspNetUsers_SellerId",
                table: "RepairRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairRequests_AspNetUsers_TechnicianId",
                table: "RepairRequests");

            migrationBuilder.DropIndex(
                name: "IX_RepairRequests_ApplianceId",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "ApplianceId",
                table: "RepairRequests");

            migrationBuilder.RenameColumn(
                name: "SellerId",
                table: "RepairRequests",
                newName: "BuyerId");

            migrationBuilder.RenameColumn(
                name: "IssuesDescription",
                table: "RepairRequests",
                newName: "ProblemDescription");

            migrationBuilder.RenameIndex(
                name: "IX_RepairRequests_SellerId",
                table: "RepairRequests",
                newName: "IX_RepairRequests_BuyerId");

            migrationBuilder.AddColumn<string>(
                name: "DeviceType",
                table: "RepairRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairRequests_AspNetUsers_BuyerId",
                table: "RepairRequests",
                column: "BuyerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairRequests_AspNetUsers_TechnicianId",
                table: "RepairRequests",
                column: "TechnicianId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
