using AllUpBack.Areas.Admin.Data;
using AllUpBack.Areas.Admin.Services;
using AllUpBack.DAL;
using AllUpBack.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AllUpBack
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddLanguageService();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddScoped<CategoryService>();
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

            var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();

            app.UseRequestLocalization(locOptions.Value);

            app.UseRouting();

            app.UseAuthorization();

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


            app.Run();
        }
    }
}