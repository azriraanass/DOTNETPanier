namespace WebApplication1.Models
{
    public class ProduitVector
    {
        public Guid Id { get; set; }
        public string TextToEmbed { get; set; } // Nom + Description + Catégorie
        public List<float> Vector { get; set; }
        public Dictionary<string, object> Payload { get; set; }
    }
}