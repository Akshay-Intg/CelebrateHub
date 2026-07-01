using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CelebrateHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnniversaryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Project = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                });

            migrationBuilder.CreateTable(
                name: "PartyEvents",
                columns: table => new
                {
                    PartyEventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DonePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TotalVotes = table.Column<int>(type: "int", nullable: false),
                    DoneVotes = table.Column<int>(type: "int", nullable: false),
                    PendingVotes = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyEvents", x => x.PartyEventId);
                    table.ForeignKey(
                        name: "FK_PartyEvents_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartyVotes",
                columns: table => new
                {
                    PartyVoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyEventId = table.Column<int>(type: "int", nullable: false),
                    VoterEmployeeId = table.Column<int>(type: "int", nullable: false),
                    VoteType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VotedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyVotes", x => x.PartyVoteId);
                    table.ForeignKey(
                        name: "FK_PartyVotes_Employees_VoterEmployeeId",
                        column: x => x.VoterEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_PartyVotes_PartyEvents_PartyEventId",
                        column: x => x.PartyEventId,
                        principalTable: "PartyEvents",
                        principalColumn: "PartyEventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeId", "AnniversaryDate", "CreatedDate", "DateOfBirth", "Department", "Email", "IsActive", "Name", "PasswordHash", "Project", "Role" },
                values: new object[,]
                {
                    { 1, new DateTime(2018, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1990, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "IT", "admin@portal.com", true, "Admin User", "$2a$11$CQsKloLgaA9GRRtcPnfxpee8lmuk.RCJhIdwg6CtLJ6vDN6iaL5.m", null, "Admin" },
                    { 2, new DateTime(2020, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1995, 7, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR", "priya@portal.com", true, "Priya Sharma", "$2a$11$gWPDg.9HW.1mufDLgoUKGuMlOxr16YVWMNIu7njkzMd38/StvPgiW", null, "User" },
                    { 3, new DateTime(2019, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1992, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Engineering", "rahul@portal.com", true, "Rahul Verma", "$2a$11$gWPDg.9HW.1mufDLgoUKGuMlOxr16YVWMNIu7njkzMd38/StvPgiW", null, "User" },
                    { 4, new DateTime(2021, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1988, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Finance", "anjali@portal.com", true, "Anjali Singh", "$2a$11$gWPDg.9HW.1mufDLgoUKGuMlOxr16YVWMNIu7njkzMd38/StvPgiW", null, "User" },
                    { 5, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1985, 11, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sales", "vikram@portal.com", true, "Vikram Mehta", "$2a$11$gWPDg.9HW.1mufDLgoUKGuMlOxr16YVWMNIu7njkzMd38/StvPgiW", null, "User" }
                });

            migrationBuilder.InsertData(
                table: "PartyEvents",
                columns: new[] { "PartyEventId", "CreatedDate", "DonePercentage", "DoneVotes", "EmployeeId", "EventDate", "EventType", "IsActive", "PendingVotes", "Status", "TotalVotes" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 0m, 0, 1, new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Birthday", true, 0, "Pending", 0 },
                    { 2, new DateTime(2025, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 0m, 0, 1, new DateTime(2025, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Anniversary", true, 0, "Pending", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyEvents_EmployeeId_EventType_EventDate",
                table: "PartyEvents",
                columns: new[] { "EmployeeId", "EventType", "EventDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyVotes_PartyEventId_VoterEmployeeId",
                table: "PartyVotes",
                columns: new[] { "PartyEventId", "VoterEmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyVotes_VoterEmployeeId",
                table: "PartyVotes",
                column: "VoterEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartyVotes");

            migrationBuilder.DropTable(
                name: "PartyEvents");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
