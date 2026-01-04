using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.DataContext;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Pages.ProduitsCrud  // <-- check this
{
    public class DeleteModel : PageModel
    {
        private readonly ProduitDBContext _context;

        public DeleteModel(ProduitDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Produit Produit { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Produit = await _context.Produit
                .Include(p => p.Category)   // <-- IMPORTANT
                .FirstOrDefaultAsync(m => m.ProduitId == id);

            if (Produit == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Produit = await _context.Produit.FindAsync(id);

            if (Produit != null)
            {
                _context.Produit.Remove(Produit);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
