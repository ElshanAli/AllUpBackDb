using AllUpBack.DAL.Entities;
using AllUpBack.Data;
using AllUpBack.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace AllUpBack.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View();


            var existUsers = await _userManager.FindByNameAsync(model.UserName);
            if (existUsers is not null)
            {
                ModelState.AddModelError("", "Username cannot be duplicated");
                return View();
            }
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.UserName,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);

                }
                return View();
            }

            var createdUser = await _userManager.FindByNameAsync(model.UserName);

           result = await _userManager.AddToRoleAsync(createdUser, Constants.UserRole);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);

                }
                return View();
            }
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View();
            var existUser = await _userManager.FindByNameAsync(model.UserName);
            if (existUser == null)
            {
                ModelState.AddModelError("", "Username isn't correct");
                return View();
            }

            var signResult = await _signInManager.PasswordSignInAsync(existUser, model.Password, model.RememberMe, false);

            if (!signResult.Succeeded)
            {
                ModelState.AddModelError("", "Invalid credential");
                return View();
            }
            if (!string.IsNullOrEmpty(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
