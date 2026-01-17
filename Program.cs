using AccountManager.Data;
using AccountManager.Helpers;
using AccountManager.Models;
using AccountManager.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Denied";
    });

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ISessionService, SessionService>();

builder.Services.AddAuthorization();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // provjeri postoji li admin role
    var adminRole = db.Roles.FirstOrDefault(r => r.Id == 2);
    if (adminRole == null)
        throw new Exception("Admin role not found. Seed roles must exist.");

    // provjeri postoji li admin user
    var adminEmail = "admin@admin.com";

    var adminExists = db.Users.Any(u => u.Email == adminEmail);

    if (!adminExists)
    {
        var adminUser = new User
        {
            Email = adminEmail,
            Password = PasswordHelper.Hash("Admin123!"),
            RoleId = adminRole.Id,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(adminUser);
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
