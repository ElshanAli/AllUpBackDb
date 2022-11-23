using AllUpBack.Areas.Admin.Services;
using AllUpBack.DAL;
using AllUpBack.DAL.Entities;
using AllUpBack.Data;
using AllUpBack.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AllUpBack
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddLanguageService();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                   builder =>
                   {
                       builder.MigrationsAssembly(nameof(AllUpBack));
                   });

            });

            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredLength = 5;

                options.User.RequireUniqueEmail = true;

            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            builder.Services.AddScoped<CategoryService>();
            builder.Services.Configure<AdminUser>(builder.Configuration.GetSection("AdminUser"));

            Constants.RootPath = builder.Environment.WebRootPath;
            Constants.FlagPath = Path.Combine(Constants.RootPath, "assets", "images", "flag");
            Constants.CategoryPath = Path.Combine(Constants.RootPath, "assets", "images", "category");
            Constants.ProductPath = Path.Combine(Constants.RootPath, "assets", "images", "product");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var dataInitalizer = new DataInitalizer(serviceProvider);

                await dataInitalizer.SeedData();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();

            app.UseRequestLocalization(locOptions.Value);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                  name: "areas",
                  pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
                );

                endpoints.MapControllerRoute(
                 name: "default",
                 pattern: "{controller=Home}/{action=Index}/{id?}"
               );
            });


            await app.RunAsync();
        }
    }
}