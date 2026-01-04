using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly WebApplication1.DataContext.ProduitDBContext _context;

        public IndexModel(WebApplication1.DataContext.ProduitDBContext context)
        {
            _context = context;
        }

        public IList<Produit> Produit { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // MODIFICATION: On prend seulement les 4 derniers produits ajoutés pour la section "Nouveautés"
            // Cela évite de charger toute la base de données sur la page d'accueil
            Produit = await _context.Produit
                .OrderByDescending(p => p.ProduitId) // Ou p.DateCreation si vous avez ce champ
                .Take(4)
                .ToListAsync();
        }
    }
}