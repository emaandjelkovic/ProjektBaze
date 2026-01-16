using AccountManager.Data;
using AccountManager.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
}

