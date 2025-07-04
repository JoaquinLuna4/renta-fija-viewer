
using RentaFijaApi.Services;
using System.Net.Http;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar HttpClient
builder.Services.AddHttpClient<RentaFijaService>(); // HttpClient ser� inyectado en RentaFijaService


// Registrar Service para EXTRAER PDF
builder.Services.AddScoped<IPdfExtractionService, PdfExtractionService>();

// --- SECCI�N CLAVE DE CONFIGURACI�N DE GEMINI ---

// Registrar Service para llamar a la API de Gemini
var useGeminiSimulation = builder.Configuration.GetValue<bool>("Gemini:UseSimulation");
Console.WriteLine($"[DEBUG - Program.cs] useGeminiSimulation le�do: {useGeminiSimulation}");

var geminiApiKey = builder.Configuration["Gemini:ApiKey"]; // Lee la clave de configuraci�n
var geminiApiUrl = builder.Configuration["Gemini:ApiUrl"]; // Lee la URL base de configuraci�n
builder.Services.AddHttpClient<IGeminiApiService, GeminiApiService>(); // Para que el HttpClient se inyecte
builder.Services.AddSingleton<IGeminiApiService>(sp =>
{
    
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new GeminiApiService(httpClient, geminiApiKey, geminiApiUrl, useGeminiSimulation);
}
);
// --- FIN DE LA SECCI�N DE CONFIGURACI�N DE GEMINI ---

// Configurar CORS para permitir que tu frontend de React acceda a la API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173") // Reemplaza con la URL de tu frontend de React
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp"); // Usar la pol�tica CORS

app.UseAuthorization();

app.MapControllers();

app.Run();