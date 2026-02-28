using System.ClientModel.Primitives;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI;
using chat_web_demo.Components;
using chat_web_demo.Services;
using chat_web_demo.Services.Ingestion;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// You will need to set the endpoint and key to your own values
// You can do this using Visual Studio's "Manage User Secrets" UI, or on the command line:
//   cd this-project-directory
//   dotnet user-secrets set AzureOpenAI:Endpoint https://YOUR-DEPLOYMENT-NAME.openai.azure.com
var azureOpenAIEndpoint = new Uri(new Uri(builder.Configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Missing configuration: AzureOpenAi:Endpoint. See the README for details.")), "/openai/v1");
var azureOpenAIKey = builder.Configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Missing configuration: AzureOpenAi:ApiKey. See the README for details.");
#pragma warning disable OPENAI001 // OpenAIClient(AuthenticationPolicy, OpenAIClientOptions) and GetResponsesClient(string) are experimental and subject to change or removal in future updates.
var azureOpenAi = new OpenAIClient(
    new ApiKeyCredential(azureOpenAIKey), // 改用 ApiKeyCredential
    new OpenAIClientOptions { Endpoint = azureOpenAIEndpoint });

var chatClient = azureOpenAi.GetResponsesClient("gpt-5.2-chat").AsIChatClient();
#pragma warning restore OPENAI001

var embeddingGenerator = azureOpenAi.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();

var vectorStorePath = Path.Combine(AppContext.BaseDirectory, "vector-store.db");
var vectorStoreConnectionString = $"Data Source={vectorStorePath}";
builder.Services.AddSqliteVectorStore(_ => vectorStoreConnectionString);
builder.Services.AddSqliteCollection<string, IngestedChunk>(IngestedChunk.CollectionName, vectorStoreConnectionString);

builder.Services.AddSingleton<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();
builder.Services.AddKeyedSingleton("ingestion_directory", new DirectoryInfo(Path.Combine(builder.Environment.WebRootPath, "Data")));
builder.Services.AddChatClient(chatClient).UseFunctionInvocation().UseLogging();
builder.Services.AddEmbeddingGenerator(embeddingGenerator);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
