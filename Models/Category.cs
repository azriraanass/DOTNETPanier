using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace WebApplication1.Models
{
    public class Category
    {
        [Key]
        public Guid CategoryId { get; set; }
        public string? Name { get; set; }

        // One To Many Relationship with Produit
        [JsonIgnore]
        public ICollection<Produit>? Produits { get; set; }

    }
}
