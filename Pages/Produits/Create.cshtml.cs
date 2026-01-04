using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.DataContext;
using WebApplication1.Models;

namespace WebApplication1.Pages.Produits
{
    public class CreateModel : PageModel
    {
        private readonly WebApplication1.DataContext.ProduitDBContext _context;

        public CreateModel(WebApplication1.DataContext.ProduitDBContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["CategoryId"] = new SelectList(_context.Category, "CategoryId", "CategoryId");
            return Page();
        }

        [BindProperty]
        public Produit Produit { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Produit.Add(Produit);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
