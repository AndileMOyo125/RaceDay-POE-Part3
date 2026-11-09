using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace RaceDay.Api.Migrations;
public partial class AddEventBannerImageUrl : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(name: "BannerImageUrl", table: "Events", type: "varchar(1000)", maxLength: 1000, nullable: true);
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(name: "BannerImageUrl", table: "Events");
}
