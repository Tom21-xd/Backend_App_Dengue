using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_App_Dengue.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsAndSavedPublications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_PUBLICACION_PROGRAMADA",
                table: "publicacion",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PUBLICADA",
                table: "publicacion",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "etiqueta_publicacion",
                columns: table => new
                {
                    ID_ETIQUETA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_ETIQUETA = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_ETIQUETA = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_etiqueta_publicacion", x => x.ID_ETIQUETA);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "publicacion_guardada",
                columns: table => new
                {
                    ID_GUARDADO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_PUBLICACION = table.Column<int>(type: "int", nullable: false),
                    FK_ID_USUARIO = table.Column<int>(type: "int", nullable: false),
                    FECHA_GUARDADO = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_publicacion_guardada", x => x.ID_GUARDADO);
                    table.ForeignKey(
                        name: "FK_publicacion_guardada_publicacion_FK_ID_PUBLICACION",
                        column: x => x.FK_ID_PUBLICACION,
                        principalTable: "publicacion",
                        principalColumn: "ID_PUBLICACION",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_publicacion_guardada_usuario_FK_ID_USUARIO",
                        column: x => x.FK_ID_USUARIO,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "publicacion_etiqueta",
                columns: table => new
                {
                    ID_PUBLICACION_ETIQUETA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_PUBLICACION = table.Column<int>(type: "int", nullable: false),
                    FK_ID_ETIQUETA = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_publicacion_etiqueta", x => x.ID_PUBLICACION_ETIQUETA);
                    table.ForeignKey(
                        name: "FK_publicacion_etiqueta_etiqueta_publicacion_FK_ID_ETIQUETA",
                        column: x => x.FK_ID_ETIQUETA,
                        principalTable: "etiqueta_publicacion",
                        principalColumn: "ID_ETIQUETA",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_publicacion_etiqueta_publicacion_FK_ID_PUBLICACION",
                        column: x => x.FK_ID_PUBLICACION,
                        principalTable: "publicacion",
                        principalColumn: "ID_PUBLICACION",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_etiqueta_publicacion_NOMBRE_ETIQUETA",
                table: "etiqueta_publicacion",
                column: "NOMBRE_ETIQUETA",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_publicacion_etiqueta_FK_ID_ETIQUETA",
                table: "publicacion_etiqueta",
                column: "FK_ID_ETIQUETA");

            migrationBuilder.CreateIndex(
                name: "IX_publicacion_etiqueta_FK_ID_PUBLICACION_FK_ID_ETIQUETA",
                table: "publicacion_etiqueta",
                columns: new[] { "FK_ID_PUBLICACION", "FK_ID_ETIQUETA" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_publicacion_guardada_FK_ID_PUBLICACION_FK_ID_USUARIO",
                table: "publicacion_guardada",
                columns: new[] { "FK_ID_PUBLICACION", "FK_ID_USUARIO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_publicacion_guardada_FK_ID_USUARIO",
                table: "publicacion_guardada",
                column: "FK_ID_USUARIO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "publicacion_etiqueta");

            migrationBuilder.DropTable(
                name: "publicacion_guardada");

            migrationBuilder.DropTable(
                name: "etiqueta_publicacion");

            migrationBuilder.DropColumn(
                name: "FECHA_PUBLICACION_PROGRAMADA",
                table: "publicacion");

            migrationBuilder.DropColumn(
                name: "PUBLICADA",
                table: "publicacion");
        }
    }
}
