using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tourism.Migrations
{
    /// <inheritdoc />
    public partial class AddedPhotosToRegionsAndCities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropIndex(
            //     name: "IX_GuideTours_GuideId",
            //     table: "GuideTours");

            migrationBuilder.AddColumn<string>(
                name: "MainPhoto",
                table: "Regions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainPhoto",
                table: "Cities",
                type: "nvarchar(max)",
                nullable: true);

            // migrationBuilder.AddPrimaryKey(
            //     name: "PK_GuideTours",
            //     table: "GuideTours",
            //     columns: new[] { "GuideId", "TourId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropPrimaryKey(
            //     name: "PK_GuideTours",
            //     table: "GuideTours");

            migrationBuilder.DropColumn(
                name: "MainPhoto",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "MainPhoto",
                table: "Cities");

            // migrationBuilder.CreateIndex(
            //     name: "IX_GuideTours_GuideId",
            //     table: "GuideTours",
            //     column: "GuideId");
        }
    }
}
