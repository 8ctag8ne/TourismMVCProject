using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tourism.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Tourism.Models;

namespace Tourism.Controllers
{
    public class AccountController : Controller
    {

            private readonly UserManager<User> _userManager;
            private readonly SignInManager<User> _signInManager;

            public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
            {
                _userManager = userManager;
                _signInManager = signInManager;
            }
            [HttpGet]
            public IActionResult Register()
            {
                return View();
            }
            [HttpPost]
            public async Task<IActionResult> Register(RegisterViewModel model)
            {
                if (ModelState.IsValid && model.Password is not null)
                {
                    User user = new User {  /*PhoneNumber = model.PhoneNumber,*/
                                            Email = model.Email, 
                                            UserName = model.Login};
                    // додаємо користувача
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        // установка кукі
                        await _signInManager.SignInAsync(user, false);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                return View(model);
            }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if(user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        // перевіряємо, чи належить URL додатку
                        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return Redirect(model.ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Неправильний логін чи (та) пароль");
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // видаляємо автентифікаційні куки
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }    
}
