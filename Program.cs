using ImageDescriptionApp;
//using ImageDescriptionApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Google.Rpc.Context.AttributeContext.Types;





var builder = WebApplication.CreateBuilder(args);

// Carica la configurazione da appsettings.json
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddTransient<ClarifaiClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var clarifaiApiKey = configuration["Clarifai:ApiKey"];
    Console.WriteLine("CLARIFAI KEY: " + clarifaiApiKey); // solo per debug, NON in produzione
    return new ClarifaiClient(clarifaiApiKey);
});


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


// Aggiungi servizi al contenitore
builder.Services.AddControllers();





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
