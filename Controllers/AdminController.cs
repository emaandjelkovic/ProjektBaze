using AccountManager.Data;
using AccountManager.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace AccountManager.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /Admin/Users
    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var users = await _db.Users
            .Include(u => u.Role)
            .OrderBy(u => u.Id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                RoleName = u.Role.Name,
                RoleId = u.RoleId,
                CreatedAt = u.CreatedAt,
                HasAccount = u.Account != null
            })
            .ToListAsync();

        ViewBag.Roles = await _db.Roles
    .AsNoTracking()
    .OrderBy(r => r.Id)
    .ToListAsync();

        return View(users);
    }

    // GET: /Admin/Accounts
    [HttpGet]
    public async Task<IActionResult> Accounts()
    {
        var accounts = await _db.Accounts
            .Include(a => a.User)
            .ThenInclude(u => u.Role)
            .OrderBy(a => a.Id)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserEmail = a.User.Email,
                UserRole = a.User.Role.Name,
                FirstName = a.FirstName,
                LastName = a.LastName,
                DateOfBirth = a.DateOfBirth,
                Address = a.Address
            })
            .ToListAsync();

        return View(accounts);
    }

    // GET: /Admin/AccountDetails/5
    [HttpGet]
    public async Task<IActionResult> AccountDetails(int id)
    {
        var account = await _db.Accounts
            .Include(a => a.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (account == null)
            return NotFound();

        var vm = new AdminAccountDetailsVm
        {
            Id = account.Id,
            UserId = account.UserId,
            UserEmail = account.User.Email,
            UserRole = account.User.Role.Name,
            FirstName = account.FirstName,
            LastName = account.LastName,
            DateOfBirth = account.DateOfBirth,
            Address = account.Address
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRole(int userId)
    {
        // dohvat trenutne role iz baze (ne iz DTO)
        var currentRoleId = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.RoleId)
            .SingleOrDefaultAsync();

        if (currentRoleId == 0)
        {
            TempData["Error"] = "Korisnik ne postoji.";
            return RedirectToAction(nameof(Users)); // <= prilagodi ako ti je action drugačiji
        }

        // Pretpostavka: 1=User, 2=Admin
        var newRoleId = (currentRoleId == 2) ? 1 : 2;

        try
        {
            await _db.Database.ExecuteSqlRawAsync(
                @"CALL public.sp_admin_set_role({0}, {1});",
                userId, newRoleId
            );
            TempData["Success"] = "Rola promijenjena.";
        }
        catch (PostgresException ex) when (ex.SqlState == "P0002")
        {
            TempData["Error"] = "Korisnik ne postoji.";
        }
        catch (PostgresException ex) when (ex.SqlState == "P0003")
        {
            TempData["Error"] = "Rola ne postoji.";
        }

        return RedirectToAction(nameof(Users)); // <= prilagodi na tvoju users action
    }

    public async Task<IActionResult> Audit()
    {
        // 1) USER audit logovi (ovo imaš kao DbSet)
        var userLogs = await _db.UserAuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.ChangedAt)
            .Take(200)
            .Select(x => new
            {
                Entity = AuditTypeEnum.User,
                EntityId = (int?)x.UserId,
                UserId = (int?)x.UserId,
                x.Action,
                x.ChangedAt,
                x.OldData,
                x.NewData
            })
            .ToListAsync();

        // 2) ACCOUNT audit logovi (raw SQL)
        var accountRows = new List<AccountAuditDto>();

        await using (var conn = (NpgsqlConnection)_db.Database.GetDbConnection())
        {
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@"
            SELECT ""Id"", ""AccountId"", ""UserId"", ""Action"", ""ChangedAt"", ""OldData"", ""NewData""
            FROM account_audit_logs
            ORDER BY ""ChangedAt"" DESC
            LIMIT 200;", conn);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                accountRows.Add(new AccountAuditDto(
                    Id: reader.GetInt64(0),
                    AccountId: reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    UserId: reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    Action: reader.GetString(3),
                    ChangedAt: reader.GetDateTime(4),  // timestamptz -> DateTime (UTC)
                    OldData: reader.IsDBNull(5) ? null : reader.GetString(5),
                    NewData: reader.IsDBNull(6) ? null : reader.GetString(6)
                ));
            }
        }

        var accountLogs = accountRows.Select(x => new
        {
            Entity = AuditTypeEnum.Account,
            EntityId = x.AccountId,
            UserId = x.UserId,
            Action = x.Action,
            ChangedAt = x.ChangedAt,
            OldData = x.OldData,
            NewData = x.NewData
        }).ToList();

        // 3) Merge + sort
        var merged = userLogs
            .Concat(accountLogs)
            .OrderByDescending(x => x.ChangedAt)
            .Take(300)
            .ToList();

        return View(merged);
    }
    private sealed record AccountAuditDto(
    long Id,
    int? AccountId,
    int? UserId,
    string Action,
    DateTime ChangedAt,
    string? OldData,
    string? NewData
);
}

