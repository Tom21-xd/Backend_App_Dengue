using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_App_Dengue.Migrations
{
    /// <inheritdoc />
    public partial class AddUserApprovalRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "solicitud_aprobacion_usuario",
                columns: table => new
                {
                    ID_SOLICITUD = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_USUARIO = table.Column<int>(type: "int", nullable: false),
                    ESTADO_SOLICITUD = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ROL_SOLICITADO = table.Column<int>(type: "int", nullable: false),
                    MOTIVO_RECHAZO = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FK_ID_ADMIN = table.Column<int>(type: "int", nullable: true),
                    FECHA_SOLICITUD = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FECHA_RESOLUCION = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DATOS_RETHUS = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ERROR_RETHUS = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitud_aprobacion_usuario", x => x.ID_SOLICITUD);
                    table.ForeignKey(
                        name: "FK_solicitud_aprobacion_usuario_rol_ROL_SOLICITADO",
                        column: x => x.ROL_SOLICITADO,
                        principalTable: "rol",
                        principalColumn: "ID_ROL",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_solicitud_aprobacion_usuario_usuario_FK_ID_ADMIN",
                        column: x => x.FK_ID_ADMIN,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_solicitud_aprobacion_usuario_usuario_FK_ID_USUARIO",
                        column: x => x.FK_ID_USUARIO,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_solicitud_aprobacion_usuario_ESTADO_SOLICITUD",
                table: "solicitud_aprobacion_usuario",
                column: "ESTADO_SOLICITUD");

            migrationBuilder.CreateIndex(
                name: "IX_solicitud_aprobacion_usuario_FECHA_SOLICITUD",
                table: "solicitud_aprobacion_usuario",
                column: "FECHA_SOLICITUD");

            migrationBuilder.CreateIndex(
                name: "IX_solicitud_aprobacion_usuario_FK_ID_ADMIN",
                table: "solicitud_aprobacion_usuario",
                column: "FK_ID_ADMIN");

            migrationBuilder.CreateIndex(
                name: "IX_solicitud_aprobacion_usuario_FK_ID_USUARIO",
                table: "solicitud_aprobacion_usuario",
                column: "FK_ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_solicitud_aprobacion_usuario_ROL_SOLICITADO",
                table: "solicitud_aprobacion_usuario",
                column: "ROL_SOLICITADO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "solicitud_aprobacion_usuario");
        }
    }
}
