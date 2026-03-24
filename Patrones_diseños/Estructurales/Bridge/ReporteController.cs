// ---- Registro en Program.cs ----
// Las implementaciones se registran con sus nombres para resolverlas por string
builder.Services.AddScoped<RendizadorJson>();
builder.Services.AddScoped<RendizadorCsv>();
builder.Services.AddScoped<RendizadorImpresora>();

// Factory que resuelve el rendizador correcto según el parámetro "formato"
builder.Services.AddScoped<Func<string, IRendizador>>(sp => formato => formato switch
{
    "json"      => sp.GetRequiredService<RendizadorJson>(),
    "csv"       => sp.GetRequiredService<RendizadorCsv>(),
    "impresora" => sp.GetRequiredService<RendizadorImpresora>(),
    _ => throw new ArgumentException($"Formato '{formato}' no soportado.")
});

// ──────────────────────────────────────────────

[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly IReporteServiceProxy _proxy;   // Proxy (patrón 11)
    private readonly Func<string, IRendizador> _rendizadorFactory;
    private readonly IVentaRepository _ventaRepo;
    private readonly INominaRepository _nominaRepo;
    private readonly AppConfiguracion _config;

    // GET /api/reportes/ventas?formato=csv&desde=2025-01-01&hasta=2025-01-31
    [HttpGet("ventas")]
    public async Task<IActionResult> ReporteVentas(
        [FromQuery] string formato  = "json",
        [FromQuery] DateTime desde  = default,
        [FromQuery] DateTime hasta  = default)
    {
        var filtro     = new FiltroReporte { FechaInicio = desde, FechaFin = hasta };
        var rendizador = _rendizadorFactory(formato);

        // Bridge: inyectamos el rendizador elegido en tiempo de ejecución
        var reporte    = new ReporteVentas(rendizador, _config, _ventaRepo);
        var resultado  = await _proxy.GenerarReporteAsync(
                             () => reporte.GenerarAsync(filtro),
                             User, "Ventas");

        return resultado.Extension == ".json"
            ? Ok(resultado.Contenido)
            : File(System.Text.Encoding.UTF8.GetBytes(resultado.Contenido.ToString()!),
                   resultado.ContentType,
                   resultado.NombreArchivo);
    }

    // GET /api/reportes/nomina?formato=impresora&desde=...&hasta=...
    [HttpGet("nomina")]
    public async Task<IActionResult> ReporteNomina(
        [FromQuery] string formato  = "json",
        [FromQuery] DateTime desde  = default,
        [FromQuery] DateTime hasta  = default)
    {
        var filtro     = new FiltroReporte { FechaInicio = desde, FechaFin = hasta };
        var rendizador = _rendizadorFactory(formato);

        // Misma abstracción, diferente implementación — el Bridge brilla aquí
        var reporte   = new ReporteNomina(rendizador, _config, _nominaRepo);
        var resultado = await _proxy.GenerarReporteAsync(
                            () => reporte.GenerarAsync(filtro),
                            User, "Nómina");

        return resultado.Extension == ".json"
            ? Ok(resultado.Contenido)
            : File(System.Text.Encoding.UTF8.GetBytes(resultado.Contenido.ToString()!),
                   resultado.ContentType,
                   resultado.NombreArchivo);
    }
}
