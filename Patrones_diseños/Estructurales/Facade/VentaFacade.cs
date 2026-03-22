
// ─────────────────────────────────────────────────────────────────────────
// VentaFacade actualizado — publica evento en lugar de coordinar directo
// El Mediator reemplaza la coordinación explícita dentro del Facade.
// ─────────────────────────────────────────────────────────────────────────

public class VentaFacadeConMediator
{
    private readonly PedidoDirector    _director;
    private readonly VentaChain        _chain;
    private readonly IVentaRepository  _ventaRepo;
    private readonly ISistemaMediator  _mediator;   // ← reemplaza coordinación manual

    public VentaFacadeConMediator(
        PedidoDirector   director,
        VentaChain       chain,
        IVentaRepository ventaRepo,
        ISistemaMediator mediator)
    {
        _director  = director;
        _chain     = chain;
        _ventaRepo = ventaRepo;
        _mediator  = mediator;
    }

    public async Task<VentaFacadeResponse> ProcesarVentaLocal(
        VentaLocalFacadeRequest req,
        string                  usuario,
        string                  rol)
    {
        // 1. Builder construye el Pedido
        var builder = new PedidoLocalBuilder();
        _director.CambiarBuilder(builder);
        var cliente = new ClienteDto(req.IdCliente, "Cliente",
            req.TipoComprobante, null, null);
        var pedido = _director.ConstruirVentaLocal(
            cliente,
            req.Lineas.Select(l => new ProductoDto(
                l.IdProducto, l.NombreProducto, l.Cantidad, l.PrecioUnitario)).ToList(),
            null, req.TipoComprobante, req.NCF);

        // 2. Chain valida todo
        var ctx = await _chain.Ejecutar(pedido, usuario, rol);

        // 3. Persiste la venta
        var idVenta = await _ventaRepo.GuardarAsync(pedido);

        // 4. Publica el evento — el Mediator coordina inventario + factura + notif
        //    El Facade ya no sabe nada de esos módulos
        await _mediator.Publicar(new VentaRegistradaEvento(
            IdVenta:        idVenta,
            IdCliente:      req.IdCliente,
            NombreCliente:  pedido.NombreCliente,
            Total:          pedido.Total,
            TipoComprobante: req.TipoComprobante,
            NCF:            req.NCF,
            Lineas:         req.Lineas
        ) { Originador = usuario });

        return new VentaFacadeResponse
        {
            Exitoso       = true,
            Mensaje       = "Venta registrada y evento publicado",
            IdTransaccion = idVenta,
            Total         = $"RD${pedido.Total:F2}",
            Advertencias  = ctx.Advertencias
        };
    }
}

/* ─── 15 patrones activos ─────────────────────────────────────────────────
 *  Singleton(1) · Factory Method(2) · Builder(3) · Observer(4)
 *  State(5) · Strategy(6) · Chain(7) · Command(8) · Decorator(9)
 *  Facade(10) · Proxy(11) · Template Method(12) · Composite(13)
 *  Iterator(14) · Mediator(15)
 * ──────────────────────────────────────────────────────────────────────── */