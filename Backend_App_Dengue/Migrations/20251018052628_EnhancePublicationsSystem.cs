using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_App_Dengue.Migrations
{
    /// <inheritdoc />
    public partial class EnhancePublicationsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DIRECCION",
                table: "publicacion",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "ENVIAR_NOTIFICACION_PUSH",
                table: "publicacion",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_ENVIO_NOTIFICACION",
                table: "publicacion",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA_EXPIRACION",
                table: "publicacion",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FIJADA",
                table: "publicacion",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FK_ID_CATEGORIA",
                table: "publicacion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FK_ID_CIUDAD",
                table: "publicacion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FK_ID_DEPARTAMENTO",
                table: "publicacion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LATITUD",
                table: "publicacion",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LONGITUD",
                table: "publicacion",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NIVEL_PRIORIDAD",
                table: "publicacion",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "NOTIFICACION_ENVIADA",
                table: "publicacion",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "categoria_publicacion",
                columns: table => new
                {
                    ID_CATEGORIA_PUBLICACION = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_CATEGORIA = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPCION_CATEGORIA = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ICONO = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    COLOR = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ESTADO_CATEGORIA = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria_publicacion", x => x.ID_CATEGORIA_PUBLICACION);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "comentario_publicacion",
                columns: table => new
                {
                    ID_COMENTARIO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_PUBLICACION = table.Column<int>(type: "int", nullable: false),
                    FK_ID_USUARIO = table.Column<int>(type: "int", nullable: false),
                    CONTENIDO_COMENTARIO = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FK_ID_COMENTARIO_PADRE = table.Column<int>(type: "int", nullable: true),
                    FECHA_COMENTARIO = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ESTADO_COMENTARIO = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comentario_publicacion", x => x.ID_COMENTARIO);
                    table.ForeignKey(
                        name: "FK_comentario_publicacion_comentario_publicacion_FK_ID_COMENTAR~",
                        column: x => x.FK_ID_COMENTARIO_PADRE,
                        principalTable: "comentario_publicacion",
                        principalColumn: "ID_COMENTARIO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_comentario_publicacion_publicacion_FK_ID_PUBLICACION",
                        column: x => x.FK_ID_PUBLICACION,
                        principalTable: "publicacion",
                        principalColumn: "ID_PUBLICACION",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_comentario_publicacion_usuario_FK_ID_USUARIO",
                        column: x => x.FK_ID_USUARIO,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "lectura_publicacion",
                columns: table => new
                {
                    ID_LECTURA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_PUBLICACION = table.Column<int>(type: "int", nullable: false),
                    FK_ID_USUARIO = table.Column<int>(type: "int", nullable: false),
                    FECHA_LECTURA = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TIEMPO_LECTURA_SEGUNDOS = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lectura_publicacion", x => x.ID_LECTURA);
                    table.ForeignKey(
                        name: "FK_lectura_publicacion_publicacion_FK_ID_PUBLICACION",
                        column: x => x.FK_ID_PUBLICACION,
                        principalTable: "publicacion",
                        principalColumn: "ID_PUBLICACION",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lectura_publicacion_usuario_FK_ID_USUARIO",
                        column: x => x.FK_ID_USUARIO,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reaccion_publicacion",
                columns: table => new
                {
                    ID_REACCION = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_PUBLICACION = table.Column<int>(type: "int", nullable: false),
                    FK_ID_USUARIO = table.Column<int>(type: "int", nullable: false),
                    TIPO_REACCION = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FECHA_REACCION = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reaccion_publicacion", x => x.ID_REACCION);
                    table.ForeignKey(
                        name: "FK_reaccion_publicacion_publicacion_FK_ID_PUBLICACION",
                        column: x => x.FK_ID_PUBLICACION,
                        principalTable: "publicacion",
                        principalColumn: "ID_PUBLICACION",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reaccion_publicacion_usuario_FK_ID_USUARIO",
                        column: x => x.FK_ID_USUARIO,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_publicacion_FECHA_PUBLICACION",
                table: "publicacion",
                column: "FECHA_PUBLICACION");

            migrationBuilder.CreateIndex(
                name: "IX_publicacion_FIJADA",
                table: "publicacion",
                column: "FIJADA");

            migrationBuilder.CreateIndex(
                name: "IX_publicacion_FK_ID_CATEGORIA",
                table: "publicacion",
                column: "FK_ID_CATEGORIA");

            migrationBuilder.CreateIndex(
                name: "IX_publicacion_FK_ID_CIUDAD",
                table: "publicacion",
                column: "FK_ID_CIUDAD");

            migrationBuilder.CreateIndex(
                name: "IX_publicacion_FK_ID_DEPARTAMENTO",
                table: "publicacion",
                column: "FK_ID_DEPARTAMENTO");

            migrationBuilder.CreateIndex(
                name: "IX_publicacion_NIVEL_PRIORIDAD",
                table: "publicacion",
                column: "NIVEL_PRIORIDAD");

            migrationBuilder.CreateIndex(
                name: "IX_categoria_publicacion_NOMBRE_CATEGORIA",
                table: "categoria_publicacion",
                column: "NOMBRE_CATEGORIA",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_comentario_publicacion_FK_ID_COMENTARIO_PADRE",
                table: "comentario_publicacion",
                column: "FK_ID_COMENTARIO_PADRE");

            migrationBuilder.CreateIndex(
                name: "IX_comentario_publicacion_FK_ID_PUBLICACION",
                table: "comentario_publicacion",
                column: "FK_ID_PUBLICACION");

            migrationBuilder.CreateIndex(
                name: "IX_comentario_publicacion_FK_ID_USUARIO",
                table: "comentario_publicacion",
                column: "FK_ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_lectura_publicacion_FK_ID_PUBLICACION_FECHA_LECTURA",
                table: "lectura_publicacion",
                columns: new[] { "FK_ID_PUBLICACION", "FECHA_LECTURA" });

            migrationBuilder.CreateIndex(
                name: "IX_lectura_publicacion_FK_ID_USUARIO",
                table: "lectura_publicacion",
                column: "FK_ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_reaccion_publicacion_FK_ID_PUBLICACION_FK_ID_USUARIO",
                table: "reaccion_publicacion",
                columns: new[] { "FK_ID_PUBLICACION", "FK_ID_USUARIO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reaccion_publicacion_FK_ID_USUARIO",
                table: "reaccion_publicacion",
                column: "FK_ID_USUARIO");

            migrationBuilder.AddForeignKey(
                name: "FK_publicacion_categoria_publicacion_FK_ID_CATEGORIA",
                table: "publicacion",
                column: "FK_ID_CATEGORIA",
                principalTable: "categoria_publicacion",
                principalColumn: "ID_CATEGORIA_PUBLICACION",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_publicacion_departamento_FK_ID_DEPARTAMENTO",
                table: "publicacion",
                column: "FK_ID_DEPARTAMENTO",
                principalTable: "departamento",
                principalColumn: "ID_DEPARTAMENTO",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_publicacion_municipio_FK_ID_CIUDAD",
                table: "publicacion",
                column: "FK_ID_CIUDAD",
                principalTable: "municipio",
                principalColumn: "ID_MUNICIPIO",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_publicacion_categoria_publicacion_FK_ID_CATEGORIA",
                table: "publicacion");

            migrationBuilder.DropForeignKey(
                name: "FK_publicacion_departamento_FK_ID_DEPARTAMENTO",
                table: "publicacion");

            migrationBuilder.DropForeignKey(
                name: "FK_publicacion_municipio_FK_ID_CIUDAD",
                table: "publicacion");

            migrationBuilder.DropTable(
                name: "categoria_publicacion");

            migrationBuilder.DropTable(
                name: "comentario_publicacion");

            migrationBuilder.DropTable(
                name: "lectura_publicacion");

            migrationBuilder.DropTable(
                name: "reaccion_publicacion");

            migrationBuilder.DropIndex(
                name: "IX_publicacion_FECHA_PUBLICACION",
                table: "publicacion");

            migrationBuilder.DropIndex(
                name: "IX_publicacion_FIJADA",
                table: "publicacion");

            migrationBuilder.DropIndex(
                name: "IX_publicacion_FK_ID_CATEGORIA",
                table: "publicacion");

            migrationBuilder.DropIndex(
                name: "IX_publicacion_FK_ID_CIUDAD",
                table: "publicacion");

            migrationBuilder.DropIndex(
                name: "IX_publicacion_FK_ID_DEPARTAMENTO",
                table: "publicacion");

            migrationBuilder.DropIndex(
                name: "IX_publicacion_NIVEL_PRIORIDAD",
                table: "publicacion");

            migrationBuilder.DropColumn(
                name: "DIRECCION",
                table: "publicacion");

            migrationBuilder.DropColumn(
                name: "ENVIAR_NOTIFICACION_PUSH",
                table: "publicacion");

            migrationBuilder.DropColumn(
                name: "FECHA_ENVIO_NOTIFICACION",
                table: "publicacion");

            migrationBuilder.DropColumn(
                name: "FECHA_EXPIRACION",
                table: "publicacion");

            migrationBuilder.DropColumn(
                name: "FIJADA",
                table: "publicacion");

            migrationBuilder.DropColumn(
                name: "FK_ID_CATEGORIA",
                table: "publicacion");

            migrationBuilder.DropColumn(
                name: "FK_ID_CIUDAD",
                table: "publicacion");

            migrationBuilder.DropColumn(
                name: "FK_ID_DEPARTAMENTO",
                table: "publicacion");

            migrationBuilder.DropColumn(
                name: "LATITUD",
                table: "publicacion");

            migrationBuilder.DropColumn(
                name: "LONGITUD",
                table: "publicacion");

            migrationBuilder.DropColumn(
                name: "NIVEL_PRIORIDAD",
                table: "publicacion");

            migrationBuilder.DropColumn(
                name: "NOTIFICACION_ENVIADA",
                table: "publicacion");
        }
    }
}
