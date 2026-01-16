using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AccountManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialAllInOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_accounts_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "User" },
                    { 2, "Admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_UserId",
                table: "accounts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_Name",
                table: "roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_RoleId",
                table: "users",
                column: "RoleId");



            // 1) Audit tablica: account_audit_logs
            migrationBuilder.CreateTable(
                name: "account_audit_logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),

                    AccountId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),

                    Action = table.Column<string>(type: "text", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),

                    OldData = table.Column<string>(type: "text", nullable: true),
                    NewData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_audit_logs_AccountId",
                table: "account_audit_logs",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_account_audit_logs_UserId",
                table: "account_audit_logs",
                column: "UserId");


            // 2) Predikat: user_has_account(p_user_id) RETURNS boolean
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION user_has_account(p_user_id integer)
RETURNS boolean
LANGUAGE sql
AS $$
    SELECT EXISTS (
        SELECT 1 FROM accounts WHERE ""UserId"" = p_user_id
    );
$$;
");



            // 3) Audit trigger funkcija: fn_account_audit()
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION fn_account_audit()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF (TG_OP = 'INSERT') THEN
        INSERT INTO account_audit_logs(""AccountId"", ""UserId"", ""Action"", ""ChangedAt"", ""OldData"", ""NewData"")
        VALUES (NEW.""Id"", NEW.""UserId"", 'INSERT', now(), NULL, row_to_json(NEW)::text);
        RETURN NEW;

    ELSIF (TG_OP = 'UPDATE') THEN
        INSERT INTO account_audit_logs(""AccountId"", ""UserId"", ""Action"", ""ChangedAt"", ""OldData"", ""NewData"")
        VALUES (NEW.""Id"", NEW.""UserId"", 'UPDATE', now(), row_to_json(OLD)::text, row_to_json(NEW)::text);
        RETURN NEW;

    ELSIF (TG_OP = 'DELETE') THEN
        INSERT INTO account_audit_logs(""AccountId"", ""UserId"", ""Action"", ""ChangedAt"", ""OldData"", ""NewData"")
        VALUES (OLD.""Id"", OLD.""UserId"", 'DELETE', now(), row_to_json(OLD)::text, NULL);
        RETURN OLD;
    END IF;

    RETURN NULL;
END;
$$;
");


            // 4) Trigger na Accounts: trg_account_audit
            migrationBuilder.Sql(@"
DROP TRIGGER IF EXISTS trg_account_audit ON accounts;

CREATE TRIGGER trg_account_audit
AFTER INSERT OR UPDATE OR DELETE ON accounts
FOR EACH ROW
EXECUTE FUNCTION fn_account_audit();
");



            // 5) VIEW: vw_admin_accounts
            migrationBuilder.Sql(@"
CREATE OR REPLACE VIEW vw_admin_accounts AS
SELECT
    a.""Id""           AS ""AccountId"",
    a.""UserId""       AS ""UserId"",
    u.""Email""        AS ""UserEmail"",
    r.""Name""         AS ""UserRole"",
    a.""FirstName""    AS ""FirstName"",
    a.""LastName""     AS ""LastName"",
    a.""DateOfBirth""  AS ""DateOfBirth"",
    a.""Address""      AS ""Address""
FROM accounts a
JOIN users u ON u.""Id"" = a.""UserId""
JOIN roles r ON r.""Id"" = u.""RoleId"";
");



            // 6) Stored procedure: sp_create_account (koristi predikat)
            migrationBuilder.Sql(@"
CREATE OR REPLACE PROCEDURE sp_create_account(
    IN p_user_id integer,
    IN p_first_name text,
    IN p_last_name text,
    IN p_date_of_birth date,
    IN p_address text,
    OUT new_account_id integer
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF user_has_account(p_user_id) THEN
        RAISE EXCEPTION 'Account already exists for userId=%', p_user_id
            USING ERRCODE = 'P0001';
    END IF;

    INSERT INTO accounts (""UserId"", ""FirstName"", ""LastName"", ""DateOfBirth"", ""Address"")
    VALUES (p_user_id, p_first_name, p_last_name, p_date_of_birth, p_address)
    RETURNING ""Id"" INTO new_account_id;
END;
$$;
");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(name: "account_audit_logs");

            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_admin_accounts;");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_account_audit ON accounts;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS fn_account_audit();");
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS sp_create_account(integer, text, text, date, text);");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS user_has_account(integer);");
        }
    }
}
