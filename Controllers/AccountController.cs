using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using ItemProcessor.Models;
using ItemProcessor.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ItemProcessor.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        // Constructor injection — ASP.NET Core automatically provides the DbContext
        // Why injection? Keeps controller testable and loosely coupled
        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            // If already logged in, go straight to items
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Item");

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    var passwordHash = HashPassword(model.Password);

    var user = await _db.Users
        .FirstOrDefaultAsync(u =>
            u.Email == model.Email &&
            u.PasswordHash == passwordHash &&
            u.IsActive == true);

    if (user == null)
    {
        // Key is empty string "" — renders in validation summary
        ModelState.AddModelError(string.Empty, "Invalid email or password");
        return View(model);
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Name,           user.Username),
        new Claim(ClaimTypes.Email,          user.Email)
    };

    var identity  = new ClaimsIdentity(claims,
        CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc   = DateTime.UtcNow.AddHours(8)
        });

return Redirect("/item/index");
}

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: /Account/Register
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string Username, string Email, string Password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Username) ||
                    string.IsNullOrWhiteSpace(Email) ||
                    string.IsNullOrWhiteSpace(Password))
                {
                    ModelState.AddModelError("", "All fields are required");
                    return View();
                }

                if (Password.Length < 6)
                {
                    ModelState.AddModelError("", "Password must be at least 6 characters");
                    return View();
                }

                var exists = await _db.Users.AnyAsync(u => u.Email == Email);
                if (exists)
                {
                    ModelState.AddModelError("", "Email already registered");
                    return View();
                }

                var user = new User
                {
                    Username = Username,
                    Email = Email,
                    PasswordHash = HashPassword(Password),
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Account created! Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Registration failed: {ex.Message}");
                return View();
            }
        }

        // Simple SHA256 hash — in production use BCrypt
        // Why hash? Never store plain text passwords
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}