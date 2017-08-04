using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Playground.core.Migrations
{
    public partial class SubCompanies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Companies",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Sub",
                table: "Companies",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sub2",
                table: "Companies",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Sub",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Sub2",
                table: "Companies");
        }
    }
}
