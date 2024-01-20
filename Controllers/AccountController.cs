using Microsoft.AspNetCore.Mvc;
using Projekt_studia2.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Projekt_studia2.Services;

using Microsoft.AspNetCore.Authorization;



namespace Projekt_studia2.Controllers
{

    public class AccountController : Controller
    {
        private readonly LibraryContext _context;
        private readonly LoggingService _loggingService;
        public AccountController(LibraryContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {

                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _loggingService.LogEvent(User.Identity.Name, "Akcja Logout zakończona pomyślnie - użytkownik wylogowany");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Register()
        {
            return View();
        }
        [Authorize(Roles = "admin")]
        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddUser(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    await _loggingService.LogEvent(User.Identity.Name, "Akcja AddUser zakończona niepowodzeniem - użytkownik o tej nazwie już istnieje ");
                    ModelState.AddModelError("Username", "Użytkownik o tej nazwie już istnieje.");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    Role = model.Role
                };
                await _loggingService.LogEvent(User.Identity.Name, "Akcja AddUser zakończona pomyślnie - użytkownik został pomyślnie dodany");
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Użytkownik został pomyślnie dodany.";
                return RedirectToAction("AddUser");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users
                                                 .AsNoTracking()
                                                 .AnyAsync(u => u.Username == model.Username);

                if (existingUser)
                {
                    await _loggingService.LogEvent(model.Username, "Akcja Register zakończona niepowodzeniem - użytkownik o tej nazwie już istnieje.");
                    ModelState.AddModelError("Username", "Użytkownik o tej nazwie już istnieje.");
                    return View(model);
                }
                var Password = model.Password;

                if (!string.IsNullOrWhiteSpace(Password))
                {
                    var user = new User
                    {
                        Username = model.Username,
                        Password = Password,
                        Role = "czytelnik"
                    };
                    _context.Users.Add(user);

                    try
                    {
                        await _loggingService.LogEvent(user.Username, "Akcja Register zakończona pomyślnie");
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Login");
                    }
                    catch (Exception ex)
                    {
                        await _loggingService.LogEvent(user.Username, "Akcja Register zakończona niepowodzeniem - wystąpił błąd podczas zapisu do bazy danych");
                        ModelState.AddModelError("", "Wystąpił błąd podczas zapisu do bazy danych: " + ex.Message);
                    }
                }
            }
            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                                         .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

                if (user != null)
                {              
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                     new Claim(ClaimTypes.Role, user.Role),                  
                };
                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties{};

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    await _loggingService.LogEvent(user.Username, "Akcja Login zakończona powodzeniem");
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    await _loggingService.LogEvent(model.Username, "Akcja Login zakończona niepowodzeniem");
                    ModelState.AddModelError("", "Username or password is incorrect");

                }
            }
            return View(model);
        }
        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult DeleteUser()
        {
            var users = _context.Users
                .Where(u => _context.Users
                    .Any(ur => ur.ID == u.ID && (ur.Role == "admin" || ur.Role == "bibliotekarz")))
                .ToList();

            return View(users);
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteUserConfirmed(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                await _loggingService.LogEvent(User.Identity.Name, "Akcja DeleteUserConfirmed zakończona niepowodzeniem - nie znaleziono użytkownika");
                return NotFound();
            }
            await _loggingService.LogEvent(User.Identity.Name, "Akcja DeleteUserConfirmed zakończona pomyślnie");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Użytkownik został pomyślnie usunięty.";

            return RedirectToAction(nameof(DeleteUser));
        }
    }
}