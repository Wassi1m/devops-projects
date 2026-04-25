using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BourseIA.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MotDePasseHash = table.Column<string>(type: "text", nullable: false),
                    DateInscription = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PhotoProfil = table.Column<string>(type: "text", nullable: true),
                    ProfilInvestisseur = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourbesBousieres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomFichier = table.Column<string>(type: "text", nullable: false),
                    CheminFichier = table.Column<string>(type: "text", nullable: false),
                    TypeAction = table.Column<string>(type: "text", nullable: true),
                    DateUpload = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Statut = table.Column<string>(type: "text", nullable: false),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourbesBousieres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourbesBousieres_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Message = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    EstLue = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LienAction = table.Column<string>(type: "text", nullable: true),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateurId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Utilisateurs_CreateurId",
                        column: x => x.CreateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResultatsAnalyse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tendance = table.Column<string>(type: "text", nullable: false),
                    PrixMin = table.Column<double>(type: "double precision", nullable: true),
                    PrixMax = table.Column<double>(type: "double precision", nullable: true),
                    PrixMoyen = table.Column<double>(type: "double precision", nullable: true),
                    EcartType = table.Column<double>(type: "double precision", nullable: true),
                    PointsCles = table.Column<string>(type: "text", nullable: true),
                    RapportJson = table.Column<string>(type: "text", nullable: true),
                    StatutAnalyse = table.Column<string>(type: "text", nullable: false),
                    DateAnalyse = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MessageErreur = table.Column<string>(type: "text", nullable: true),
                    CourbeBoursiereId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultatsAnalyse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultatsAnalyse_CourbesBousieres_CourbeBoursiereId",
                        column: x => x.CourbeBoursiereId,
                        principalTable: "CourbesBousieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MembresEquipe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    DateAdhesion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Statut = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembresEquipe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembresEquipe_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembresEquipe_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessagesChat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Contenu = table.Column<string>(type: "text", nullable: false),
                    DateEnvoi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstLu = table.Column<bool>(type: "boolean", nullable: false),
                    TypeMessage = table.Column<string>(type: "text", nullable: false),
                    ExpediteurId = table.Column<int>(type: "integer", nullable: false),
                    DestinataireId = table.Column<int>(type: "integer", nullable: true),
                    TeamId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessagesChat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessagesChat_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessagesChat_Utilisateurs_DestinataireId",
                        column: x => x.DestinataireId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MessagesChat_Utilisateurs_ExpediteurId",
                        column: x => x.ExpediteurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartagesEquipe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    CourbeBoursiereId = table.Column<int>(type: "integer", nullable: false),
                    PartageParId = table.Column<int>(type: "integer", nullable: false),
                    DatePartage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Commentaire = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartagesEquipe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartagesEquipe_CourbesBousieres_CourbeBoursiereId",
                        column: x => x.CourbeBoursiereId,
                        principalTable: "CourbesBousieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartagesEquipe_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartagesEquipe_Utilisateurs_PartageParId",
                        column: x => x.PartageParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IdeesInvestissement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ActionConcernee = table.Column<string>(type: "text", nullable: true),
                    TypeIdee = table.Column<string>(type: "text", nullable: false),
                    Tendance = table.Column<string>(type: "text", nullable: false),
                    RentabiliteEstimee = table.Column<double>(type: "double precision", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstPublique = table.Column<bool>(type: "boolean", nullable: false),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: true),
                    ResultatAnalyseId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdeesInvestissement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdeesInvestissement_ResultatsAnalyse_ResultatAnalyseId",
                        column: x => x.ResultatAnalyseId,
                        principalTable: "ResultatsAnalyse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IdeesInvestissement_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IdeesInvestissement_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourbesBousieres_UtilisateurId",
                table: "CourbesBousieres",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_IdeesInvestissement_ResultatAnalyseId",
                table: "IdeesInvestissement",
                column: "ResultatAnalyseId");

            migrationBuilder.CreateIndex(
                name: "IX_IdeesInvestissement_TeamId",
                table: "IdeesInvestissement",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_IdeesInvestissement_UtilisateurId",
                table: "IdeesInvestissement",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_MembresEquipe_TeamId",
                table: "MembresEquipe",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MembresEquipe_UtilisateurId",
                table: "MembresEquipe",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesChat_DestinataireId",
                table: "MessagesChat",
                column: "DestinataireId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesChat_ExpediteurId",
                table: "MessagesChat",
                column: "ExpediteurId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesChat_TeamId",
                table: "MessagesChat",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UtilisateurId",
                table: "Notifications",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_PartagesEquipe_CourbeBoursiereId",
                table: "PartagesEquipe",
                column: "CourbeBoursiereId");

            migrationBuilder.CreateIndex(
                name: "IX_PartagesEquipe_PartageParId",
                table: "PartagesEquipe",
                column: "PartageParId");

            migrationBuilder.CreateIndex(
                name: "IX_PartagesEquipe_TeamId",
                table: "PartagesEquipe",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultatsAnalyse_CourbeBoursiereId",
                table: "ResultatsAnalyse",
                column: "CourbeBoursiereId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CreateurId",
                table: "Teams",
                column: "CreateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Email",
                table: "Utilisateurs",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdeesInvestissement");

            migrationBuilder.DropTable(
                name: "MembresEquipe");

            migrationBuilder.DropTable(
                name: "MessagesChat");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PartagesEquipe");

            migrationBuilder.DropTable(
                name: "ResultatsAnalyse");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "CourbesBousieres");

            migrationBuilder.DropTable(
                name: "Utilisateurs");
        }
    }
}
