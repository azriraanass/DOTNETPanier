using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WebApplication1.DataContext;
using WebApplication1.Models;
using WebApplication1.Services.Cookies.CarteItem;


namespace WebApplication1.Pages.Panier
{
    public class IndexModel : PageModel
    {
        public List<CartItem> CartItems { get; set; } = new();
        public decimal TotalGeneral { get; set; }

        private readonly CarteItemCookiesManager _cookieCartService;
        private readonly ProduitDBContext _dbContext;
        private readonly IDistributedCache _cache;

        public IndexModel(
            CarteItemCookiesManager cookieCartService,
            ProduitDBContext dbContext,
            IDistributedCache cache)
        {
            _cookieCartService = cookieCartService;
            _dbContext = dbContext;
            _cache = cache;
        }

        // =========================
        // GET PANIER (CACHED)
        // =========================
        public async Task OnGetAsync()
        {
            CartItems = _cookieCartService.LoadCartFromCookie()
                        ?? new List<CartItem>();

            if (!CartItems.Any())
            {
                TotalGeneral = 0;
                return;
            }

            var productIds = CartItems
                .Select(c => c.ProduitId)
                .Distinct()
                .ToList();

            var products = await GetProductsCachedAsync(productIds);

            foreach (var item in CartItems)
            {
                if (products.TryGetValue(item.ProduitId, out var produit))
                {
                    item.Produit = produit;
                    item.TotalPrice = produit.Price * item.Quantity;
                }
            }

            TotalGeneral = CartItems.Sum(ci => ci.TotalPrice);
        }

        // =========================
        // PRODUCT CACHE (BY ID)
        // =========================
        private async Task<Dictionary<Guid, Produit>> GetProductsCachedAsync(List<Guid> productIds)
        {
            var result = new Dictionary<Guid, Produit>();

            foreach (var id in productIds)
            {
                var cacheKey = $"produit:{id}";
                var cached = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cached))
                {
                    result[id] = JsonSerializer.Deserialize<Produit>(cached)!;
                    continue;
                }

                var produit = await _dbContext.Produit
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProduitId == id);

                if (produit != null)
                {
                    await _cache.SetStringAsync(
                        cacheKey,
                        JsonSerializer.Serialize(produit),
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                        });

                    result[id] = produit;
                }
            }

            return result;
        }

        // =========================
        // VIDER PANIER
        // =========================
        public RedirectToPageResult OnPostViderPanier()
        {
            _cookieCartService.ClearCartCookie();
            return RedirectToPage();
        }

        // =========================
        // REMOVE ITEM
        // =========================
        public IActionResult OnPostRemoveItem(Guid productId)
        {
            var cartItems = _cookieCartService.LoadCartFromCookie();
            if (cartItems == null || !cartItems.Any())
                return RedirectToPage("/Panier");

            var itemToRemove = cartItems
                .FirstOrDefault(item => item.ProduitId == productId);

            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                _cookieCartService.SaveCartToCookie(cartItems);
                TempData["Success"] = "Produit retiré du panier !";
            }

            return RedirectToPage();
        }

        // =========================
        // UPDATE QUANTITY
        // =========================
        public IActionResult OnPostUpdateQuantity(Guid productId, string action)
        {
            try
            {
                var cartItems = _cookieCartService.LoadCartFromCookie();
                if (cartItems == null || !cartItems.Any())
                    return RedirectToPage();

                var cartItem = cartItems
                    .FirstOrDefault(item => item.ProduitId == productId);

                if (cartItem != null)
                {
                    switch (action)
                    {
                        case "increase":
                            cartItem.Quantity++;
                            break;

                        case "decrease":
                            if (cartItem.Quantity > 1)
                                cartItem.Quantity--;
                            break;
                    }

                    _cookieCartService.SaveCartToCookie(cartItems);
                    TempData["Success"] = "Quantité mise à jour avec succès";
                }
            }
            catch
            {
                TempData["Error"] = "Erreur lors de la mise à jour de la quantité";
            }

            return RedirectToPage();
        }
    }
}
