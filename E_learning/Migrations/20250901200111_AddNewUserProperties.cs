using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_learning.Migrations
{
    /// <inheritdoc />
    public partial class AddNewUserProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cours",
                columns: table => new
                {
                    IdCours = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Niveau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Titre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cours", x => x.IdCours);
                });

            migrationBuilder.CreateTable(
                name: "SessionsVisioconference",
                columns: table => new
                {
                    IdSession = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LienDailyCo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnregistrementMP4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomSession = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrlSession = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateHeureDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CoursId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionsVisioconference", x => x.IdSession);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    IdUtilisateur = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotDePasse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstActif = table.Column<bool>(type: "bit", nullable: false),
                    TypeUtilisateur = table.Column<int>(type: "int", nullable: false),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateDeNaissance = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Adresse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumTel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Identifiant = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.IdUtilisateur);
                });

            migrationBuilder.CreateTable(
                name: "Exercices",
                columns: table => new
                {
                    IdExercice = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Contenu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReponseCorrecte = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateLimite = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CoursId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercices", x => x.IdExercice);
                    table.ForeignKey(
                        name: "FK_Exercices_Cours_CoursId",
                        column: x => x.CoursId,
                        principalTable: "Cours",
                        principalColumn: "IdCours",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enregistrements",
                columns: table => new
                {
                    IdEnregistrement = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LienMP4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CoursId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enregistrements", x => x.IdEnregistrement);
                    table.ForeignKey(
                        name: "FK_Enregistrements_Cours_CoursId",
                        column: x => x.CoursId,
                        principalTable: "Cours",
                        principalColumn: "IdCours",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enregistrements_SessionsVisioconference_SessionId",
                        column: x => x.SessionId,
                        principalTable: "SessionsVisioconference",
                        principalColumn: "IdSession",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Absences",
                columns: table => new
                {
                    IdAbsence = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateAbsence = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false),
                    Motif = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodeAbsence = table.Column<int>(type: "int", nullable: false),
                    DateAjout = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UtilisateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EnregistreurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CoursIdCours = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UtilisateurIdUtilisateur = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Absences", x => x.IdAbsence);
                    table.ForeignKey(
                        name: "FK_Absences_Cours_CoursIdCours",
                        column: x => x.CoursIdCours,
                        principalTable: "Cours",
                        principalColumn: "IdCours");
                    table.ForeignKey(
                        name: "FK_Absences_Utilisateurs_EnregistreurId",
                        column: x => x.EnregistreurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                    table.ForeignKey(
                        name: "FK_Absences_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                    table.ForeignKey(
                        name: "FK_Absences_Utilisateurs_UtilisateurIdUtilisateur",
                        column: x => x.UtilisateurIdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                });

            migrationBuilder.CreateTable(
                name: "CoursApprenants",
                columns: table => new
                {
                    IdCours = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdApprenant = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursApprenants", x => new { x.IdCours, x.IdApprenant });
                    table.ForeignKey(
                        name: "FK_CoursApprenants_Cours_IdCours",
                        column: x => x.IdCours,
                        principalTable: "Cours",
                        principalColumn: "IdCours",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoursApprenants_Utilisateurs_IdApprenant",
                        column: x => x.IdApprenant,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                });

            migrationBuilder.CreateTable(
                name: "CoursFormateurs",
                columns: table => new
                {
                    IdCours = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdFormateur = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursFormateurs", x => new { x.IdCours, x.IdFormateur });
                    table.ForeignKey(
                        name: "FK_CoursFormateurs_Cours_IdCours",
                        column: x => x.IdCours,
                        principalTable: "Cours",
                        principalColumn: "IdCours",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoursFormateurs_Utilisateurs_IdFormateur",
                        column: x => x.IdFormateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                });

            migrationBuilder.CreateTable(
                name: "Fichiers",
                columns: table => new
                {
                    IdFichier = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheminFichier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeFichier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoursId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ExerciceId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ResultatId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fichiers", x => x.IdFichier);
                    table.ForeignKey(
                        name: "FK_Fichiers_Cours_CoursId",
                        column: x => x.CoursId,
                        principalTable: "Cours",
                        principalColumn: "IdCours",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Fichiers_Exercices_ExerciceId",
                        column: x => x.ExerciceId,
                        principalTable: "Exercices",
                        principalColumn: "IdExercice");
                });

            migrationBuilder.CreateTable(
                name: "Resultats",
                columns: table => new
                {
                    IdResultat = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Note = table.Column<double>(type: "float", nullable: false),
                    Remarque = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResultatFinal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatePublication = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdApprenant = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdAuteur = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CoursId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FichierId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resultats", x => x.IdResultat);
                    table.ForeignKey(
                        name: "FK_Resultats_Cours_CoursId",
                        column: x => x.CoursId,
                        principalTable: "Cours",
                        principalColumn: "IdCours",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resultats_Fichiers_FichierId",
                        column: x => x.FichierId,
                        principalTable: "Fichiers",
                        principalColumn: "IdFichier",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Resultats_Utilisateurs_IdApprenant",
                        column: x => x.IdApprenant,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resultats_Utilisateurs_IdAuteur",
                        column: x => x.IdAuteur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Absences_CoursIdCours",
                table: "Absences",
                column: "CoursIdCours");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_EnregistreurId",
                table: "Absences",
                column: "EnregistreurId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_UtilisateurId",
                table: "Absences",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_UtilisateurIdUtilisateur",
                table: "Absences",
                column: "UtilisateurIdUtilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_CoursApprenants_IdApprenant",
                table: "CoursApprenants",
                column: "IdApprenant");

            migrationBuilder.CreateIndex(
                name: "IX_CoursFormateurs_IdFormateur",
                table: "CoursFormateurs",
                column: "IdFormateur");

            migrationBuilder.CreateIndex(
                name: "IX_Enregistrements_CoursId",
                table: "Enregistrements",
                column: "CoursId");

            migrationBuilder.CreateIndex(
                name: "IX_Enregistrements_SessionId",
                table: "Enregistrements",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercices_CoursId",
                table: "Exercices",
                column: "CoursId");

            migrationBuilder.CreateIndex(
                name: "IX_Fichiers_CoursId",
                table: "Fichiers",
                column: "CoursId");

            migrationBuilder.CreateIndex(
                name: "IX_Fichiers_ExerciceId",
                table: "Fichiers",
                column: "ExerciceId");

            migrationBuilder.CreateIndex(
                name: "IX_Resultats_CoursId",
                table: "Resultats",
                column: "CoursId");

            migrationBuilder.CreateIndex(
                name: "IX_Resultats_FichierId",
                table: "Resultats",
                column: "FichierId",
                unique: true,
                filter: "[FichierId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Resultats_IdApprenant",
                table: "Resultats",
                column: "IdApprenant");

            migrationBuilder.CreateIndex(
                name: "IX_Resultats_IdAuteur",
                table: "Resultats",
                column: "IdAuteur");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Absences");

            migrationBuilder.DropTable(
                name: "CoursApprenants");

            migrationBuilder.DropTable(
                name: "CoursFormateurs");

            migrationBuilder.DropTable(
                name: "Enregistrements");

            migrationBuilder.DropTable(
                name: "Resultats");

            migrationBuilder.DropTable(
                name: "SessionsVisioconference");

            migrationBuilder.DropTable(
                name: "Fichiers");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Exercices");

            migrationBuilder.DropTable(
                name: "Cours");
        }
    }
}
