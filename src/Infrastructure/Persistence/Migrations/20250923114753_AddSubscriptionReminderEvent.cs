using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionReminderEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "CheckInLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "CheckInLogs");
        }
    }
}
