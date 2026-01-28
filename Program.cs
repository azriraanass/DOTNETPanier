using DOTNETPanier.Services;
using DOTNETPanier.Services;
using DOTNETPanier.Services.Cache;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WebApplication1.DataContext;
using WebApplication1.Services;
using WebApplication1.Services.Cookies.CarteItem;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

// Read Groq config
var groqConfig = builder.Configuration.GetSection("Groq");
var groqApiKey = groqConfig["ApiKey"];
var groqBaseUrl = groqConfig["BaseUrl"];

// Register HttpClient for Groq
// Register HttpClient for Groq
builder.Services.AddHttpClient("Groq", client =>
{
    // Ensure the URL ends with a slash so HttpClient doesn't cut off the 'v1'
    string url = groqBaseUrl;
    if (!url.EndsWith("/"))
    {
        url += "/";
    }

    client.BaseAddress = new Uri(url);
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", groqApiKey);
});

// Register service as IChatService
builder.Services.AddSingleton<IChatService, GroqService>();
builder.Services.AddSingleton<QdrantService>();
builder.Services.AddScoped<RAGSyncService>();


// 🔐 Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
    });

builder.Services.AddAuthorization();

// 🔴 Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetValue<string>("Redis:ConnectionString");
    return ConnectionMultiplexer.Connect(configuration);
});

// 🔴 Redis Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("Redis:ConnectionString");
    options.InstanceName = "WebApp1:";
});

// DB Context
builder.Services.AddDbContext<ProduitDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CarteItemCookiesManager>();
builder.Services.AddScoped<ProduitCacheService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthentication();   // ✅ REQUIRED
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
