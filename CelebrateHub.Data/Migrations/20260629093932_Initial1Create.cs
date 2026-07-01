using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CelebrateHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial1Create : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeId", "AnniversaryDate", "CreatedDate", "DateOfBirth", "Department", "Email", "IsActive", "Name", "PasswordHash", "Project", "Role" },
                values: new object[,]
                {
                    { 2, new DateTime(2020, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1995, 7, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR", "priya@portal.com", true, "Priya Sharma", "$2a$11$gWPDg.9HW.1mufDLgoUKGuMlOxr16YVWMNIu7njkzMd38/StvPgiW", null, "User" },
                    { 3, new DateTime(2019, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1992, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Engineering", "rahul@portal.com", true, "Rahul Verma", "$2a$11$gWPDg.9HW.1mufDLgoUKGuMlOxr16YVWMNIu7njkzMd38/StvPgiW", null, "User" },
                    { 4, new DateTime(2021, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1988, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Finance", "anjali@portal.com", true, "Anjali Singh", "$2a$11$gWPDg.9HW.1mufDLgoUKGuMlOxr16YVWMNIu7njkzMd38/StvPgiW", null, "User" },
                    { 5, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1985, 11, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sales", "vikram@portal.com", true, "Vikram Mehta", "$2a$11$gWPDg.9HW.1mufDLgoUKGuMlOxr16YVWMNIu7njkzMd38/StvPgiW", null, "User" }
                });
        }
    }
}
