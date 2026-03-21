using AguaMinami.API.Data;
using AguaMinami.API.Repositories;

var builder = WebApplication.CreateBuilder(args);

// --- Forzar puerto 5000 para que la app WPF sepa dónde conectar ---
builder.WebHost.UseUrls("http://127.0.0.1:5000");

// --- Capa de acceso a datos ---
builder.Services.AddSingleton<DbConnectionFactory>();

// --- Repositorios (interfaz → implementación = desacoplamiento) ---
builder.Services.AddScoped<IOfertaRepository, OfertaRepository>();
builder.Services.AddScoped<IOfertaCantidadRepository, OfertaCantidadRepository>();
builder.Services.AddScoped<IOfertaDescuentoRepository, OfertaDescuentoRepository>();
builder.Services.AddScoped<IOfertaAsignacionRepository, OfertaAsignacionRepository>();
builder.Services.AddScoped<IVarianteOfertaRepository, VarianteOfertaRepository>();

// --- Controladores ---
builder.Services.AddControllers();

// --- CORS: permitir que la app WPF (u otro cliente) acceda a la API ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowAll");
app.MapControllers();

// Health check ligero para verificar que la API esta viva
app.MapGet("/health", () => Results.Ok("ok"));

// Pre-calentar el connection pool de SQL Server al arrancar
try
{
    var db = app.Services.GetRequiredService<DbConnectionFactory>();
    using var conn = db.CreateConnection();
    conn.Open();
}
catch { /* Si falla, la API seguira funcionando y reintentara en la primera request */ }

app.Run();