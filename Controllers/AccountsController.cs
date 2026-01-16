using AccountManager.Data;
using AccountManager.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Security.Claims;

namespace AccountManager.Controllers
{
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext context;

        public AccountsController(ApplicationDbContext context)
        {
            this.context = context;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        //[HttpGet]
        //public async Task<IActionResult> My()
        //{
        //    var userId = GetCurrentUserId();

        //    var account = await context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
        //    if (account == null)
        //        return RedirectToAction(nameof(Create));

        //    return View(account);
        //}

        [HttpGet]
        public async Task<IActionResult> My()
        {
            var userId = GetCurrentUserId();

            var row = await context.VwAdminAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (row == null)
                return RedirectToAction(nameof(Create));

            
            var model = new AccountManager.Models.Account
            {
                Id = row.AccountId,
                UserId = row.UserId,
                FirstName = row.FirstName,
                LastName = row.LastName,
                DateOfBirth = row.DateOfBirth,
                Address = row.Address,
                User = null! 
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new Account
            {
                DateOfBirth = DateTime.Today,
                UserId = GetCurrentUserId()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Account model)
        {
            // navigaciju ne validiramo (da ne traži User)
            ModelState.Remove(nameof(Account.User));

            var userId = GetCurrentUserId();

            if (!ModelState.IsValid)
                return View(model);

            model.UserId = userId;

            model.DateOfBirth = model.DateOfBirth.Date;

            // provjera: jedan account po useru
            var alreadyHas = await context.Accounts.AsNoTracking().AnyAsync(a => a.UserId == userId);
            if (alreadyHas)
            {
                ModelState.AddModelError(string.Empty, "Već imaš kreiran račun.");
                return View(model);
            }

            try
            {
                context.Accounts.Add(model);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
            {
                ModelState.AddModelError(string.Empty, "Već imaš kreiran račun.");
                return View(model);
            }

            await RefreshClaimsAsync(userId);
            return RedirectToAction(nameof(My));
        }


        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = GetCurrentUserId();
            var account = await context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
                return RedirectToAction(nameof(Create));

            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Account model)
        {
            var userId = GetCurrentUserId();
            var account = await context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
                return RedirectToAction(nameof(Create));

            if (!ModelState.IsValid)
                return View(model);

            // update samo polja koja user smije mijenjati
            account.FirstName = model.FirstName;
            account.LastName = model.LastName;
            account.DateOfBirth = model.DateOfBirth;
            account.Address = model.Address;

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(My));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            var userId = GetCurrentUserId();
            var account = await context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);

            if (account != null)
            {
                context.Accounts.Remove(account);
                await context.SaveChangesAsync();
            }

            await RefreshClaimsAsync(userId);

            return RedirectToAction(nameof(Create));
        }

        private int GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idStr!);
        }

        // osvježi HasAccount claim da navbar odmah pokaže "Moj račun" ili "Kreiraj račun"
        private async Task RefreshClaimsAsync(int userId)
        {
            var user = await context.Users.Include(u => u.Role).SingleAsync(u => u.Id == userId);
            //var hasAccount = await context.Accounts.AnyAsync(a => a.UserId == userId);
            var hasAccount = await UserHasAccountAsync(userId);


            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role.Name),
            new Claim("HasAccount", hasAccount ? "1" : "0")
        };

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true }
            );
        }


        private async Task<bool> UserHasAccountAsync(int userId)
        {
            return await context.Database
                .SqlQueryRaw<bool>("SELECT user_has_account({0});", userId)
                .SingleAsync();
        }


    }
}
