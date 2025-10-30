using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_App_Dengue.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    ID_REFRESH_TOKEN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FK_ID_USUARIO = table.Column<int>(type: "int", nullable: false),
                    TOKEN = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EXPIRES_AT = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IS_REVOKED = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    REVOKED_AT = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DEVICE_INFO = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.ID_REFRESH_TOKEN);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_usuario_FK_ID_USUARIO",
                        column: x => x.FK_ID_USUARIO,
                        principalTable: "usuario",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_EXPIRES_AT",
                table: "refresh_tokens",
                column: "EXPIRES_AT");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_FK_ID_USUARIO",
                table: "refresh_tokens",
                column: "FK_ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_TOKEN",
                table: "refresh_tokens",
                column: "TOKEN",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refresh_tokens");
        }
    }
}
