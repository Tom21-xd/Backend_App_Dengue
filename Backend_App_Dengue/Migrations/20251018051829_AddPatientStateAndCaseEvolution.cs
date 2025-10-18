using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_App_Dengue.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientStateAndCaseEvolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DIA_ENFERMEDAD_ACTUAL",
                table: "casoreportado",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FK_ID_ESTADO_PACIENTE_ACTUAL",
                table: "casoreportado",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FK_ID_TIPODENGUE_ACTUAL",
                table: "casoreportado",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FK_ID_ULTIMA_EVOLUCION",
                table: "casoreportado",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "estado_paciente",
                columns: table => new
                {
                    ID_ESTADO_PACIENTE = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_ESTADO_PACIENTE = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPCION = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NIVEL_GRAVEDAD = table.Column<int>(type: "int", nullable: false),
                    COLOR_INDICADOR = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_ACTIVO = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estado_paciente", x => x.ID_ESTADO_PACIENTE);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "evolucion_caso",
                columns: table => new
                {
                    ID_EVOLUCION = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_CASO = table.Column<int>(type: "int", nullable: false),
                    FK_ID_MEDICO = table.Column<int>(type: "int", nullable: false),
                    FK_ID_TIPODENGUE = table.Column<int>(type: "int", nullable: false),
                    FK_ID_ESTADO_PACIENTE = table.Column<int>(type: "int", nullable: false),
                    FECHA_EVOLUCION = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DIA_ENFERMEDAD = table.Column<int>(type: "int", nullable: true),
                    SINTOMAS_REPORTADOS = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TEMPERATURA = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    PRESION_ARTERIAL_SISTOLICA = table.Column<int>(type: "int", nullable: true),
                    PRESION_ARTERIAL_DIASTOLICA = table.Column<int>(type: "int", nullable: true),
                    FRECUENCIA_CARDIACA = table.Column<int>(type: "int", nullable: true),
                    FRECUENCIA_RESPIRATORIA = table.Column<int>(type: "int", nullable: true),
                    SATURACION_OXIGENO = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    PLAQUETAS = table.Column<int>(type: "int", nullable: true),
                    HEMATOCRITO = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    LEUCOCITOS = table.Column<int>(type: "int", nullable: true),
                    HEMOGLOBINA = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    TRANSAMINASAS_AST = table.Column<int>(type: "int", nullable: true),
                    TRANSAMINASAS_ALT = table.Column<int>(type: "int", nullable: true),
                    OBSERVACIONES_CLINICAS = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TRATAMIENTO_INDICADO = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EXAMENES_SOLICITADOS = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CAMBIO_TIPO_DENGUE = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EMPEORAMIENTO_DETECTADO = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    REQUIERE_HOSPITALIZACION = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    REQUIERE_UCI = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PROXIMA_CITA = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RECOMENDACIONES_PACIENTE = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_EVOLUCION = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FECHA_REGISTRO = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FECHA_MODIFICACION = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evolucion_caso", x => x.ID_EVOLUCION);
                    table.ForeignKey(
                        name: "FK_evolucion_caso_casoreportado_FK_ID_CASO",
                        column: x => x.FK_ID_CASO,
                        principalTable: "casoreportado",
                        principalColumn: "ID_CASO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_evolucion_caso_estado_paciente_FK_ID_ESTADO_PACIENTE",
                        column: x => x.FK_ID_ESTADO_PACIENTE,
                        principalTable: "estado_paciente",
                        principalColumn: "ID_ESTADO_PACIENTE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_evolucion_caso_tipodengue_FK_ID_TIPODENGUE",
                        column: x => x.FK_ID_TIPODENGUE,
                        principalTable: "tipodengue",
                        principalColumn: "ID_TIPODENGUE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_evolucion_caso_usuario_FK_ID_MEDICO",
                        column: x => x.FK_ID_MEDICO,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_casoreportado_FK_ID_ESTADO_PACIENTE_ACTUAL",
                table: "casoreportado",
                column: "FK_ID_ESTADO_PACIENTE_ACTUAL");

            migrationBuilder.CreateIndex(
                name: "IX_casoreportado_FK_ID_TIPODENGUE_ACTUAL",
                table: "casoreportado",
                column: "FK_ID_TIPODENGUE_ACTUAL");

            migrationBuilder.CreateIndex(
                name: "IX_casoreportado_FK_ID_ULTIMA_EVOLUCION",
                table: "casoreportado",
                column: "FK_ID_ULTIMA_EVOLUCION");

            migrationBuilder.CreateIndex(
                name: "IX_estado_paciente_NOMBRE_ESTADO_PACIENTE",
                table: "estado_paciente",
                column: "NOMBRE_ESTADO_PACIENTE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_evolucion_caso_FECHA_EVOLUCION",
                table: "evolucion_caso",
                column: "FECHA_EVOLUCION");

            migrationBuilder.CreateIndex(
                name: "IX_evolucion_caso_FK_ID_CASO_FECHA_EVOLUCION",
                table: "evolucion_caso",
                columns: new[] { "FK_ID_CASO", "FECHA_EVOLUCION" });

            migrationBuilder.CreateIndex(
                name: "IX_evolucion_caso_FK_ID_ESTADO_PACIENTE",
                table: "evolucion_caso",
                column: "FK_ID_ESTADO_PACIENTE");

            migrationBuilder.CreateIndex(
                name: "IX_evolucion_caso_FK_ID_MEDICO",
                table: "evolucion_caso",
                column: "FK_ID_MEDICO");

            migrationBuilder.CreateIndex(
                name: "IX_evolucion_caso_FK_ID_TIPODENGUE",
                table: "evolucion_caso",
                column: "FK_ID_TIPODENGUE");

            migrationBuilder.AddForeignKey(
                name: "FK_casoreportado_estado_paciente_FK_ID_ESTADO_PACIENTE_ACTUAL",
                table: "casoreportado",
                column: "FK_ID_ESTADO_PACIENTE_ACTUAL",
                principalTable: "estado_paciente",
                principalColumn: "ID_ESTADO_PACIENTE",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_casoreportado_evolucion_caso_FK_ID_ULTIMA_EVOLUCION",
                table: "casoreportado",
                column: "FK_ID_ULTIMA_EVOLUCION",
                principalTable: "evolucion_caso",
                principalColumn: "ID_EVOLUCION",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_casoreportado_tipodengue_FK_ID_TIPODENGUE_ACTUAL",
                table: "casoreportado",
                column: "FK_ID_TIPODENGUE_ACTUAL",
                principalTable: "tipodengue",
                principalColumn: "ID_TIPODENGUE",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_casoreportado_estado_paciente_FK_ID_ESTADO_PACIENTE_ACTUAL",
                table: "casoreportado");

            migrationBuilder.DropForeignKey(
                name: "FK_casoreportado_evolucion_caso_FK_ID_ULTIMA_EVOLUCION",
                table: "casoreportado");

            migrationBuilder.DropForeignKey(
                name: "FK_casoreportado_tipodengue_FK_ID_TIPODENGUE_ACTUAL",
                table: "casoreportado");

            migrationBuilder.DropTable(
                name: "evolucion_caso");

            migrationBuilder.DropTable(
                name: "estado_paciente");

            migrationBuilder.DropIndex(
                name: "IX_casoreportado_FK_ID_ESTADO_PACIENTE_ACTUAL",
                table: "casoreportado");

            migrationBuilder.DropIndex(
                name: "IX_casoreportado_FK_ID_TIPODENGUE_ACTUAL",
                table: "casoreportado");

            migrationBuilder.DropIndex(
                name: "IX_casoreportado_FK_ID_ULTIMA_EVOLUCION",
                table: "casoreportado");

            migrationBuilder.DropColumn(
                name: "DIA_ENFERMEDAD_ACTUAL",
                table: "casoreportado");

            migrationBuilder.DropColumn(
                name: "FK_ID_ESTADO_PACIENTE_ACTUAL",
                table: "casoreportado");

            migrationBuilder.DropColumn(
                name: "FK_ID_TIPODENGUE_ACTUAL",
                table: "casoreportado");

            migrationBuilder.DropColumn(
                name: "FK_ID_ULTIMA_EVOLUCION",
                table: "casoreportado");
        }
    }
}
