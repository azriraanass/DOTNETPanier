using Microsoft.EntityFrameworkCore; // <--- INDISPENSABLE pour ToListAsync et Include
using WebApplication1.DataContext;
using WebApplication1.Services;
using WebApplication1.Models; // Assure-toi d'avoir accès à tes modèles

namespace DOTNETPanier.Services
{
    public class RAGSyncService
    {
        private readonly ProduitDBContext _db;
        private readonly QdrantService _qdrant;

        public RAGSyncService(ProduitDBContext db, QdrantService qdrant)
        {
            _db = db;
            _qdrant = qdrant;
        }

        public async Task SyncAllProducts()
        {
            // Vérifie que ta table s'appelle bien "Produit" dans ton DbContext
            var produits = await _db.Produit
                                    .Include(p => p.Category)
                                    .ToListAsync();

            foreach (var p in produits)
            {
                string context = $"Produit: {p.Name}, Prix: {p.Price}€, Catégorie: {p.Category?.Name}. Description: {p.Description}";

                // On utilise un vecteur de 384 si tu utilises un modèle léger type All-MiniLM
                // ou 1536 pour OpenAI.
                float[] mockVector = new float[1536];

                // CORRECTION : Qdrant attend souvent une chaîne ou un ulong pour l'ID. 
                // Si ton UpsertProductAsync attend un Guid, assure-toi que la signature correspond.
                //await _qdrant.UpsertProductAsync(p.ProduitId, mockVector, context);
            }
        }
    }
}