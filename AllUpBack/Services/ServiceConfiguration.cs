using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace AllUpBack.Services
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddLanguageService(this IServiceCollection services)
        {
            services.AddSingleton<LanguageService>();
            services.AddLocalization(option => option.ResourcesPath = "Resources");
            services.AddMvc().AddViewLocalization();

            services.Configure<RequestLocalizationOptions>(options =>
               {
                   var supportedCultures = new List<CultureInfo>
                   {
                       new CultureInfo("en-US"),
                       new CultureInfo("az"),

                   };

                   options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                   options.SupportedCultures = supportedCultures;
                   options.SupportedUICultures = supportedCultures;
                   
               });

            return services;
        }
    }
}
