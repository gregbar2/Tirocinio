using ImageDescriptionApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Carica la configurazione da appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Aggiungi servizi al contenitore
builder.Services.AddControllers();

// Leggi la chiave API dal file `appsettings.json` o configurazione
var hugApiKey = builder.Configuration["HuggingFace:ApiKey"];

// Registrazione dei servizi nel contenitore DI
builder.Services.AddSingleton<HuggingFaceService>(sp =>
{
    return new HuggingFaceService(hugApiKey);  // Passa la chiave API al costruttore
});

// Aggiungi il servizio ComputerVisionService con chiave API e endpoint dal file di configurazione
builder.Services.AddSingleton<ComputerVisionService>(sp =>
{
    var apiKey = builder.Configuration["Azure:ComputerVision:ApiKey"];
    var endpoint = builder.Configuration["Azure:ComputerVision:Endpoint"];
    return new ComputerVisionService(apiKey, endpoint);
});


builder.Services.AddSingleton<ImageDescriptionService>(); // Registrazione di ImageDescriptionService


// Configura Swagger (se vuoi usare la documentazione API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura la pipeline delle richieste HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
