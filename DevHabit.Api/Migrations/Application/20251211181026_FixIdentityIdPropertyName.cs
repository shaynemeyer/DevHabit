using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevHabit.Api.Migrations.Application;

/// <inheritdoc />
public partial class FixIdentityIdPropertyName : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "indentity_id",
            schema: "dev_habit",
            table: "users",
            newName: "identity_id");

        migrationBuilder.RenameIndex(
            name: "ix_users_indentity_id",
            schema: "dev_habit",
            table: "users",
            newName: "ix_users_identity_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "identity_id",
            schema: "dev_habit",
            table: "users",
            newName: "indentity_id");

        migrationBuilder.RenameIndex(
            name: "ix_users_identity_id",
            schema: "dev_habit",
            table: "users",
            newName: "ix_users_indentity_id");
    }
}
