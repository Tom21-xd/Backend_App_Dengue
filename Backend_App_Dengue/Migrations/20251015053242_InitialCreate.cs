using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_App_Dengue.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "departamento",
                columns: table => new
                {
                    ID_DEPARTAMENTO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_DEPARTAMENTO = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_DEPARTAMENTO = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departamento", x => x.ID_DEPARTAMENTO);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "estadocaso",
                columns: table => new
                {
                    ID_ESTADOCASO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_ESTADOCASO = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_ESTADOCASO = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estadocaso", x => x.ID_ESTADOCASO);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "genero",
                columns: table => new
                {
                    ID_GENERO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_GENERO = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_GENERO = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_genero", x => x.ID_GENERO);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rol",
                columns: table => new
                {
                    ID_ROL = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_ROL = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_ROL = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol", x => x.ID_ROL);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sintoma",
                columns: table => new
                {
                    ID_SINTOMA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_SINTOMA = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_SINTOMA = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sintoma", x => x.ID_SINTOMA);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tipodengue",
                columns: table => new
                {
                    ID_TIPODENGUE = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_TIPODENGUE = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_TIPODENGUE = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipodengue", x => x.ID_TIPODENGUE);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tiposangre",
                columns: table => new
                {
                    ID_TIPOSANGRE = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_TIPOSANGRE = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_TIPOSANGRE = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tiposangre", x => x.ID_TIPOSANGRE);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "municipio",
                columns: table => new
                {
                    ID_MUNICIPIO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_MUNICIPIO = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FK_ID_DEPARTAMENTO = table.Column<int>(type: "int", nullable: false),
                    ESTADO_MUNICIPIO = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_municipio", x => x.ID_MUNICIPIO);
                    table.ForeignKey(
                        name: "FK_municipio_departamento_FK_ID_DEPARTAMENTO",
                        column: x => x.FK_ID_DEPARTAMENTO,
                        principalTable: "departamento",
                        principalColumn: "ID_DEPARTAMENTO",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hospital",
                columns: table => new
                {
                    ID_HOSPITAL = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_HOSPITAL = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DIRECCION_HOSPITAL = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FK_ID_MUNICIPIO = table.Column<int>(type: "int", nullable: false),
                    LATITUD_HOSPITAL = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LONGITUD_HOSPITAL = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IMAGEN_HOSPITAL = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_HOSPITAL = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hospital", x => x.ID_HOSPITAL);
                    table.ForeignKey(
                        name: "FK_hospital_municipio_FK_ID_MUNICIPIO",
                        column: x => x.FK_ID_MUNICIPIO,
                        principalTable: "municipio",
                        principalColumn: "ID_MUNICIPIO",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    ID_USUARIO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_USUARIO = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CORREO_USUARIO = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CONTRASENIA_USUARIO = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DIRECCION_USUARIO = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FK_ID_ROL = table.Column<int>(type: "int", nullable: false),
                    FK_ID_MUNICIPIO = table.Column<int>(type: "int", nullable: true),
                    FK_ID_TIPOSANGRE = table.Column<int>(type: "int", nullable: false),
                    FK_ID_GENERO = table.Column<int>(type: "int", nullable: false),
                    ESTADO_USUARIO = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.ID_USUARIO);
                    table.ForeignKey(
                        name: "FK_usuario_genero_FK_ID_GENERO",
                        column: x => x.FK_ID_GENERO,
                        principalTable: "genero",
                        principalColumn: "ID_GENERO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_municipio_FK_ID_MUNICIPIO",
                        column: x => x.FK_ID_MUNICIPIO,
                        principalTable: "municipio",
                        principalColumn: "ID_MUNICIPIO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_rol_FK_ID_ROL",
                        column: x => x.FK_ID_ROL,
                        principalTable: "rol",
                        principalColumn: "ID_ROL",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_tiposangre_FK_ID_TIPOSANGRE",
                        column: x => x.FK_ID_TIPOSANGRE,
                        principalTable: "tiposangre",
                        principalColumn: "ID_TIPOSANGRE",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "casoreportado",
                columns: table => new
                {
                    ID_CASO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DESCRIPCION_CASOREPORTADO = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FECHA_CASOREPORTADO = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FK_ID_ESTADOCASO = table.Column<int>(type: "int", nullable: false),
                    FK_ID_HOSPITAL = table.Column<int>(type: "int", nullable: false),
                    FK_ID_TIPODENGUE = table.Column<int>(type: "int", nullable: false),
                    FK_ID_PACIENTE = table.Column<int>(type: "int", nullable: false),
                    FK_ID_PERSONALMEDICO = table.Column<int>(type: "int", nullable: true),
                    FECHAFINALIZACION_CASO = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DIRECCION_CASOREPORTADO = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_CASO = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_casoreportado", x => x.ID_CASO);
                    table.ForeignKey(
                        name: "FK_casoreportado_estadocaso_FK_ID_ESTADOCASO",
                        column: x => x.FK_ID_ESTADOCASO,
                        principalTable: "estadocaso",
                        principalColumn: "ID_ESTADOCASO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_casoreportado_hospital_FK_ID_HOSPITAL",
                        column: x => x.FK_ID_HOSPITAL,
                        principalTable: "hospital",
                        principalColumn: "ID_HOSPITAL",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_casoreportado_tipodengue_FK_ID_TIPODENGUE",
                        column: x => x.FK_ID_TIPODENGUE,
                        principalTable: "tipodengue",
                        principalColumn: "ID_TIPODENGUE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_casoreportado_usuario_FK_ID_PACIENTE",
                        column: x => x.FK_ID_PACIENTE,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_casoreportado_usuario_FK_ID_PERSONALMEDICO",
                        column: x => x.FK_ID_PERSONALMEDICO,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fcmtoken",
                columns: table => new
                {
                    ID_FCMTOKEN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_USUARIO = table.Column<int>(type: "int", nullable: false),
                    FCM_TOKEN = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FECHA_REGISTRO = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FECHA_ACTUALIZACION = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fcmtoken", x => x.ID_FCMTOKEN);
                    table.ForeignKey(
                        name: "FK_fcmtoken_usuario_FK_ID_USUARIO",
                        column: x => x.FK_ID_USUARIO,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notificacion",
                columns: table => new
                {
                    ID_NOTIFICACION = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CONTENIDO_NOTIFICACION = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FECHA_NOTIFICACION = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FK_ID_USUARIO = table.Column<int>(type: "int", nullable: false),
                    LEIDA_NOTIFICACION = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ESTADO_NOTIFICACION = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacion", x => x.ID_NOTIFICACION);
                    table.ForeignKey(
                        name: "FK_notificacion_usuario_FK_ID_USUARIO",
                        column: x => x.FK_ID_USUARIO,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "publicacion",
                columns: table => new
                {
                    ID_PUBLICACION = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TITULO_PUBLICACION = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPCION_PUBLICACION = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FECHA_PUBLICACION = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FK_ID_USUARIO = table.Column<int>(type: "int", nullable: false),
                    ID_IMAGEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_PUBLICACION = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_publicacion", x => x.ID_PUBLICACION);
                    table.ForeignKey(
                        name: "FK_publicacion_usuario_FK_ID_USUARIO",
                        column: x => x.FK_ID_USUARIO,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_casoreportado_FK_ID_ESTADOCASO",
                table: "casoreportado",
                column: "FK_ID_ESTADOCASO");

            migrationBuilder.CreateIndex(
                name: "IX_casoreportado_FK_ID_HOSPITAL",
                table: "casoreportado",
                column: "FK_ID_HOSPITAL");

            migrationBuilder.CreateIndex(
                name: "IX_casoreportado_FK_ID_PACIENTE",
                table: "casoreportado",
                column: "FK_ID_PACIENTE");

            migrationBuilder.CreateIndex(
                name: "IX_casoreportado_FK_ID_PERSONALMEDICO",
                table: "casoreportado",
                column: "FK_ID_PERSONALMEDICO");

            migrationBuilder.CreateIndex(
                name: "IX_casoreportado_FK_ID_TIPODENGUE",
                table: "casoreportado",
                column: "FK_ID_TIPODENGUE");

            migrationBuilder.CreateIndex(
                name: "IX_fcmtoken_FK_ID_USUARIO",
                table: "fcmtoken",
                column: "FK_ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_hospital_FK_ID_MUNICIPIO",
                table: "hospital",
                column: "FK_ID_MUNICIPIO");

            migrationBuilder.CreateIndex(
                name: "IX_municipio_FK_ID_DEPARTAMENTO",
                table: "municipio",
                column: "FK_ID_DEPARTAMENTO");

            migrationBuilder.CreateIndex(
                name: "IX_notificacion_FK_ID_USUARIO",
                table: "notificacion",
                column: "FK_ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_publicacion_FK_ID_USUARIO",
                table: "publicacion",
                column: "FK_ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_CORREO_USUARIO",
                table: "usuario",
                column: "CORREO_USUARIO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_FK_ID_GENERO",
                table: "usuario",
                column: "FK_ID_GENERO");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_FK_ID_MUNICIPIO",
                table: "usuario",
                column: "FK_ID_MUNICIPIO");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_FK_ID_ROL",
                table: "usuario",
                column: "FK_ID_ROL");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_FK_ID_TIPOSANGRE",
                table: "usuario",
                column: "FK_ID_TIPOSANGRE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "casoreportado");

            migrationBuilder.DropTable(
                name: "fcmtoken");

            migrationBuilder.DropTable(
                name: "notificacion");

            migrationBuilder.DropTable(
                name: "publicacion");

            migrationBuilder.DropTable(
                name: "sintoma");

            migrationBuilder.DropTable(
                name: "estadocaso");

            migrationBuilder.DropTable(
                name: "hospital");

            migrationBuilder.DropTable(
                name: "tipodengue");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "genero");

            migrationBuilder.DropTable(
                name: "municipio");

            migrationBuilder.DropTable(
                name: "rol");

            migrationBuilder.DropTable(
                name: "tiposangre");

            migrationBuilder.DropTable(
                name: "departamento");
        }
    }
}
