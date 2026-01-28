using WebApplication1.Models;

namespace DOTNETPanier.Services
{
    public interface IChatService
    {
        Task<string> GetResponseAsync(List<MessageLine> history); // Must match exactly
    }

}