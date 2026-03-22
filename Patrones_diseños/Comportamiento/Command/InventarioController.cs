// InventarioController.cs — Command + Observer + Chain + Singleton
// Los tres tipos de movimiento del CU 001 "Gestionar Inventario"
// se procesan como comandos encapsulados con soporte de deshacer.

[ApiController]
[Route("api/inventario")]
[Authorize]
public class InventarioController : ControllerBase
{
    private readonly InventarioInvoker    _invoker;
    private readonly StockAlmacen         _almacen;
    private readonly IMovimientoRepository  _movRepo;
    private readonly AppConfiguracion      _config;

    public InventarioController(
        InventarioInvoker    invoker,
        StockAlmacen         almacen,
        IMovimientoRepository  movRepo,
        AppConfiguracion      config)
    {
        _invoker = invoker;
        _almacen = almacen;
        _movRepo = movRepo;
        _config  = config;
    }

    // POST api/inventario/movimiento
    // Body: { "tipo": "Entrada|Salida|Ajuste", "idProducto": 3, "cantidad": 365, ... }
    // Crea el comando correcto y lo ejecuta via Invoker
    [HttpPost("movimiento")]
    public async Task<IActionResult> RegistrarMovimiento(
        [FromBody] MovimientoRequest req)
    {
        try
        {
            var usuario = User.Identity!.Name!;
            var datos   = new DatosMovimiento(
                req.IdProducto, req.NombreProducto,
                req.Cantidad, req.Motivo, usuario,
                1, req.StockMinimoEspecifico);

            // Fábrica de comandos según tipo (podría usarse Factory Method aquí)
            IInventarioCommand comando = req.Tipo switch
            {
                "Entrada" => new EntradaInventarioCommand(_almacen, datos, _movRepo),
                "Salida"  => new SalidaInventarioCommand(_almacen, datos, _movRepo),
                "Ajuste"  => new AjusteInventarioCommand(_almacen, datos, _movRepo),
                _ => throw new ArgumentException($"Tipo inválido: {req.Tipo}")
            };

            var resultado = await _invoker.Ejecutar(comando);

            return Ok(new
            {
                resultado.Mensaje,
                resultado.StockAnterior,
                resultado.StockNuevo,
                AlertaStock      = resultado.AlertaStock,
                StockMinimo      = _config.StockMinimoGlobal,
                PuedeDeshacerse  = _invoker.PuedeDeshacerse,
                TotalEnHistorial = _invoker.TotalEjecutados
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // POST api/inventario/deshacer
    // Revierte el último movimiento registrado
    [HttpPost("deshacer")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Deshacer()
    {
        try
        {
            var resultado = await _invoker.Deshacer();
            return Ok(new
            {
                resultado.Mensaje,
                resultado.StockNuevo,
                PuedeSeguirDeshaciendo = _invoker.PuedeDeshacerse
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // POST api/inventario/salida-ruta/batch
    // Encola y ejecuta múltiples salidas de una vez (salida en ruta)
    // Integra Command (batch) + Builder (Pedido de ruta) + Observer
    [HttpPost("salida-ruta/batch")]
    [Authorize(Roles = "Administrador,Asistente")]
    public async Task<IActionResult> SalidaRutaBatch(
        [FromBody] List<MovimientoRequest> movimientos)
    {
        var usuario = User.Identity!.Name!;

        // Encola todos los comandos de salida
        foreach (var mov in movimientos)
        {
            var datos   = new DatosMovimiento(
                mov.IdProducto, mov.NombreProducto,
                mov.Cantidad, mov.Motivo, usuario, 1);

            _invoker.Encolar(new SalidaInventarioCommand(_almacen, datos, _movRepo));
        }

        // Ejecuta toda la cola — si falla uno, se detiene
        var resultados = await _invoker.EjecutarCola();

        var exitosos = resultados.Count(r => r.Exitoso);
        var fallidos = resultados.Count(r => !r.Exitoso);

        return Ok(new
        {
            Total    = resultados.Count,
            Exitosos = exitosos,
            Fallidos = fallidos,
            Detalle  = resultados.Select(r => new
            {
                r.Mensaje, r.StockNuevo, r.AlertaStock
            })
        });
    }

    // GET api/inventario/historial
    // Devuelve el historial de comandos del session actual
    [HttpGet("historial")]
    public IActionResult ObtenerHistorial() =>
        Ok(new
        {
            Historial        = _invoker.ObtenerHistorial(),
            PuedeDeshacerse  = _invoker.PuedeDeshacerse,
            TotalEjecutados  = _invoker.TotalEjecutados
        });
}

public record MovimientoRequest(
    int    IdProducto,
    string NombreProducto,
    int    Cantidad,
    string Tipo,
    string Motivo,
    int?   StockMinimoEspecifico = null
);

// ── Registro en Program.cs ──
// builder.Services.AddScoped<InventarioInvoker>();
// (Scoped: cada request tiene su propio invoker con su propia pila)