using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MaintenanceTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_requests", x => x.id);
                    table.CheckConstraint("ck_maintenance_requests_category", "category IN ('Infrastructure', 'Equipment', 'Software', 'Other')");
                    table.CheckConstraint("ck_maintenance_requests_description_length", "char_length(btrim(description)) BETWEEN 10 AND 2000");
                    table.CheckConstraint("ck_maintenance_requests_priority", "priority IN ('Low', 'Medium', 'High', 'Critical')");
                    table.CheckConstraint("ck_maintenance_requests_status", "status IN ('Pending', 'InProgress', 'OnHold', 'Resolved', 'Cancelled')");
                    table.CheckConstraint("ck_maintenance_requests_title_length", "char_length(btrim(title)) BETWEEN 5 AND 120");
                    table.ForeignKey(
                        name: "fk_maintenance_requests_users_assignee_id",
                        column: x => x.assignee_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_maintenance_requests_users_requester_id",
                        column: x => x.requester_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "activity_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintenance_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    performed_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    previous_assignee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    new_assignee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_entries", x => x.id);
                    table.CheckConstraint("ck_activity_entries_from_status", "from_status IN ('Pending', 'InProgress', 'OnHold', 'Resolved', 'Cancelled')");
                    table.CheckConstraint("ck_activity_entries_to_status", "to_status IN ('Pending', 'InProgress', 'OnHold', 'Resolved', 'Cancelled')");
                    table.CheckConstraint("ck_activity_entries_type_columns", "(type = 'Created' AND from_status IS NULL AND to_status IS NULL\n    AND previous_assignee_id IS NULL AND new_assignee_id IS NULL)\nOR (type = 'StatusChanged' AND from_status IS NOT NULL AND to_status IS NOT NULL\n    AND previous_assignee_id IS NULL AND new_assignee_id IS NULL)\nOR (type = 'AssigneeChanged' AND from_status IS NULL AND to_status IS NULL\n    AND new_assignee_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_activity_entries_maintenance_requests_maintenance_request_id",
                        column: x => x.maintenance_request_id,
                        principalTable: "maintenance_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_activity_entries_users_new_assignee_id",
                        column: x => x.new_assignee_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_activity_entries_users_performed_by_id",
                        column: x => x.performed_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_activity_entries_users_previous_assignee_id",
                        column: x => x.previous_assignee_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b01"), "Ana Restrepo" },
                    { new Guid("0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b02"), "Laura Gómez" },
                    { new Guid("0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b03"), "Carlos Mejía" },
                    { new Guid("0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b04"), "Andrés Ríos" },
                    { new Guid("0b6a2f4e-1c3d-4e5f-8a9b-1d2e3f4a5b05"), "Sofía Vélez" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_activity_entries_maintenance_request_id_occurred_at_id",
                table: "activity_entries",
                columns: new[] { "maintenance_request_id", "occurred_at", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_activity_entries_new_assignee_id",
                table: "activity_entries",
                column: "new_assignee_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_entries_performed_by_id",
                table: "activity_entries",
                column: "performed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_entries_previous_assignee_id",
                table: "activity_entries",
                column: "previous_assignee_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_requests_assignee_id",
                table: "maintenance_requests",
                column: "assignee_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_requests_category",
                table: "maintenance_requests",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_requests_created_at_id",
                table: "maintenance_requests",
                columns: new[] { "created_at", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_requests_priority",
                table: "maintenance_requests",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_requests_requester_id",
                table: "maintenance_requests",
                column: "requester_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_requests_status",
                table: "maintenance_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_requests_title",
                table: "maintenance_requests",
                column: "title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_entries");

            migrationBuilder.DropTable(
                name: "maintenance_requests");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
