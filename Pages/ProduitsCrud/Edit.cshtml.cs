using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DataContext;
using WebApplication1.Models;

namespace WebApplication1.Pages.ProduitsCrud
{
    public class EditModel : PageModel
    {
        private readonly ProduitDBContext _context;

        public EditModel(ProduitDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Produit Produit { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
                return NotFound();

            Produit = await _context.Produit
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProduitId == id);

            if (Produit == null)
                return NotFound();

            // Load dropdown with category names
            ViewData["CategoryId"] = new SelectList(
                _context.Category,
                "CategoryId",
                "Name",
                Produit.CategoryId // pre-select correct category
            );

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ViewData["CategoryId"] = new SelectList(
                    _context.Category,
                    "CategoryId",
                    "Name",
                    Produit.CategoryId
                );

                return Page();
            }

            _context.Attach(Produit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Produit.Any(e => e.ProduitId == Produit.ProduitId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToPage("./Index");
        }
    }
}
