using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataContext;
using WebApplication1.Models;

namespace WebApplication1.Pages.Produits
{
    public class DeleteModel : PageModel
    {
        private readonly WebApplication1.DataContext.ProduitDBContext _context;

        public DeleteModel(WebApplication1.DataContext.ProduitDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Produit Produit { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produit = await _context.Produit.FirstOrDefaultAsync(m => m.ProduitId == id);

            if (produit is not null)
            {
                Produit = produit;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produit = await _context.Produit.FindAsync(id);
            if (produit != null)
            {
                Produit = produit;
                _context.Produit.Remove(Produit);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
