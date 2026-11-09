using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace RaceDay.Api.Migrations;
public partial class AddProfileImageUrl : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(name: "ProfileImageUrl", table: "Users", type: "varchar(1000)", maxLength: 1000, nullable: true);
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(name: "ProfileImageUrl", table: "Users");
}
