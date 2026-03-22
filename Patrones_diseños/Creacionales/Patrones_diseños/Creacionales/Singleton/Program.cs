// Program.cs — ASP.NET 9
// Registro del Singleton en el contenedor de DI nativo

using AguaMinami.Infrastructure.Config;

var builder = WebApplication.CreateBuilder(args);

// ── Registrar el Singleton en el contenedor de DI de ASP.NET ──
// AddSingleton garantiza una sola instancia por proceso,
// igual que nuestro patrón manual pero integrado con la DI.
builder.Services.AddSingleton(AppConfiguracion.ObtenerInstancia());

// ── Entity Framework con la cadena del Singleton ──
var config = AppConfiguracion.ObtenerInstancia();

builder.Services.AddDbContext<AguaMinamiDbContext>(options =>
    options.UseSqlServer(
        config.CadenaConexion,
        sql => sql.CommandTimeout(config.TimeoutConexion)
    )
);

// ── Configurar Kestrel con la IP y puerto del Singleton ──
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(config.PuertoAPI);
    // Solo acepta conexiones de la red LAN interna
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();