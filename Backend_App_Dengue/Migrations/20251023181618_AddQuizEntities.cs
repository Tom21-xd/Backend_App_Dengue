using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_App_Dengue.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quiz_attempts",
                columns: table => new
                {
                    ID_INTENTO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_USUARIO = table.Column<int>(type: "int", nullable: false),
                    FECHA_INICIO = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FECHA_FINALIZACION = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PUNTUACION_OBTENIDA = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TOTAL_PREGUNTAS = table.Column<int>(type: "int", nullable: false),
                    RESPUESTAS_CORRECTAS = table.Column<int>(type: "int", nullable: false),
                    RESPUESTAS_INCORRECTAS = table.Column<int>(type: "int", nullable: false),
                    TIEMPO_TOTAL_SEGUNDOS = table.Column<int>(type: "int", nullable: false),
                    ESTADO_INTENTO = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_attempts", x => x.ID_INTENTO);
                    table.ForeignKey(
                        name: "FK_quiz_attempts_usuario_FK_ID_USUARIO",
                        column: x => x.FK_ID_USUARIO,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "quiz_categories",
                columns: table => new
                {
                    ID_CATEGORIA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_CATEGORIA = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPCION = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ICONO = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ORDEN_VISUALIZACION = table.Column<int>(type: "int", nullable: false),
                    ESTADO_CATEGORIA = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FECHA_CREACION = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_categories", x => x.ID_CATEGORIA);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "certificates",
                columns: table => new
                {
                    ID_CERTIFICADO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_USUARIO = table.Column<int>(type: "int", nullable: false),
                    FK_ID_INTENTO = table.Column<int>(type: "int", nullable: false),
                    CODIGO_VERIFICACION = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FECHA_EMISION = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PUNTUACION_OBTENIDA = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ESTADO_CERTIFICADO = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    URL_PDF_CERTIFICADO = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certificates", x => x.ID_CERTIFICADO);
                    table.ForeignKey(
                        name: "FK_certificates_quiz_attempts_FK_ID_INTENTO",
                        column: x => x.FK_ID_INTENTO,
                        principalTable: "quiz_attempts",
                        principalColumn: "ID_INTENTO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_certificates_usuario_FK_ID_USUARIO",
                        column: x => x.FK_ID_USUARIO,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "quiz_questions",
                columns: table => new
                {
                    ID_PREGUNTA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_CATEGORIA = table.Column<int>(type: "int", nullable: false),
                    TEXTO_PREGUNTA = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TIPO_PREGUNTA = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DIFICULTAD = table.Column<int>(type: "int", nullable: false),
                    PUNTOS = table.Column<int>(type: "int", nullable: false),
                    EXPLICACION_RESPUESTA = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_PREGUNTA = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FECHA_CREACION = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FECHA_MODIFICACION = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_questions", x => x.ID_PREGUNTA);
                    table.ForeignKey(
                        name: "FK_quiz_questions_quiz_categories_FK_ID_CATEGORIA",
                        column: x => x.FK_ID_CATEGORIA,
                        principalTable: "quiz_categories",
                        principalColumn: "ID_CATEGORIA",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "quiz_answers",
                columns: table => new
                {
                    ID_RESPUESTA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_PREGUNTA = table.Column<int>(type: "int", nullable: false),
                    TEXTO_RESPUESTA = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ES_CORRECTA = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ORDEN_RESPUESTA = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_answers", x => x.ID_RESPUESTA);
                    table.ForeignKey(
                        name: "FK_quiz_answers_quiz_questions_FK_ID_PREGUNTA",
                        column: x => x.FK_ID_PREGUNTA,
                        principalTable: "quiz_questions",
                        principalColumn: "ID_PREGUNTA",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "quiz_user_answers",
                columns: table => new
                {
                    ID_RESPUESTA_USUARIO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_INTENTO = table.Column<int>(type: "int", nullable: false),
                    FK_ID_PREGUNTA = table.Column<int>(type: "int", nullable: false),
                    FK_ID_RESPUESTA_SELECCIONADA = table.Column<int>(type: "int", nullable: false),
                    ES_CORRECTA = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FECHA_RESPUESTA = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TIEMPO_RESPUESTA_SEGUNDOS = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_user_answers", x => x.ID_RESPUESTA_USUARIO);
                    table.ForeignKey(
                        name: "FK_quiz_user_answers_quiz_answers_FK_ID_RESPUESTA_SELECCIONADA",
                        column: x => x.FK_ID_RESPUESTA_SELECCIONADA,
                        principalTable: "quiz_answers",
                        principalColumn: "ID_RESPUESTA",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quiz_user_answers_quiz_attempts_FK_ID_INTENTO",
                        column: x => x.FK_ID_INTENTO,
                        principalTable: "quiz_attempts",
                        principalColumn: "ID_INTENTO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quiz_user_answers_quiz_questions_FK_ID_PREGUNTA",
                        column: x => x.FK_ID_PREGUNTA,
                        principalTable: "quiz_questions",
                        principalColumn: "ID_PREGUNTA",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_certificates_CODIGO_VERIFICACION",
                table: "certificates",
                column: "CODIGO_VERIFICACION",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_certificates_FK_ID_INTENTO",
                table: "certificates",
                column: "FK_ID_INTENTO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_certificates_FK_ID_USUARIO",
                table: "certificates",
                column: "FK_ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_answers_FK_ID_PREGUNTA",
                table: "quiz_answers",
                column: "FK_ID_PREGUNTA");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_ESTADO_INTENTO",
                table: "quiz_attempts",
                column: "ESTADO_INTENTO");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_FECHA_INICIO",
                table: "quiz_attempts",
                column: "FECHA_INICIO");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_FK_ID_USUARIO",
                table: "quiz_attempts",
                column: "FK_ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_categories_NOMBRE_CATEGORIA",
                table: "quiz_categories",
                column: "NOMBRE_CATEGORIA",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_ESTADO_PREGUNTA",
                table: "quiz_questions",
                column: "ESTADO_PREGUNTA");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_FK_ID_CATEGORIA",
                table: "quiz_questions",
                column: "FK_ID_CATEGORIA");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_user_answers_FK_ID_INTENTO",
                table: "quiz_user_answers",
                column: "FK_ID_INTENTO");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_user_answers_FK_ID_PREGUNTA",
                table: "quiz_user_answers",
                column: "FK_ID_PREGUNTA");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_user_answers_FK_ID_RESPUESTA_SELECCIONADA",
                table: "quiz_user_answers",
                column: "FK_ID_RESPUESTA_SELECCIONADA");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "certificates");

            migrationBuilder.DropTable(
                name: "quiz_user_answers");

            migrationBuilder.DropTable(
                name: "quiz_answers");

            migrationBuilder.DropTable(
                name: "quiz_attempts");

            migrationBuilder.DropTable(
                name: "quiz_questions");

            migrationBuilder.DropTable(
                name: "quiz_categories");
        }
    }
}
