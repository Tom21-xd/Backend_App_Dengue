using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_App_Dengue.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCaseSystemAndAddPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "IX_casoreportado_FK_ID_TIPODENGUE_ACTUAL",
                table: "casoreportado");

            migrationBuilder.DropColumn(
                name: "DIA_ENFERMEDAD_ACTUAL",
                table: "casoreportado");

            migrationBuilder.RenameColumn(
                name: "FK_ID_ULTIMA_EVOLUCION",
                table: "casoreportado",
                newName: "FK_ID_USUARIO_REGISTRO");

            migrationBuilder.RenameColumn(
                name: "FK_ID_TIPODENGUE_ACTUAL",
                table: "casoreportado",
                newName: "EDAD_PACIENTE");

            migrationBuilder.RenameColumn(
                name: "FK_ID_ESTADO_PACIENTE_ACTUAL",
                table: "casoreportado",
                newName: "ANIO_REPORTE");

            migrationBuilder.RenameIndex(
                name: "IX_casoreportado_FK_ID_ULTIMA_EVOLUCION",
                table: "casoreportado",
                newName: "IX_casoreportado_FK_ID_USUARIO_REGISTRO");

            migrationBuilder.RenameIndex(
                name: "IX_casoreportado_FK_ID_ESTADO_PACIENTE_ACTUAL",
                table: "casoreportado",
                newName: "IX_casoreportado_ANIO_REPORTE");

            migrationBuilder.AlterColumn<int>(
                name: "FK_ID_PACIENTE",
                table: "casoreportado",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "FK_ID_HOSPITAL",
                table: "casoreportado",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "BARRIO_VEREDA",
                table: "casoreportado",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "LATITUD",
                table: "casoreportado",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LONGITUD",
                table: "casoreportado",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NOMBRE_TEMPORAL",
                table: "casoreportado",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "permiso",
                columns: table => new
                {
                    ID_PERMISO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_PERMISO = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPCION_PERMISO = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CODIGO_PERMISO = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CATEGORIA_PERMISO = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_PERMISO = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permiso", x => x.ID_PERMISO);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rol_permiso",
                columns: table => new
                {
                    ID_ROL_PERMISO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_ROL = table.Column<int>(type: "int", nullable: false),
                    FK_ID_PERMISO = table.Column<int>(type: "int", nullable: false),
                    FECHA_ASIGNACION = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ESTADO_ROL_PERMISO = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol_permiso", x => x.ID_ROL_PERMISO);
                    table.ForeignKey(
                        name: "FK_rol_permiso_permiso_FK_ID_PERMISO",
                        column: x => x.FK_ID_PERMISO,
                        principalTable: "permiso",
                        principalColumn: "ID_PERMISO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rol_permiso_rol_FK_ID_ROL",
                        column: x => x.FK_ID_ROL,
                        principalTable: "rol",
                        principalColumn: "ID_ROL",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_casoreportado_BARRIO_VEREDA",
                table: "casoreportado",
                column: "BARRIO_VEREDA");

            migrationBuilder.CreateIndex(
                name: "IX_casoreportado_LATITUD_LONGITUD",
                table: "casoreportado",
                columns: new[] { "LATITUD", "LONGITUD" });

            migrationBuilder.CreateIndex(
                name: "IX_permiso_CATEGORIA_PERMISO",
                table: "permiso",
                column: "CATEGORIA_PERMISO");

            migrationBuilder.CreateIndex(
                name: "IX_permiso_CODIGO_PERMISO",
                table: "permiso",
                column: "CODIGO_PERMISO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rol_permiso_FK_ID_PERMISO",
                table: "rol_permiso",
                column: "FK_ID_PERMISO");

            migrationBuilder.CreateIndex(
                name: "IX_rol_permiso_FK_ID_ROL_FK_ID_PERMISO",
                table: "rol_permiso",
                columns: new[] { "FK_ID_ROL", "FK_ID_PERMISO" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_casoreportado_usuario_FK_ID_USUARIO_REGISTRO",
                table: "casoreportado",
                column: "FK_ID_USUARIO_REGISTRO",
                principalTable: "usuario",
                principalColumn: "ID_USUARIO",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_casoreportado_usuario_FK_ID_USUARIO_REGISTRO",
                table: "casoreportado");

            migrationBuilder.DropTable(
                name: "rol_permiso");

            migrationBuilder.DropTable(
                name: "permiso");

            migrationBuilder.DropIndex(
                name: "IX_casoreportado_BARRIO_VEREDA",
                table: "casoreportado");

            migrationBuilder.DropIndex(
                name: "IX_casoreportado_LATITUD_LONGITUD",
                table: "casoreportado");

            migrationBuilder.DropColumn(
                name: "BARRIO_VEREDA",
                table: "casoreportado");

            migrationBuilder.DropColumn(
                name: "LATITUD",
                table: "casoreportado");

            migrationBuilder.DropColumn(
                name: "LONGITUD",
                table: "casoreportado");

            migrationBuilder.DropColumn(
                name: "NOMBRE_TEMPORAL",
                table: "casoreportado");

            migrationBuilder.RenameColumn(
                name: "FK_ID_USUARIO_REGISTRO",
                table: "casoreportado",
                newName: "FK_ID_ULTIMA_EVOLUCION");

            migrationBuilder.RenameColumn(
                name: "EDAD_PACIENTE",
                table: "casoreportado",
                newName: "FK_ID_TIPODENGUE_ACTUAL");

            migrationBuilder.RenameColumn(
                name: "ANIO_REPORTE",
                table: "casoreportado",
                newName: "FK_ID_ESTADO_PACIENTE_ACTUAL");

            migrationBuilder.RenameIndex(
                name: "IX_casoreportado_FK_ID_USUARIO_REGISTRO",
                table: "casoreportado",
                newName: "IX_casoreportado_FK_ID_ULTIMA_EVOLUCION");

            migrationBuilder.RenameIndex(
                name: "IX_casoreportado_ANIO_REPORTE",
                table: "casoreportado",
                newName: "IX_casoreportado_FK_ID_ESTADO_PACIENTE_ACTUAL");

            migrationBuilder.AlterColumn<int>(
                name: "FK_ID_PACIENTE",
                table: "casoreportado",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FK_ID_HOSPITAL",
                table: "casoreportado",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DIA_ENFERMEDAD_ACTUAL",
                table: "casoreportado",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "estado_paciente",
                columns: table => new
                {
                    ID_ESTADO_PACIENTE = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    COLOR_INDICADOR = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPCION = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_ACTIVO = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NOMBRE_ESTADO_PACIENTE = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NIVEL_GRAVEDAD = table.Column<int>(type: "int", nullable: false)
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
                    FK_ID_ESTADO_PACIENTE = table.Column<int>(type: "int", nullable: false),
                    FK_ID_TIPODENGUE = table.Column<int>(type: "int", nullable: false),
                    TRANSAMINASAS_ALT = table.Column<int>(type: "int", nullable: true),
                    TRANSAMINASAS_AST = table.Column<int>(type: "int", nullable: true),
                    OBSERVACIONES_CLINICAS = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FECHA_REGISTRO = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DIA_ENFERMEDAD = table.Column<int>(type: "int", nullable: true),
                    CAMBIO_TIPO_DENGUE = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EMPEORAMIENTO_DETECTADO = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PRESION_ARTERIAL_DIASTOLICA = table.Column<int>(type: "int", nullable: true),
                    FECHA_EVOLUCION = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FRECUENCIA_CARDIACA = table.Column<int>(type: "int", nullable: true),
                    HEMATOCRITO = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    HEMOGLOBINA = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ESTADO_EVOLUCION = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PROXIMA_CITA = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SATURACION_OXIGENO = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    RECOMENDACIONES_PACIENTE = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PLAQUETAS = table.Column<int>(type: "int", nullable: true),
                    TRATAMIENTO_INDICADO = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SINTOMAS_REPORTADOS = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EXAMENES_SOLICITADOS = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    REQUIERE_HOSPITALIZACION = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    REQUIERE_UCI = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FRECUENCIA_RESPIRATORIA = table.Column<int>(type: "int", nullable: true),
                    PRESION_ARTERIAL_SISTOLICA = table.Column<int>(type: "int", nullable: true),
                    TEMPERATURA = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    FECHA_MODIFICACION = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LEUCOCITOS = table.Column<int>(type: "int", nullable: true)
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
                name: "IX_casoreportado_FK_ID_TIPODENGUE_ACTUAL",
                table: "casoreportado",
                column: "FK_ID_TIPODENGUE_ACTUAL");

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
    }
}
