using System.Globalization;
using EduCore.Data;
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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

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
