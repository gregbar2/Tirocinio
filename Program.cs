using ImageDescriptionApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Carica la configurazione da appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Aggiungi servizi al contenitore
builder.Services.AddControllers();

// Aggiungi il servizio ComputerVisionService con chiave API e endpoint dal file di configurazione
builder.Services.AddSingleton<ComputerVisionService>(sp =>
{
    var apiKey = builder.Configuration["Azure:ComputerVision:ApiKey"];
    var endpoint = builder.Configuration["Azure:ComputerVision:Endpoint"];
    return new ComputerVisionService(apiKey, endpoint);
});

builder.Services.AddSingleton<GroqService>(sp =>
{
    var groqKey = builder.Configuration["Groq:ApiKey"];
    return new GroqService(groqKey);
});


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
