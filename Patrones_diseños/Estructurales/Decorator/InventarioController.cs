
// ─────────────────────────────────────────────────────────────
// InventarioController — usa IInventarioRepository sin saber
// cuántas capas hay debajo. El Decorator es completamente transparente.
// ─────────────────────────────────────────────────────────────

[ApiController]
[Route("api/inventario")]
public class InventarioController : ControllerBase
{
    private readonly IInventarioRepository _repo;  // recibe el LoggingDecorator

    public InventarioController(IInventarioRepository repo) => _repo = repo;

    // GET api/inventario/stock/3
    // Flujo: Logging → Cache (HIT si ya consultado) → Real (si MISS)
    // El controller nunca sabe si vino de caché o de BD
    [HttpGet("stock/{idProducto}")]
    public async Task<IActionResult> ObtenerStock(int idProducto)
    {
        var stock = await _repo.ObtenerStockActualAsync(idProducto);
        return Ok(new { IdProducto = idProducto, Stock = stock });
    }

    // GET api/inventario/todos/1
    // Flujo: Logging → Cache → Real (primer request) luego Cache (siguientes)
    [HttpGet("todos/{idAlmacen}")]
    public async Task<IActionResult> ObtenerTodos(int idAlmacen)
    {
        var productos = await _repo.ObtenerTodosAsync(idAlmacen);
        return Ok(productos);
    }

    // POST api/inventario/movimiento
    // Flujo: Logging → Cache (invalida) → Validacion → Real
    // Si la validación falla lanza excepción — el controller la atrapa
    [HttpPost("movimiento")]
    public async Task<IActionResult> GuardarMovimiento(
        [FromBody] MovimientoInventario mov)
    {
        try
        {
            mov.Usuario = User.Identity!.Name!;
            var id = await _repo.GuardarMovimientoAsync(mov);
            return Ok(new { IdMovimiento = id, Mensaje = "Movimiento registrado" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Error = ex.Message });
        }
    }
}