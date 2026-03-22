// Program.cs — registro y cableado de observers en el contenedor DI
// Los observers se suscriben al StockAlmacen al arrancar el servidor

using AguaMinami.Application.Inventory;
using AguaMinami.Infrastructure.Config;

var builder = WebApplication.CreateBuilder(args);

// ── Singleton de configuración (patrón 1) ──
builder.Services.AddSingleton(AppConfiguracion.ObtenerInstancia());

// ── Repositories ──
builder.Services.AddScoped<IMovimientoRepository,  MovimientoRepository>();
builder.Services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
builder.Services.AddScoped<IAuditoriaRepository,   AuditoriaRepository>();
builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();
builder.Services.AddScoped<IProveedorService,      ProveedorService>();

// ── Observers (Singleton: se crean una vez y viven toda la app) ──
builder.Services.AddSingleton<AlertaComprasObserver>();
builder.Services.AddSingleton<LogMovimientoObserver>();
builder.Services.AddSingleton<NotificacionAdminObserver>();

// ── StockAlmacen como Singleton con observers ya cableados ──
builder.Services.AddSingleton<StockAlmacen>(sp =>
{
    var config  = sp.GetRequiredService<AppConfiguracion>();
    var repo    = sp.GetRequiredService<IMovimientoRepository>();

    var almacen = new StockAlmacen(1, "Almacén Principal", config, repo);

    // Cablear los tres observers al arrancar
    almacen.Suscribir(sp.GetRequiredService<AlertaComprasObserver>());
    almacen.Suscribir(sp.GetRequiredService<LogMovimientoObserver>());
    almacen.Suscribir(sp.GetRequiredService<NotificacionAdminObserver>());

    return almacen;
});

builder.Services.AddControllers();

var app = builder.Build();

// Inicializar stock desde BD al arrancar el servidor
var almacenPrincipal = app.Services.GetRequiredService<StockAlmacen>();
await almacenPrincipal.InicializarStockAsync();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();