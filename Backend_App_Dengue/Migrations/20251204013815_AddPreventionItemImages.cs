using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_App_Dengue.Migrations
{
    /// <inheritdoc />
    public partial class AddPreventionItemImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prevention_item_images",
                columns: table => new
                {
                    ID_IMAGEN_ITEM = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_ITEM_PREVENCION = table.Column<int>(type: "int", nullable: false),
                    ID_IMAGEN_MONGO = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TITULO_IMAGEN = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ORDEN_VISUALIZACION = table.Column<int>(type: "int", nullable: false),
                    FECHA_CREACION = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prevention_item_images", x => x.ID_IMAGEN_ITEM);
                    table.ForeignKey(
                        name: "FK_prevention_item_images_prevention_items_FK_ID_ITEM_PREVENCION",
                        column: x => x.FK_ID_ITEM_PREVENCION,
                        principalTable: "prevention_items",
                        principalColumn: "ID_ITEM_PREVENCION",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_prevention_item_images_FK_ID_ITEM_PREVENCION",
                table: "prevention_item_images",
                column: "FK_ID_ITEM_PREVENCION");

            migrationBuilder.CreateIndex(
                name: "IX_prevention_item_images_ORDEN_VISUALIZACION",
                table: "prevention_item_images",
                column: "ORDEN_VISUALIZACION");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prevention_item_images");
        }
    }
}
