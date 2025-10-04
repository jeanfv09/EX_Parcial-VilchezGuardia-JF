using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EX_Parcial_VilchezGuardia_JF.Data.Migrations
{
    /// <inheritdoc />
    public partial class AjustesModeloInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_UsuarioId",
                table: "Matriculas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matriculas_AspNetUsers_UsuarioId",
                table: "Matriculas",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matriculas_AspNetUsers_UsuarioId",
                table: "Matriculas");

            migrationBuilder.DropIndex(
                name: "IX_Matriculas_UsuarioId",
                table: "Matriculas");
        }
    }
}
