using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace test2026.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentToTodo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Todos",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Todos");
        }
    }
}
