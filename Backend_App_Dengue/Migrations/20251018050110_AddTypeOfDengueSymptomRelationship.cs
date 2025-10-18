using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_App_Dengue.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeOfDengueSymptomRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tipodengue_sintoma",
                columns: table => new
                {
                    ID_TIPODENGUE_SINTOMA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_TIPODENGUE = table.Column<int>(type: "int", nullable: false),
                    FK_ID_SINTOMA = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipodengue_sintoma", x => x.ID_TIPODENGUE_SINTOMA);
                    table.ForeignKey(
                        name: "FK_tipodengue_sintoma_sintoma_FK_ID_SINTOMA",
                        column: x => x.FK_ID_SINTOMA,
                        principalTable: "sintoma",
                        principalColumn: "ID_SINTOMA",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tipodengue_sintoma_tipodengue_FK_ID_TIPODENGUE",
                        column: x => x.FK_ID_TIPODENGUE,
                        principalTable: "tipodengue",
                        principalColumn: "ID_TIPODENGUE",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_tipodengue_sintoma_FK_ID_SINTOMA",
                table: "tipodengue_sintoma",
                column: "FK_ID_SINTOMA");

            migrationBuilder.CreateIndex(
                name: "IX_tipodengue_sintoma_FK_ID_TIPODENGUE_FK_ID_SINTOMA",
                table: "tipodengue_sintoma",
                columns: new[] { "FK_ID_TIPODENGUE", "FK_ID_SINTOMA" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tipodengue_sintoma");
        }
    }
}
