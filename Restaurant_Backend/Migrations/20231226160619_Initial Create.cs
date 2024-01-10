using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant_Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Adresse = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admins", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Adresse = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "dishes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dishes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "tokenExists",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tokenExists", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "dishes");

            migrationBuilder.DropTable(
                name: "tokenExists");
        }
    }
}
