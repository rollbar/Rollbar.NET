using System;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Microsoft.EntityFrameworkCore.Migrations;

namespace Rollbar.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Destinations",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Endpoint = table.Column<string>(nullable: false),
                    AccessToken = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinations", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PayloadRecords",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    PayloadJson = table.Column<string>(nullable: false),
                    ConfigJson = table.Column<string>(nullable: true),
                    DestinationID = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayloadRecords", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PayloadRecords_Destinations_DestinationID",
                        column: x => x.DestinationID,
                        principalTable: "Destinations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_Endpoint_AccessToken",
                table: "Destinations",
                columns: new[] { "Endpoint", "AccessToken" });

            migrationBuilder.CreateIndex(
                name: "IX_PayloadRecords_DestinationID",
                table: "PayloadRecords",
                column: "DestinationID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayloadRecords");

            migrationBuilder.DropTable(
                name: "Destinations");
        }
    }
}
