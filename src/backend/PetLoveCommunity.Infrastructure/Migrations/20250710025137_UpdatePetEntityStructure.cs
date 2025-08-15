using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetLoveCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePetEntityStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pets_ExpiresAt",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "AdoptedAt",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsGoodWithKids",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsGoodWithPets",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsHouseTrained",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsSpayedNeutered",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "PrimaryPhotoUrl",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "SpecialNeeds",
                table: "Pets");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Pets",
                newName: "Views");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Pets",
                newName: "PetType");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "Pets",
                newName: "AdoptionStatus");

            migrationBuilder.RenameColumn(
                name: "IsVaccinated",
                table: "Pets",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "Pets",
                newName: "DateOfBirth");

            migrationBuilder.RenameColumn(
                name: "AdoptionFee",
                table: "Pets",
                newName: "Price");

            migrationBuilder.RenameIndex(
                name: "IX_Pets_Type",
                table: "Pets",
                newName: "IX_Pets_Views");

            migrationBuilder.RenameIndex(
                name: "IX_Pets_Status",
                table: "Pets",
                newName: "IX_Pets_PetType");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Pets",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Pets",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Breed",
                table: "Pets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Pets",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_AdoptionStatus",
                table: "Pets",
                column: "AdoptionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_IsActive",
                table: "Pets",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pets_AdoptionStatus",
                table: "Pets");

            migrationBuilder.DropIndex(
                name: "IX_Pets_IsActive",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Pets");

            migrationBuilder.RenameColumn(
                name: "Views",
                table: "Pets",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Pets",
                newName: "AdoptionFee");

            migrationBuilder.RenameColumn(
                name: "PetType",
                table: "Pets",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Pets",
                newName: "IsVaccinated");

            migrationBuilder.RenameColumn(
                name: "DateOfBirth",
                table: "Pets",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "AdoptionStatus",
                table: "Pets",
                newName: "Size");

            migrationBuilder.RenameIndex(
                name: "IX_Pets_Views",
                table: "Pets",
                newName: "IX_Pets_Type");

            migrationBuilder.RenameIndex(
                name: "IX_Pets_PetType",
                table: "Pets",
                newName: "IX_Pets_Status");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Pets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Pets",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "Breed",
                table: "Pets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "AdoptedAt",
                table: "Pets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Pets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Pets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsGoodWithKids",
                table: "Pets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGoodWithPets",
                table: "Pets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHouseTrained",
                table: "Pets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSpayedNeutered",
                table: "Pets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Pets",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryPhotoUrl",
                table: "Pets",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialNeeds",
                table: "Pets",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pets_ExpiresAt",
                table: "Pets",
                column: "ExpiresAt");
        }
    }
}
