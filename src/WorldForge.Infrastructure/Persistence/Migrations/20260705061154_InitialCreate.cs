using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldForge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    TenantTemplateId = table.Column<string>(type: "TEXT", nullable: false),
                    DbFilePath = table.Column<string>(type: "TEXT", nullable: false),
                    LastOpenedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Settings_DefaultEra = table.Column<string>(type: "TEXT", nullable: false),
                    Settings_AutoDetectOnSave = table.Column<bool>(type: "INTEGER", nullable: false),
                    Settings_PreferredOllamaModel = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contradictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    SourceDocumentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TargetDocumentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceExcerpt = table.Column<string>(type: "TEXT", nullable: false),
                    TargetExcerpt = table.Column<string>(type: "TEXT", nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Suggestion = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DetectionRunId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contradictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contradictions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntityRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FromEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ToEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationType = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityRelationships_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    ContentJson = table.Column<string>(type: "TEXT", nullable: false),
                    Embedding = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: true),
                    WordCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: true),
                    EventDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    EraName = table.Column<string>(type: "TEXT", nullable: true),
                    SortKey = table.Column<long>(type: "INTEGER", nullable: true),
                    RelatedEntityIds = table.Column<string>(type: "TEXT", nullable: true),
                    FullName = table.Column<string>(type: "TEXT", nullable: true),
                    Aliases = table.Column<string>(type: "TEXT", nullable: true),
                    Age = table.Column<int>(type: "INTEGER", nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    FactionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Race = table.Column<string>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", nullable: true),
                    Alignment = table.Column<string>(type: "TEXT", nullable: true),
                    CustomFieldsJson = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    WorldFaction_CustomFieldsJson = table.Column<string>(type: "TEXT", nullable: true),
                    WorldItem_Name = table.Column<string>(type: "TEXT", nullable: true),
                    WorldItem_CustomFieldsJson = table.Column<string>(type: "TEXT", nullable: true),
                    WorldLocation_Name = table.Column<string>(type: "TEXT", nullable: true),
                    LocationType = table.Column<int>(type: "INTEGER", nullable: true),
                    ParentLocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CoordinatesJson = table.Column<string>(type: "TEXT", nullable: true),
                    WorldLocation_CustomFieldsJson = table.Column<string>(type: "TEXT", nullable: true),
                    DoctorVisit_EventDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    DoctorVisit_SortKey = table.Column<long>(type: "INTEGER", nullable: true),
                    DoctorVisit_RelatedEntityIds = table.Column<string>(type: "TEXT", nullable: true),
                    SymptomName = table.Column<string>(type: "TEXT", nullable: true),
                    Severity = table.Column<string>(type: "TEXT", nullable: true),
                    AccountName = table.Column<string>(type: "TEXT", nullable: true),
                    Provider = table.Column<string>(type: "TEXT", nullable: true),
                    DigitalAccount_CustomFieldsJson = table.Column<string>(type: "TEXT", nullable: true),
                    Recipient = table.Column<string>(type: "TEXT", nullable: true),
                    LifeEvent_EventDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    LifeEvent_SortKey = table.Column<long>(type: "INTEGER", nullable: true),
                    LifeEvent_RelatedEntityIds = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntityLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceDocumentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartOffset = table.Column<int>(type: "INTEGER", nullable: false),
                    EndOffset = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityLinks_Notes_SourceDocumentId",
                        column: x => x.SourceDocumentId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityLinks_Notes_TargetEntityId",
                        column: x => x.TargetEntityId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NoteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityTags_Notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contradictions_ProjectId_Status",
                table: "Contradictions",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityLinks_SourceDocumentId",
                table: "EntityLinks",
                column: "SourceDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityLinks_TargetEntityId",
                table: "EntityLinks",
                column: "TargetEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityRelationships_ProjectId",
                table: "EntityRelationships",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityTags_NoteId",
                table: "EntityTags",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_ProjectId",
                table: "Notes",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contradictions");

            migrationBuilder.DropTable(
                name: "EntityLinks");

            migrationBuilder.DropTable(
                name: "EntityRelationships");

            migrationBuilder.DropTable(
                name: "EntityTags");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
