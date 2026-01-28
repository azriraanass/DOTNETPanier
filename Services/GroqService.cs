using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using WebApplication1.Models;

namespace DOTNETPanier.Services
{
    public class GroqService : IChatService
    {
        private readonly IHttpClientFactory _http;
        private readonly string _model;

        public GroqService(IHttpClientFactory http, IConfiguration config)
        {
            _http = http;
            _model = config["Groq:Model"] ?? "groq-mini"; // your Groq model
        }

        public async Task<string> GetResponseAsync(List<MessageLine> history)
        {
            var client = _http.CreateClient("Groq");

            // Map history to the format Groq expects
            var apiMessages = history.Select(m => new
            {
                role = m.Role.ToLower() == "assistant" ? "assistant" : "user",
                content = m.Text
            }).ToList();

            // Always insert a system message at the very beginning (index 0)
            apiMessages.Insert(0, new { role = "system", content = "You are a helpful store assistant." });

            var body = new
            {
                model = _model, // This will now be "llama-3.3-70b-versatile"
                messages = apiMessages,
                temperature = 0.7
            };

            var response = await client.PostAsJsonAsync("chat/completions", body);

            if (!response.IsSuccessStatusCode)
            {
                // This will print the EXACT reason in your Visual Studio Debug/Output window
                var errorDetails = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq Error: {errorDetails}");
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }
    }
}
