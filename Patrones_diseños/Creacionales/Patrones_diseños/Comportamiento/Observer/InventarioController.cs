// InventarioController.cs — Observer + Builder + Factory + Singleton juntos
// Registra una salida de inventario:
//   1. Builder arma el pedido
//   2. StockAlmacen (Observer) descuenta y notifica si hay stock bajo
//   3. Si la venta viene con factura → Factory emite el documento

[ApiController]
[Route("api/[controller]")]
public class InventarioController : ControllerBase
{
    private readonly StockAlmacen     _almacen;
    private readonly AppConfiguracion _config;

    public InventarioController(
        StockAlmacen     almacen,
        AppConfiguracion config)
    {
        _almacen = almacen;
        _config  = config;
    }

    // POST api/inventario/movimiento
    // Registra entrada, salida o ajuste — dispara observers si baja del mínimo
    [HttpPost("movimiento")]
    public async Task<IActionResult> RegistrarMovimiento(
        [FromBody] MovimientoRequest req)
    {
        try
        {
            var resultado = await _almacen.RegistrarMovimiento(
                idProducto:    req.IdProducto,
                nombreProducto: req.NombreProducto,
                cantidad:      req.Cantidad,
                tipo:          req.Tipo,
                motivo:        req.Motivo,
                usuario:       User.Identity?.Name ?? "Sistema",
                stockMinimoEspecifico: req.StockMinimoEspecifico
            );

            return Ok(new
            {
                StockAnterior  = resultado.StockAnterior,
                StockNuevo     = resultado.StockNuevo,
                AlertaGenerada = resultado.AlertaGenerada,
                Mensaje = resultado.AlertaGenerada
                    ? $"Movimiento registrado. ALERTA: stock bajo mínimo ({_config.StockMinimoGlobal})"
                    : "Movimiento registrado correctamente"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // GET api/inventario/alertas
    // Devuelve los productos actualmente bajo el stock mínimo
    [HttpGet("alertas")]
    public async Task<IActionResult> ObtenerAlertas(
        [FromServices] INotificacionRepository notifRepo)
    {
        var alertas = await notifRepo
            .ObtenerNoLeidasAsync("ALERTA_STOCK");

        return Ok(new
        {
            Total      = alertas.Count,
            StockMinimo = _config.StockMinimoGlobal,
            Servidor   = _config.IPServidorLocal,
            Alertas    = alertas
        });
    }

    // POST api/inventario/desuscribir-observer
    // Permite desactivar un observer en caliente (sin reiniciar)
    // Útil si temporalmente no se quieren generar órdenes automáticas
    [HttpPost("desuscribir-observer/{nombre}")]
    [Authorize(Roles = "Administrador")]
    public IActionResult DesuscribirObserver(
        string nombre,
        [FromServices] IEnumerable<IStockObserver> todosLosObservers)
    {
        var observer = todosLosObservers
            .FirstOrDefault(o => o.Nombre == nombre);

        if (observer is null)
            return NotFound($"Observer '{nombre}' no encontrado");

        _almacen.Desuscribir(observer);
        return Ok($"Observer '{nombre}' desactivado");
    }
}

public record MovimientoRequest(
    int     IdProducto,
    string  NombreProducto,
    int     Cantidad,
    string  Tipo,               // "Entrada" | "Salida" | "Ajuste"
    string  Motivo,
    int?    StockMinimoEspecifico = null
);