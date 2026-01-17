using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AccountManager.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.Id);
                });

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
                name: "user_audit_logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    OldData = table.Column<string>(type: "text", nullable: true),
                    NewData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_role_permissions_permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_sessions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "USERS_VIEW", "Pregled korisnika" },
                    { 2, "USERS_ROLE_CHANGE", "Promjena role korisnika" },
                    { 3, "ACCOUNTS_VIEW", "Pregled računa" },
                    { 4, "AUDIT_VIEW", "Pregled audita" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "User" },
                    { 2, "Admin" }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 2 },
                    { 2, 2 },
                    { 3, 2 },
                    { 4, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_UserId",
                table: "accounts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_permissions_Code",
                table: "permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_PermissionId",
                table: "role_permissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Name",
                table: "roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_audit_logs_UserId",
                table: "user_audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_UserId",
                table: "user_sessions",
                column: "UserId");

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



            // 6) Stored procedure: sp_admin_set_role (koristi predikat)
            migrationBuilder.Sql(@"
CREATE OR REPLACE PROCEDURE public.sp_admin_set_role(
    IN p_user_id integer,
    IN p_role_id integer
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- provjera postoji li user
    IF NOT EXISTS (SELECT 1 FROM users WHERE ""Id"" = p_user_id) THEN
        RAISE EXCEPTION 'User not found id=%', p_user_id
            USING ERRCODE = 'P0002';
    END IF;

    -- provjera postoji li rola
    IF NOT EXISTS (SELECT 1 FROM roles WHERE ""Id"" = p_role_id) THEN
        RAISE EXCEPTION 'Role not found id=%', p_role_id
            USING ERRCODE = 'P0003';
    END IF;

    -- promjena role
    UPDATE users
    SET ""RoleId"" = p_role_id
    WHERE ""Id"" = p_user_id;
END;
$$;
");



            // 8) Audit funkcija + trigger na users -> user_audit_logs
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION fn_user_audit()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF (TG_OP = 'INSERT') THEN
        INSERT INTO user_audit_logs(""UserId"", ""Action"", ""ChangedAt"", ""OldData"", ""NewData"")
        VALUES (NEW.""Id"", 'INSERT', now(), NULL, row_to_json(NEW)::text);
        RETURN NEW;

    ELSIF (TG_OP = 'UPDATE') THEN
        INSERT INTO user_audit_logs(""UserId"", ""Action"", ""ChangedAt"", ""OldData"", ""NewData"")
        VALUES (NEW.""Id"", 'UPDATE', now(), row_to_json(OLD)::text, row_to_json(NEW)::text);
        RETURN NEW;

    ELSIF (TG_OP = 'DELETE') THEN
        INSERT INTO user_audit_logs(""UserId"", ""Action"", ""ChangedAt"", ""OldData"", ""NewData"")
        VALUES (OLD.""Id"", 'DELETE', now(), row_to_json(OLD)::text, NULL);
        RETURN OLD;
    END IF;

    RETURN NULL;
END;
$$;

DROP TRIGGER IF EXISTS trg_user_audit ON users;

CREATE TRIGGER trg_user_audit
AFTER INSERT OR UPDATE OR DELETE ON users
FOR EACH ROW
EXECUTE FUNCTION fn_user_audit();
");

            // 9) Predikat/funkcija: user_has_permission(userId, permCode) preko role_permissions
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION user_has_permission(p_user_id integer, p_perm_code text)
RETURNS boolean
LANGUAGE sql
AS $$
    SELECT EXISTS (
        SELECT 1
        FROM users u
        JOIN role_permissions rp ON rp.""RoleId"" = u.""RoleId""
        JOIN permissions p ON p.""Id"" = rp.""PermissionId""
        WHERE u.""Id"" = p_user_id
          AND p.""Code"" = p_perm_code
    );
$$;
");

            // 10) Stored procedure: revoke sessions (logout/all devices)
            migrationBuilder.Sql(@"
CREATE OR REPLACE PROCEDURE sp_revoke_user_sessions(IN p_user_id integer)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE user_sessions
    SET ""RevokedAt"" = now()
    WHERE ""UserId"" = p_user_id
      AND ""RevokedAt"" IS NULL;
END;
$$;
");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // prvo SQL objekti koji ovise o tablicama
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_admin_accounts;");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_account_audit ON accounts;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS fn_account_audit();");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_user_audit ON users;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS fn_user_audit();");

            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS public.sp_admin_set_role(integer, integer);");
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS public.sp_revoke_user_sessions(integer);");

            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS user_has_account(integer);");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS user_has_permission(integer, text);");

            // onda tablice koje imaju FK-ove na druge
            migrationBuilder.DropTable(name: "account_audit_logs");
            migrationBuilder.DropTable(name: "user_audit_logs");
            migrationBuilder.DropTable(name: "user_sessions");
            migrationBuilder.DropTable(name: "accounts");
            migrationBuilder.DropTable(name: "role_permissions");
            migrationBuilder.DropTable(name: "permissions");
            migrationBuilder.DropTable(name: "users");
            migrationBuilder.DropTable(name: "roles");
        }

    }
}
