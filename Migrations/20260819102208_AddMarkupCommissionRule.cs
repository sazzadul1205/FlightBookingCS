using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightBookingCS.Migrations
{
    /// <inheritdoc />
    public partial class AddMarkupCommissionRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarkupCommissionRule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AirlineCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    MarkupType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MarkupValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CommissionValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkupCommissionRule", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarkupCommissionRule");
        }
    }
}
