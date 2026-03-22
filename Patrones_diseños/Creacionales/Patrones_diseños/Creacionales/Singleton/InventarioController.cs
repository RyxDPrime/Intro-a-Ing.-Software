// InventarioController.cs — ejemplo de uso real
// El controlador de inventario usa el Singleton para:
//   1. Verificar stock mínimo con el valor centralizado
//   2. Saber la impresora activa para reportes

using AguaMinami.Infrastructure.Config;

[ApiController]
[Route("api/[controller]")]
public class InventarioController : ControllerBase
{
    private readonly AppConfiguracion _config;
    private readonly IInventarioService _servicio;

    public InventarioController(AppConfiguracion config, IInventarioService servicio)
    {
        // ASP.NET inyecta la misma instancia Singleton registrada en Program.cs
        _config  = config;
        _servicio = servicio;
    }

    [HttpGet("alertas-stock")]
    public async Task<IActionResult> ObtenerAlertasStock()
    {
        // Usa el stock mínimo configurado globalmente en el Singleton
        var productoBajoStock = await _servicio
            .ObtenerProductosBajoStockAsync(_config.StockMinimoGlobal);

        return Ok(new
        {
            Productos   = productoBajoStock,
            StockMinimo = _config.StockMinimoGlobal,
            Servidor    = _config.IPServidorLocal,
            Moneda      = _config.MonedaLocal
        });
    }

    [HttpPost("imprimir-reporte")]
    public IActionResult ImprimirReporte([FromBody] int idAlmacen)
    {
        // Usa la impresora de reportes configurada en el Singleton
        var impresora = _config.ImpresoraReportes;

        return Ok(new
        {
            Mensaje   = $"Enviando a {impresora.Nombre}",
            Tipo      = impresora.Tipo,
            IdAlmacen = idAlmacen
        });
    }
}