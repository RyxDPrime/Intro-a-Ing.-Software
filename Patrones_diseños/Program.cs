using AguaMinami.Application.Inventory;
using AguaMinami.Application.Mediator;
using AguaMinami.Application.Mediator.Handlers;
using AguaMinami.Application.Reports;
using AguaMinami.Infrastructure.Config;
using AguaMinami.Infrastructure.Inventory.Decorators;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Singleton base configuration reused by the rest of the registrations.
var config = AppConfiguracion.ObtenerInstancia();
builder.Services.AddSingleton(config);

RegistrarBase(builder, config);
RegistrarObserver(builder);
RegistrarDecorator(builder);
RegistrarChainYCommand(builder);
RegistrarFactoryYBuilder(builder);
RegistrarMediator(builder);
RegistrarProxy(builder);
RegistrarFacade(builder);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

await InicializarStockSiExisteAsync(app);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

static void RegistrarBase(WebApplicationBuilder builder, AppConfiguracion config)
{
    builder.Services.AddDbContext<AguaMinamiDbContext>(options =>
        options.UseSqlServer(
            config.CadenaConexion,
            sql => sql.CommandTimeout(config.TimeoutConexion)
        )
    );

    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenLocalhost(config.PuertoAPI);
    });

    builder.Services.AddMemoryCache();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddScoped<IMovimientoRepository, MovimientoRepository>();
    builder.Services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
    builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
    builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();
    builder.Services.AddScoped<IProveedorService, ProveedorService>();
}

static void RegistrarObserver(WebApplicationBuilder builder)
{
    builder.Services.AddSingleton<AlertaComprasObserver>();
    builder.Services.AddSingleton<LogMovimientoObserver>();
    builder.Services.AddSingleton<NotificacionAdminObserver>();

    builder.Services.AddSingleton<StockAlmacen>(sp =>
    {
        var almacen = new StockAlmacen(
            1,
            "Almacén Principal",
            sp.GetRequiredService<AppConfiguracion>(),
            sp.GetRequiredService<IMovimientoRepository>()
        );

        almacen.Suscribir(sp.GetRequiredService<AlertaComprasObserver>());
        almacen.Suscribir(sp.GetRequiredService<LogMovimientoObserver>());
        almacen.Suscribir(sp.GetRequiredService<NotificacionAdminObserver>());

        return almacen;
    });
}

static void RegistrarDecorator(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<IInventarioRepository>(sp =>
    {
        IInventarioRepository repo = new InventarioRepository(
            sp.GetRequiredService<AguaMinamiDbContext>()
        );

        repo = new ValidacionInventarioDecorator(
            repo,
            sp.GetRequiredService<AppConfiguracion>()
        );

        repo = new CacheInventarioDecorator(
            repo,
            sp.GetRequiredService<IMemoryCache>()
        );

        repo = new LoggingInventarioDecorator(
            repo,
            sp.GetRequiredService<ILogger<LoggingInventarioDecorator>>()
        );

        return repo;
    });
}

static void RegistrarChainYCommand(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<AutenticacionHandler>();
    builder.Services.AddScoped<ValidacionStockHandler>();
    builder.Services.AddScoped<ValidacionPrecioHandler>();
    builder.Services.AddScoped<AplicacionOfertasHandler>();
    builder.Services.AddScoped<VentaChain>();
    builder.Services.AddScoped<InventarioInvoker>();
}

static void RegistrarFactoryYBuilder(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<DocumentoCreator>();
    builder.Services.AddScoped<PedidoDirector>(sp =>
        new PedidoDirector(new PedidoLocalBuilder())
    );
}

static void RegistrarMediator(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<InventarioVentaHandler>();
    builder.Services.AddScoped<FacturacionVentaHandler>();
    builder.Services.AddScoped<NotificacionVentaHandler>();
    builder.Services.AddScoped<ReporteMercanciaHandler>();
    builder.Services.AddScoped<GastoNominaHandler>();

    builder.Services.AddSingleton<ISistemaMediator>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<SistemaMediator>>();
        var mediator = new SistemaMediator(logger);

        using var scope = sp.CreateScope();

        mediator.Suscribir(scope.ServiceProvider
            .GetRequiredService<InventarioVentaHandler>());
        mediator.Suscribir(scope.ServiceProvider
            .GetRequiredService<FacturacionVentaHandler>());
        mediator.Suscribir(scope.ServiceProvider
            .GetRequiredService<NotificacionVentaHandler>());
        mediator.Suscribir(scope.ServiceProvider
            .GetRequiredService<ReporteMercanciaHandler>());
        mediator.Suscribir(scope.ServiceProvider
            .GetRequiredService<GastoNominaHandler>());

        return mediator;
    });
}

static void RegistrarProxy(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<ReporteService>();

    builder.Services.AddScoped<IReporteService>(sp =>
        new ReporteServiceProxy(
            real: sp.GetRequiredService<ReporteService>(),
            http: sp.GetRequiredService<IHttpContextAccessor>(),
            auditoria: sp.GetRequiredService<IAuditoriaRepository>(),
            logger: sp.GetRequiredService<ILogger<ReporteServiceProxy>>()
        )
    );
}

static void RegistrarFacade(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<VentaFacade>();
}

static async Task InicializarStockSiExisteAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var almacen = scope.ServiceProvider.GetService<StockAlmacen>();
    if (almacen is not null)
    {
        await almacen.InicializarStockAsync();
    }
}