using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultVisitsToSubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultVisits",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultVisits",
                table: "SubscriptionPlans");
        }
    }
}
