using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataContext;
using WebApplication1.Models;

namespace WebApplication1.Pages.ProduitsCrud
{
    public class IndexModel : PageModel
    {
        private readonly WebApplication1.DataContext.ProduitDBContext _context;

        public IndexModel(WebApplication1.DataContext.ProduitDBContext context)
        {
            _context = context;
        }

        public IList<Produit> Produit { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Produit = await _context.Produit
                .Include(p => p.Category).ToListAsync();
        }
    }
}
