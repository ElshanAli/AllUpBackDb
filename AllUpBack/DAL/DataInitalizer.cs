using AllUpBack.DAL.Entities;
using AllUpBack.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AllUpBack.DAL
{
    public class DataInitalizer
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _dbContext;
        private readonly AdminUser _adminUser;

        public DataInitalizer(IServiceProvider serviceProvider)
        {
            _adminUser = serviceProvider.GetService<IOptions<AdminUser>>().Value;
            _userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        }

        public async Task SeedData()
        {
            await _dbContext.Database.MigrateAsync();

            var roles = new List<string> { Constants.AdminRole, Constants.UserRole };

            foreach (var role in roles)
            {
                if (await _roleManager.RoleExistsAsync(role))
                    continue;

                var result = await _roleManager.CreateAsync(new IdentityRole { Name = role });

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        //loging
                        Console.WriteLine(error.Description);
                    }
                }
            }
            var existUser = await _userManager.FindByNameAsync(_adminUser.UserName);

            if (existUser is not null)
                return;

            var resultUser = await _userManager.CreateAsync(new User
            {
                UserName = _adminUser.UserName,
                Email = _adminUser.Email,
            }, _adminUser.Password);

            if (!resultUser.Succeeded)
            {
                foreach (var error in resultUser.Errors)
                {
                    //loging
                    Console.WriteLine(error.Description);
                }
            }
            else
            {
                var userExist = await _userManager.FindByNameAsync(_adminUser.UserName);

                await _userManager.AddToRoleAsync(userExist, Constants.AdminRole);
            }
        }
    }
}
