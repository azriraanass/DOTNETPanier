# 🛒 DOTNETPanier

> An ASP.NET e-commerce application enriched with AI capabilities — featuring semantic product search, a conversational store assistant powered by LLaMA via Groq, vector-based RAG (Retrieval-Augmented Generation), and Redis caching for high-performance product delivery.

---

## 🚀 Features

- 🗃️ **Product catalog** with category relationships, backed by SQL Server and Entity Framework Core
- ⚡ **Redis distributed cache** for fast product retrieval with automatic TTL and invalidation
- 🧠 **RAG pipeline** — products are embedded and stored in Qdrant for semantic context retrieval
- 🤖 **AI chat assistant** — uses Groq's LLaMA model to answer customer queries grounded in real product data
- 🔍 **Vector search** — find semantically relevant products using cosine similarity on 768-dim embeddings
- 🔄 **Product sync service** — automatically indexes all products into the vector DB on demand
- 🧩 **Clean service architecture** — separated concerns via interfaces and dependency injection

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core (C#) |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Cache | Redis (via `IDistributedCache`) |
| Vector DB | Qdrant (Docker) |
| Embeddings | Ollama — `nomic-embed-text` (768 dims) |
| LLM / Chat | Groq API — LLaMA model |
| HTTP Client | `IHttpClientFactory` (named clients) |

---

## 📂 Project Structure

```
DOTNETPanier/
│
├── Services/
│   ├── Cache/
│   │   └── ProduitCacheService.cs     # Redis cache layer for product reads and invalidation
│   │
│   ├── GroqService.cs                 # Groq API client — sends chat history, returns LLM response
│   ├── IChatService.cs                # Abstraction interface for any chat/LLM backend
│   ├── NomicEmbeddingService.cs       # Calls local Ollama to generate text embeddings
│   ├── QdrantService.cs               # Manages vector collection, upsert, and semantic search
│   └── RAGSyncService.cs              # Orchestrates DB → embedding → Qdrant indexing pipeline
│
├── Models/
│   ├── Produit.cs                     # Product entity (Id, Name, Price, Description, Category)
│   ├── Category.cs                    # Category entity
│   └── MessageLine.cs                 # Chat message model (Role + Text)
│
└── DataContext/
    └── ProduitDBContext.cs            # EF Core DbContext
```

### Key service responsibilities

- **`ProduitCacheService`** — wraps all product DB queries behind Redis. If a cached entry exists it is returned immediately; otherwise the DB is queried and the result is stored for 10 minutes. Cache is explicitly invalidated on writes.
- **`GroqService`** — implements `IChatService`. Sends a full conversation history (with roles: `user`, `assistant`, `system`) to the Groq `/chat/completions` endpoint and returns the model's reply. Temperature is deliberately kept low (`0.2`) to reduce hallucinations.
- **`NomicEmbeddingService`** — calls Ollama's local `/api/embed` endpoint with the `nomic-embed-text` model to convert product text into a `List<float>` vector of 768 dimensions.
- **`QdrantService`** — manages the `produits_rag` collection in Qdrant. Auto-creates the collection if it doesn't exist. Supports upserting product vectors with a `full_text` payload and searching by cosine similarity.
- **`RAGSyncService`** — the glue layer. Fetches all products from SQL Server, formats a descriptive string per product, generates its embedding, then upserts it into Qdrant. Call this to re-index the catalog.

---

## ⚡ Installation

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or remote)
- Docker (for Qdrant and optionally Redis)
- [Ollama](https://ollama.com/) with `nomic-embed-text` pulled

### 1. Clone the repository

```bash
git clone https://github.com/your-username/DOTNETPanier.git
cd DOTNETPanier
```

### 2. Start infrastructure services

```bash
# Redis
docker run -d -p 6379:6379 redis

# Qdrant
docker run -d -p 6333:6333 -p 6334:6334 qdrant/qdrant
```

### 3. Pull the embedding model via Ollama

```bash
ollama pull nomic-embed-text
```

### 4. Configure the application

Copy the example config and fill in your values (see [Environment Variables](#-environment-variables)):

```bash
cp appsettings.example.json appsettings.json
```

### 5. Apply database migrations

```bash
dotnet ef database update
```

### 6. Run the application

```bash
dotnet run
```

---

## ▶️ Usage

### Index products into the vector database

Before the AI chat can answer product-related queries, products must be synced to Qdrant. Call the sync endpoint (or inject `RAGSyncService` and trigger it at startup):

```
POST /api/rag/sync
```

### Chat with the AI assistant

Send a conversation history to get a context-aware response grounded in your product catalog:

```
POST /api/chat
Content-Type: application/json

{
  "messages": [
    { "role": "user", "text": "Do you have any running shoes under 80€?" }
  ]
}
```

---

## 🔌 API Endpoints

> *(Inferred from service design — update with your actual controllers)*

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/produits` | List all products (Redis-cached) |
| `GET` | `/api/produits/{id}` | Get product by ID (Redis-cached) |
| `POST` | `/api/chat` | Send a message, receive an AI reply |
| `POST` | `/api/rag/sync` | Re-index all products into Qdrant |

---

## 🧠 Cache System

Products are cached in **Redis** using `IDistributedCache` with the following strategy:

| Cache Key | Content | TTL |
|-----------|---------|-----|
| `produits:all` | Full product list with categories | 10 minutes |
| `produit:{id}` | Single product with category | 10 minutes |

**Cache invalidation** is explicit — when a product is created or updated, `ClearCacheAsync()` must be called to remove both the list cache and the affected product's individual cache entry. This prevents stale data from being served.

---

## 📊 Vector Database

**Qdrant** stores product embeddings in a collection called `produits_rag`.

| Setting | Value |
|---------|-------|
| Collection | `produits_rag` |
| Vector size | `768` (nomic-embed-text) |
| Distance metric | Cosine similarity |
| Results per query | Top 20 |

Each vector point carries a `full_text` payload in the format:

```
Produit: {Name}, Prix: {Price}€, Catégorie: {Category}. Description: {Description}
```

This payload is retrieved during chat queries and passed as context to the LLM.

The collection is **auto-created** on first use — no manual Qdrant setup required.

---

## 🤖 AI Integration

The AI pipeline follows a classic **RAG (Retrieval-Augmented Generation)** pattern:

```
User query
    │
    ▼
Generate query embedding (Ollama / nomic-embed-text)
    │
    ▼
Search Qdrant for top-20 similar product vectors
    │
    ▼
Assemble product context (full_text payloads)
    │
    ▼
Send context + conversation history to Groq (LLaMA)
    │
    ▼
Return grounded AI response to user
```

- The system prompt (`"You are a helpful store assistant."`) is automatically injected if not already present.
- **Temperature is set to `0.2`** to keep answers factual and prevent the model from inventing products.
- The `IChatService` interface means the LLM backend can be swapped (e.g., replaced with OpenAI or a local model) without changing the rest of the application.

---

## 🔐 Environment Variables

Configure via `appsettings.json` or environment variables. **Never commit real secrets.**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PanierDB;User Id=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;"
  },
  "Redis": {
    "Configuration": "localhost:6379"
  },
  "Qdrant": {
    "Url": "http://localhost:6334"
  },
  "Groq": {
    "ApiKey": "YOUR_GROQ_API_KEY",
    "Model": "llama3-8b-8192"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434"
  }
}
```

> ⚠️ Add `appsettings.json` (with real secrets) to `.gitignore`. Only commit `appsettings.example.json`.

---

## 📌 Notes & Best Practices

- **Always call `RAGSyncService.SyncAllProducts()`** after bulk product imports, or hook it into your product creation/update flow, to keep the vector index in sync with the SQL database.
- **Cache invalidation is critical** — any write operation on a product must call `ClearCacheAsync()` to avoid serving outdated data to users.
- **Qdrant runs over gRPC (port 6334)** for the .NET client and HTTP (port 6333) for the dashboard UI. Make sure both ports are exposed in Docker.
- **Ollama must be running locally** before the application starts, since embeddings are generated synchronously during product sync.
- The `IHttpClientFactory` pattern is used for both Groq and Ollama HTTP clients — register them as named clients in `Program.cs` with appropriate base addresses and headers (including `Authorization: Bearer YOUR_GROQ_API_KEY` for Groq).
- Consider running `SyncAllProducts` as a **background/hosted service** on startup to ensure the vector index is always fresh after a redeployment.
