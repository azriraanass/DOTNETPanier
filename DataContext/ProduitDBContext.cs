using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.DataContext
{
    public class ProduitDBContext : DbContext
    {
        public ProduitDBContext(DbContextOptions<ProduitDBContext> options) : base(options)
        {
        }
        public DbSet<WebApplication1.Models.Produit> Produit { get; set; } = default!;
        public DbSet<Category> Category { get; set; } = default!;


    }
}
