
// ─────────────────────────────────────────────────────────────
// ReportesController — totalmente limpio gracias al Proxy
// No tiene ni una línea de verificación de roles.
// Si el usuario no tiene permiso, el Proxy lanza la excepción
// antes de que el servicio real ejecute una sola consulta a BD.
// ─────────────────────────────────────────────────────────────

[ApiController]
[Route("api/reportes")]
[Authorize]           // solo requiere estar autenticado — el rol lo valida el Proxy
public class ReportesController : ControllerBase
{
    private readonly IReporteService _reportes;  // recibe el Proxy

    public ReportesController(IReporteService reportes) =>
        _reportes = reportes;

    // GET api/reportes/ventas?desde=2026-01-01&hasta=2026-03-31
    // Administrador: obtiene el reporte completo
    // Asistente:     recibe 403 con mensaje descriptivo
    [HttpGet("ventas")]
    public async Task<IActionResult> ReporteVentas(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] int?     idProducto = null,
        [FromQuery] int?     idRuta     = null)
    {
        try
        {
            var filtro  = new FiltroReporte(desde, hasta, null, idProducto, idRuta);
            var reporte = await _reportes.GenerarReporteVentas(filtro);
            return Ok(reporte);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Error = ex.Message });
        }
    }

    // GET api/reportes/nomina?anio=2026&mes=3
    [HttpGet("nomina")]
    public async Task<IActionResult> ReporteNomina(
        [FromQuery] int anio,
        [FromQuery] int mes)
    {
        try
        {
            var reporte = await _reportes.GenerarReporteNomina(anio, mes);
            return Ok(reporte);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Error = ex.Message });
        }
    }

    // GET api/reportes/inventario
    [HttpGet("inventario")]
    public async Task<IActionResult> ReporteInventario(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        try
        {
            var reporte = await _reportes
                .GenerarReporteInventario(new FiltroReporte(desde, hasta));
            return Ok(reporte);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Error = ex.Message });
        }
    }

    // GET api/reportes/gastos
    [HttpGet("gastos")]
    public async Task<IActionResult> ReporteGastos(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        try
        {
            var reporte = await _reportes
                .GenerarReporteGastos(new FiltroReporte(desde, hasta));
            return Ok(reporte);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Error = ex.Message });
        }
    }
}