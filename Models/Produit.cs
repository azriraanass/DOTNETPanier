using System.ComponentModel.DataAnnotations;



namespace WebApplication1.Models
{
    public class Produit
    {
        [Key]
        public Guid ProduitId { get; set; }

        public string? Name { get; set; }

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public string? Description { get; set; }

        public int Stock { get; set; }

        // One To Many Relationship with Category
        public Guid CategoryId { get; set; } // Cle Etrangere

        public Category? Category { get; set; }

    }
}
