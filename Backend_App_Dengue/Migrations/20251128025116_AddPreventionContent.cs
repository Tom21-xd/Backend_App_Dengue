using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_App_Dengue.Migrations
{
    /// <inheritdoc />
    public partial class AddPreventionContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prevention_categories",
                columns: table => new
                {
                    ID_CATEGORIA_PREVENCION = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOMBRE_CATEGORIA = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPCION_CATEGORIA = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ICONO = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    COLOR = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ORDEN_VISUALIZACION = table.Column<int>(type: "int", nullable: false),
                    ESTADO_CATEGORIA = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    FECHA_CREACION = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prevention_categories", x => x.ID_CATEGORIA_PREVENCION);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "prevention_category_images",
                columns: table => new
                {
                    ID_IMAGEN_CATEGORIA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_CATEGORIA_PREVENCION = table.Column<int>(type: "int", nullable: false),
                    ID_IMAGEN_MONGO = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TITULO_IMAGEN = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ORDEN_VISUALIZACION = table.Column<int>(type: "int", nullable: false),
                    FECHA_CREACION = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prevention_category_images", x => x.ID_IMAGEN_CATEGORIA);
                    table.ForeignKey(
                        name: "FK_prevention_category_images_prevention_categories_FK_ID_CATEG~",
                        column: x => x.FK_ID_CATEGORIA_PREVENCION,
                        principalTable: "prevention_categories",
                        principalColumn: "ID_CATEGORIA_PREVENCION",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "prevention_items",
                columns: table => new
                {
                    ID_ITEM_PREVENCION = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_CATEGORIA_PREVENCION = table.Column<int>(type: "int", nullable: false),
                    TITULO_ITEM = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPCION_ITEM = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EMOJI_ITEM = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ES_ADVERTENCIA = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ORDEN_VISUALIZACION = table.Column<int>(type: "int", nullable: false),
                    ESTADO_ITEM = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    FECHA_CREACION = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prevention_items", x => x.ID_ITEM_PREVENCION);
                    table.ForeignKey(
                        name: "FK_prevention_items_prevention_categories_FK_ID_CATEGORIA_PREVE~",
                        column: x => x.FK_ID_CATEGORIA_PREVENCION,
                        principalTable: "prevention_categories",
                        principalColumn: "ID_CATEGORIA_PREVENCION",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_prevention_categories_ESTADO_CATEGORIA",
                table: "prevention_categories",
                column: "ESTADO_CATEGORIA");

            migrationBuilder.CreateIndex(
                name: "IX_prevention_categories_NOMBRE_CATEGORIA",
                table: "prevention_categories",
                column: "NOMBRE_CATEGORIA");

            migrationBuilder.CreateIndex(
                name: "IX_prevention_categories_ORDEN_VISUALIZACION",
                table: "prevention_categories",
                column: "ORDEN_VISUALIZACION");

            migrationBuilder.CreateIndex(
                name: "IX_prevention_category_images_FK_ID_CATEGORIA_PREVENCION",
                table: "prevention_category_images",
                column: "FK_ID_CATEGORIA_PREVENCION");

            migrationBuilder.CreateIndex(
                name: "IX_prevention_category_images_ORDEN_VISUALIZACION",
                table: "prevention_category_images",
                column: "ORDEN_VISUALIZACION");

            migrationBuilder.CreateIndex(
                name: "IX_prevention_items_ESTADO_ITEM",
                table: "prevention_items",
                column: "ESTADO_ITEM");

            migrationBuilder.CreateIndex(
                name: "IX_prevention_items_FK_ID_CATEGORIA_PREVENCION",
                table: "prevention_items",
                column: "FK_ID_CATEGORIA_PREVENCION");

            migrationBuilder.CreateIndex(
                name: "IX_prevention_items_ORDEN_VISUALIZACION",
                table: "prevention_items",
                column: "ORDEN_VISUALIZACION");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prevention_category_images");

            migrationBuilder.DropTable(
                name: "prevention_items");

            migrationBuilder.DropTable(
                name: "prevention_categories");
        }
    }
}
