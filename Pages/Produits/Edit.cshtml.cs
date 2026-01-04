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

namespace WebApplication1.Pages.Produits
{
    public class EditModel : PageModel
    {
        private readonly WebApplication1.DataContext.ProduitDBContext _context;

        public EditModel(WebApplication1.DataContext.ProduitDBContext context)
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

            var produit =  await _context.Produit.FirstOrDefaultAsync(m => m.ProduitId == id);
            if (produit == null)
            {
                return NotFound();
            }
            Produit = produit;
           ViewData["CategoryId"] = new SelectList(_context.Category, "CategoryId", "CategoryId");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Produit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProduitExists(Produit.ProduitId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ProduitExists(Guid id)
        {
            return _context.Produit.Any(e => e.ProduitId == id);
        }
    }
}
