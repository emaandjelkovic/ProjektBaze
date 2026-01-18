using AccountManager.Data;
using AccountManager.Helpers;
using AccountManager.Models;
using AccountManager.Services;
using AccountManager.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AccountManager.Controllers;

public class AuthController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ISessionService _sessionService;
    public AuthController(ApplicationDbContext db, ISessionService sessionService)
    {
        _db = db;
        _sessionService = sessionService;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var email = vm.Email.Trim().ToLower();

        var emailTaken = await _db.Users.AnyAsync(u => u.Email == email);
        if (emailTaken)
        {
            ModelState.AddModelError(nameof(vm.Email), "Email je već registriran.");
            return View(vm);
        }

        var userRoleId = await _db.Roles
            .Where(r => r.Name == "User")
            .Select(r => r.Id)
            .SingleAsync();

        var user = new User
        {
            Email = email,
            Password = PasswordHelper.Hash(vm.Password),
            RoleId = userRoleId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // nakon registracije odmah login
        await SignInUserAsync(user.Id);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var email = vm.Email.Trim().ToLower();
        var passHash = PasswordHelper.Hash(vm.Password);

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || user.Password != passHash)
        {
            ModelState.AddModelError(string.Empty, "Neispravan email ili lozinka.");
            return View(vm);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();

        var sessionId = await _sessionService.CreateSessionAsync(user.Id, ip, ua);

        // claimovi
        var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Email),
    new Claim("roleId", user.RoleId.ToString()),
    new Claim("sessionId", sessionId.ToString())
};

        await SignInUserAsync(user.Id);

        
        if (user.Role.Name == "Admin")
            return RedirectToAction("Users", "Admin");

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _sessionService.RevokeAllAsync(userId);

        await HttpContext.SignOutAsync();
        return RedirectToAction("Login", "Auth");
    }

    [HttpGet]
    public IActionResult Denied() => View();

    private async Task SignInUserAsync(int userId)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .SingleAsync(u => u.Id == userId);

        var hasAccount = await _db.Accounts.AnyAsync(a => a.UserId == user.Id);

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Email),
        new Claim(ClaimTypes.Role, user.Role.Name),
        new Claim("HasAccount", hasAccount ? "1" : "0")
    };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true }
        );
    }

}

