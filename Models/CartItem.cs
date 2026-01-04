using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class CartItem
    {
        public Guid ProduitId { get; set; }  // Only store Product ID
        public int Quantity { get; set; }

        // Optional: compute TotalPrice later when you fetch the Product from DB
        public decimal TotalPrice { get; set; }

        [JsonIgnore]
        public Produit? Produit { get; set; } // for display only, ignored by JSON
    }
}
