using System.Globalization;
using EduCore.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace EduCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Use Egyptian Pound (EGP) for all currency formatting across the app.
            // Based on en-US number formatting (Latin digits, "1,234.00") with the EGP symbol.
            var egp = (CultureInfo)CultureInfo.GetCultureInfo("en-US").Clone();
            egp.NumberFormat.CurrencySymbol = "EGP ";
            CultureInfo.DefaultThreadCurrentCulture = egp;
            CultureInfo.DefaultThreadCurrentUICulture = egp;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register EF Core DbContext (SQL Server)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // Cookie-based authentication (custom, against the Teachers/Students tables)
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                });

            var app = builder.Build();

            // Apply any pending EF Core migrations at startup.
            // Useful for hosts (e.g. MonsterASP) whose database is only reachable from the
            // deployed app, not from a local machine running Update-Database.
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.Migrate();
                }
                catch (Exception ex)
                {
                    // Don't crash the app if migration fails (e.g. DB not reachable yet);
                    // log and continue so the error is visible in the host logs.
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Database migration on startup failed.");
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
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
