using Microsoft.Build.Framework;
using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Services.Cookies.CarteItem
{
    public class CarteItemCookiesManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _cookieName = "ShoppingCart";

        public CarteItemCookiesManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<CartItem> LoadCartFromCookie()
        {
            try
            {
                var cookieValue = _httpContextAccessor.HttpContext.Request.Cookies[_cookieName];

                if (string.IsNullOrEmpty(cookieValue))
                {
                    return new List<CartItem>();
                }

                // Return a List of CartItem After Deserialize le json string dans cookies
                return JsonSerializer.Deserialize<List<CartItem>>(cookieValue) ?? new List<CartItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Erreur chargement cookie: {ex.Message}");
                return new List<CartItem>();
            }
        }

        public void ClearCartCookie()
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(_cookieName);
            Console.WriteLine(" Cookie panier supprimé");
        }

        public void SaveCartToCookie(List<CartItem> cartItems)
        { 
            try
            {
                var json = JsonSerializer.Serialize(cartItems);
                var options = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true,
                    Secure = false 
                    // Mettez à true en production avec HTTPS 
                };

                _httpContextAccessor.HttpContext.Response.Cookies.Append(_cookieName, json, options);
                _httpContextAccessor.HttpContext.Response.Cookies.Append("Test", "Name");
                Console.WriteLine($" Panier sauvegardé dans le cookie: {cartItems.Count} items");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Erreur sauvegarde cookie: {ex.Message}");
            }
            

        }

    }
}
