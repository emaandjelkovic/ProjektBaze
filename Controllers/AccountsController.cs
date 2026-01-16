using AccountManager.Data;
using AccountManager.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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

            // mapiramo na isti model koji tvoj View očekuje (Account)
            var model = new AccountManager.Models.Account
            {
                Id = row.AccountId,
                UserId = row.UserId,
                FirstName = row.FirstName,
                LastName = row.LastName,
                DateOfBirth = row.DateOfBirth,
                Address = row.Address,
                User = null! // ne koristi se u viewu (i imamo ValidateNever)
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new Account
            {
                DateOfBirth = DateTime.Now,
                UserId = GetCurrentUserId()
            };

            return View(model);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Account model)
        //{
        //    var userId = GetCurrentUserId();

        //    // sigurnost: userId dolazi iz cookie-a, ne iz forme
        //    model.UserId = userId;

        //    // ako već postoji account, ne dopuštamo novi
        //    var exists = await context.Accounts.AnyAsync(a => a.UserId == userId);
        //    if (exists)
        //        return RedirectToAction(nameof(My));

        //    if (!ModelState.IsValid)
        //        return View(model);

        //    context.Accounts.Add(model);
        //    await context.SaveChangesAsync();

        //    await RefreshClaimsAsync(userId);

        //    return RedirectToAction(nameof(My));
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Account model)
        {
            // navigaciju ne validiramo
            ModelState.Remove(nameof(Account.User));

            var userId = GetCurrentUserId();

            if (!ModelState.IsValid)
                return View(model);

            // userId uvijek uzimamo iz cookie-a
            var pUserId = userId;
            var pFirst = model.FirstName;
            var pLast = model.LastName;
            var pDob = model.DateOfBirth.Date; // date-only
            var pAddr = model.Address;

            // OUT param
            var outId = new NpgsqlParameter("new_account_id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            try
            {
                await context.Database.ExecuteSqlRawAsync(
                    @"CALL sp_create_account({0}, {1}, {2}, {3}, {4}, {5});",
                    pUserId, pFirst, pLast, pDob, pAddr, outId
                );
            }
            catch (PostgresException ex) when (ex.SqlState == "P0001")
            {
                // naša custom greška: account već postoji
                ModelState.AddModelError(string.Empty, "Račun već postoji za ovog korisnika.");
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
