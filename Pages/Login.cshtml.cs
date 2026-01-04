using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace DOTNETPanier.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                ModelState.AddModelError("", "Username is required");
                return Page();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, Username)
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // 🔹 Sign in user and save in cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,      // ✅ Important: keeps user logged in
                    ExpiresUtc = DateTime.UtcNow.AddDays(30)
                });

            return RedirectToPage("/Index");
        }

    }
}
