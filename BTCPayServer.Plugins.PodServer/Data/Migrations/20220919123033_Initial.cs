using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTCPayServer.Plugins.PodServer.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "BTCPayServer.Plugins.PodServer");

            migrationBuilder.CreateTable(
                name: "Podcasts",
                schema: "BTCPayServer.Plugins.PodServer",
                columns: table => new
                {
                    PodcastId = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Medium = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    ImageFileId = table.Column<string>(type: "text", nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Podcasts", x => x.PodcastId);
                });

            migrationBuilder.CreateTable(
                name: "Editors",
                schema: "BTCPayServer.Plugins.PodServer",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    PodcastId = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Editors", x => new { x.UserId, x.PodcastId });
                    table.ForeignKey(
                        name: "FK_Editors_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalSchema: "BTCPayServer.Plugins.PodServer",
                        principalTable: "Podcasts",
                        principalColumn: "PodcastId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Imports",
                schema: "BTCPayServer.Plugins.PodServer",
                columns: table => new
                {
                    ImportId = table.Column<string>(type: "text", nullable: false),
                    Raw = table.Column<string>(type: "text", nullable: false),
                    Log = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PodcastId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imports", x => x.ImportId);
                    table.ForeignKey(
                        name: "FK_Imports_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalSchema: "BTCPayServer.Plugins.PodServer",
                        principalTable: "Podcasts",
                        principalColumn: "PodcastId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "People",
                schema: "BTCPayServer.Plugins.PodServer",
                columns: table => new
                {
                    PersonId = table.Column<string>(type: "text", nullable: false),
                    PodcastId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true),
                    ImageFileId = table.Column<string>(type: "text", nullable: true),
                    ValueRecipient_Type = table.Column<string>(type: "text", nullable: true),
                    ValueRecipient_Address = table.Column<string>(type: "text", nullable: true),
                    ValueRecipient_CustomKey = table.Column<string>(type: "text", nullable: true),
                    ValueRecipient_CustomValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.PersonId);
                    table.ForeignKey(
                        name: "FK_People_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalSchema: "BTCPayServer.Plugins.PodServer",
                        principalTable: "Podcasts",
                        principalColumn: "PodcastId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                schema: "BTCPayServer.Plugins.PodServer",
                columns: table => new
                {
                    SeasonId = table.Column<string>(type: "text", nullable: false),
                    PodcastId = table.Column<string>(type: "text", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.SeasonId);
                    table.ForeignKey(
                        name: "FK_Seasons_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalSchema: "BTCPayServer.Plugins.PodServer",
                        principalTable: "Podcasts",
                        principalColumn: "PodcastId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Episodes",
                schema: "BTCPayServer.Plugins.PodServer",
                columns: table => new
                {
                    EpisodeId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ImageFileId = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: true),
                    ImportGuid = table.Column<string>(type: "text", nullable: true),
                    PodcastId = table.Column<string>(type: "text", nullable: false),
                    SeasonId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.EpisodeId);
                    table.ForeignKey(
                        name: "FK_Episodes_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalSchema: "BTCPayServer.Plugins.PodServer",
                        principalTable: "Podcasts",
                        principalColumn: "PodcastId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Episodes_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "BTCPayServer.Plugins.PodServer",
                        principalTable: "Seasons",
                        principalColumn: "SeasonId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Contributions",
                schema: "BTCPayServer.Plugins.PodServer",
                columns: table => new
                {
                    ContributionId = table.Column<string>(type: "text", nullable: false),
                    PodcastId = table.Column<string>(type: "text", nullable: false),
                    PersonId = table.Column<string>(type: "text", nullable: false),
                    EpisodeId = table.Column<string>(type: "text", nullable: true),
                    Split = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contributions", x => x.ContributionId);
                    table.ForeignKey(
                        name: "FK_Contributions_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "BTCPayServer.Plugins.PodServer",
                        principalTable: "Episodes",
                        principalColumn: "EpisodeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contributions_People_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "BTCPayServer.Plugins.PodServer",
                        principalTable: "People",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contributions_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalSchema: "BTCPayServer.Plugins.PodServer",
                        principalTable: "Podcasts",
                        principalColumn: "PodcastId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enclosures",
                schema: "BTCPayServer.Plugins.PodServer",
                columns: table => new
                {
                    EnclosureId = table.Column<string>(type: "text", nullable: false),
                    EpisodeId = table.Column<string>(type: "text", nullable: false),
                    FileId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    IsAlternate = table.Column<bool>(type: "boolean", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enclosures", x => x.EnclosureId);
                    table.ForeignKey(
                        name: "FK_Enclosures_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "BTCPayServer.Plugins.PodServer",
                        principalTable: "Episodes",
                        principalColumn: "EpisodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contributions_EpisodeId",
                schema: "BTCPayServer.Plugins.PodServer",
                table: "Contributions",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Contributions_PersonId",
                schema: "BTCPayServer.Plugins.PodServer",
                table: "Contributions",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Contributions_PodcastId_EpisodeId_PersonId",
                schema: "BTCPayServer.Plugins.PodServer",
                table: "Contributions",
                columns: new[] { "PodcastId", "EpisodeId", "PersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Editors_PodcastId",
                schema: "BTCPayServer.Plugins.PodServer",
                table: "Editors",
                column: "PodcastId");

            migrationBuilder.CreateIndex(
                name: "IX_Enclosures_EpisodeId",
                schema: "BTCPayServer.Plugins.PodServer",
                table: "Enclosures",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_PodcastId_Slug",
                schema: "BTCPayServer.Plugins.PodServer",
                table: "Episodes",
                columns: new[] { "PodcastId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_SeasonId",
                schema: "BTCPayServer.Plugins.PodServer",
                table: "Episodes",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Imports_PodcastId",
                schema: "BTCPayServer.Plugins.PodServer",
                table: "Imports",
                column: "PodcastId");

            migrationBuilder.CreateIndex(
                name: "IX_People_PodcastId",
                schema: "BTCPayServer.Plugins.PodServer",
                table: "People",
                column: "PodcastId");

            migrationBuilder.CreateIndex(
                name: "IX_Podcasts_Slug",
                schema: "BTCPayServer.Plugins.PodServer",
                table: "Podcasts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_PodcastId_Number",
                schema: "BTCPayServer.Plugins.PodServer",
                table: "Seasons",
                columns: new[] { "PodcastId", "Number" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contributions",
                schema: "BTCPayServer.Plugins.PodServer");

            migrationBuilder.DropTable(
                name: "Editors",
                schema: "BTCPayServer.Plugins.PodServer");

            migrationBuilder.DropTable(
                name: "Enclosures",
                schema: "BTCPayServer.Plugins.PodServer");

            migrationBuilder.DropTable(
                name: "Imports",
                schema: "BTCPayServer.Plugins.PodServer");

            migrationBuilder.DropTable(
                name: "People",
                schema: "BTCPayServer.Plugins.PodServer");

            migrationBuilder.DropTable(
                name: "Episodes",
                schema: "BTCPayServer.Plugins.PodServer");

            migrationBuilder.DropTable(
                name: "Seasons",
                schema: "BTCPayServer.Plugins.PodServer");

            migrationBuilder.DropTable(
                name: "Podcasts",
                schema: "BTCPayServer.Plugins.PodServer");
        }
    }
}
