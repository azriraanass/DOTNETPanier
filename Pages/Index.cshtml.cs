using DOTNETPanier.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly WebApplication1.DataContext.ProduitDBContext _context;
        private readonly IChatService _chatService;

        public IndexModel(
            WebApplication1.DataContext.ProduitDBContext context,
            IChatService chatService)
        {
            _context = context;
            _chatService = chatService;
        }

        // 1. Lists to hold Database Data
        public IList<Produit> FeaturedProducts { get; set; } = new List<Produit>();
        public IList<Produit> NewArrivals { get; set; } = new List<Produit>();

        // 2. Chat History storage
        public List<MessageLine> ChatHistory { get; set; } = new();

        // 3. Helper class to receive JSON data from JavaScript
        public class ChatRequest
        {
            public string UserMessage { get; set; } = "";
        }

        public async Task OnGetAsync()
        {
            await LoadData();
            LoadChatFromSession();

            // Add this line:
            // This makes ChatHistory available to _Layout and _ChatWidget 
            // without confusing the Model types.
            ViewData["ChatHistory"] = ChatHistory;
        }

        // 4. API Handler for Chat (Prevents Page Reload)
        public async Task<JsonResult> OnPostSendMessageAsync([FromBody] ChatRequest request)
        {
            LoadChatFromSession();

            // Validate input
            if (request == null || string.IsNullOrWhiteSpace(request.UserMessage))
            {
                return new JsonResult(new { reply = "Please type something." });
            }

            // Add User message
            ChatHistory.Add(new MessageLine { Role = "User", Text = request.UserMessage });

            // Call Groq AI Service
            var aiResponse = await _chatService.GetResponseAsync(ChatHistory);

            // Add AI message
            ChatHistory.Add(new MessageLine { Role = "Assistant", Text = aiResponse });

            // Save state
            SaveChatToSession();

            return new JsonResult(new { reply = aiResponse });
        }

        public IActionResult OnPostClearChat()
        {
            HttpContext.Session.Remove("ChatSession");
            return RedirectToPage();
        }

        private async Task LoadData()
        {
            // Logic for "Editor's Picks": Get the 3 most expensive items
            FeaturedProducts = await _context.Produit
                .OrderByDescending(p => p.Price)
                .Take(3)
                .ToListAsync();

            // Logic for "New Arrivals": Get the 4 most recently created items (by ID)
            NewArrivals = await _context.Produit
                .OrderByDescending(p => p.ProduitId)
                .Take(4)
                .ToListAsync();
        }

        private void LoadChatFromSession()
        {
            var json = HttpContext.Session.GetString("ChatSession");
            if (!string.IsNullOrEmpty(json))
            {
                ChatHistory = JsonSerializer.Deserialize<List<MessageLine>>(json) ?? new List<MessageLine>();
            }
        }

        private void SaveChatToSession()
        {
            HttpContext.Session.SetString("ChatSession", JsonSerializer.Serialize(ChatHistory));
        }
    }
}