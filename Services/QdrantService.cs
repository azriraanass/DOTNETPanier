using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace WebApplication1.Services
{
    public class QdrantService
    {
        private readonly QdrantClient _client;
        private const string CollectionName = "produits_rag";

        public QdrantService(IConfiguration config)
        {
            // Récupère l'URL de Qdrant depuis appsettings
            _client = new QdrantClient(new Uri("http://localhost:6334"));
        }

        // Cette méthode cherche les produits les plus pertinents
        public async Task<string> GetContextAsync(float[] queryVector)
        {
            var searchResults = await _client.SearchAsync(CollectionName, queryVector, limit: 3);

            if (!searchResults.Any()) return "Aucun produit spécifique trouvé.";

            var contextStrings = searchResults.Select(r =>
                $"Produit: {r.Payload["name"].StringValue}, " +
                $"Prix: {r.Payload["price"].DoubleValue}€, " +
                $"Description: {r.Payload["description"].StringValue}");

            return string.Join("\n---\n", contextStrings);
        }
    }
}