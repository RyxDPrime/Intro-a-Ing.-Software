// ---- Integración con el Builder (patrón 3) y la Chain (patrón 7) ----

public class PedidoLocalBuilder : IPedidoBuilder
{
    private readonly ProductoFlyweightFactory _flyweightFactory;
    private readonly Pedido _pedido = new();

    public PedidoLocalBuilder(ProductoFlyweightFactory flyweightFactory)
        => _flyweightFactory = flyweightFactory;

    public async Task AgregarLineaAsync(int productoId, decimal cantidad,
                                        decimal precioNegociado = 0,
                                        decimal descuentoPct    = 0)
    {
        // Obtiene el flyweight (del cache si ya existe, de BD si no)
        var flyweight = await _flyweightFactory.ObtenerAsync(productoId);

        _pedido.Lineas.Add(new LineaPedido(flyweight, cantidad,
                                           precioNegociado, descuentoPct));
    }

    public Pedido Build() => _pedido;
}

// ── Controller ──────────────────────────────────────────────────────────
[ApiController]
[Route("api/ventas")]
public class VentasController : ControllerBase
{
    private readonly VentaFacade _facade;                 // patrón 10
    private readonly ProductoFlyweightFactory _fwFactory;

    // GET /api/ventas/cache — diagnóstico para el administrador
    [HttpGet("cache")]
    [Authorize(Roles = "Administrador")]
    public IActionResult EstadoCache()
        => Ok(_fwFactory.ObtenerEstado());

    // DELETE /api/ventas/cache/{productoId} — invalida cuando se edita un producto
    [HttpDelete("cache/{productoId:int}")]
    [Authorize(Roles = "Administrador")]
    public IActionResult InvalidarCache(int productoId)
    {
        _fwFactory.Invalidar(productoId);
        return Ok($"Flyweight del producto {productoId} invalidado.");
    }
}
