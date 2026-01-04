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
    public class DetailsModel : PageModel
    {
        private readonly WebApplication1.DataContext.ProduitDBContext _context;

        public DetailsModel(WebApplication1.DataContext.ProduitDBContext context)
        {
            _context = context;
        }

        public Produit Produit { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produit = await _context.Produit
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.ProduitId == id);



            if (produit is not null)
            {
                Produit = produit;

                return Page();
            }

            return NotFound();
        }
        
    }
}
