[ApiController]
[Route("api/ventas")]
public class VentasController : ControllerBase
{
    private readonly VentaChain       _chain;
    private readonly PedidoDirector   _director;
    private readonly StockAlmacen     _almacen;
    private readonly DocumentoCreator _docCreator;
    private readonly IVentaRepository _ventaRepo;

    public VentasController(
        VentaChain       chain,
        PedidoDirector   director,
        StockAlmacen     almacen,
        DocumentoCreator docCreator,
        IVentaRepository ventaRepo)
    {
        _chain      = chain;
        _director   = director;
        _almacen    = almacen;
        _docCreator = docCreator;
        _ventaRepo  = ventaRepo;
    }

    // POST api/ventas/local — flujo completo con todos los patrones
    [HttpPost("local")]
    public async Task<IActionResult> RegistrarVentaLocal(
        [FromBody] VentaLocalRequest req)
    {
        try
        {
            // ── 1. BUILDER: construye el Pedido complejo ──
            var builder = new PedidoLocalBuilder();
            _director.CambiarBuilder(builder);
            var pedido = _director.ConstruirVentaLocal(
                req.Cliente, req.Productos,
                ofertas: null,           // las ofertas las aplica la chain
                req.TipoComprobante, req.NCF);

            // ── 2. CHAIN: valida y enriquece el Pedido ──
            //    auth → stock → precio → ofertas (en orden)
            var usuario = User.Identity!.Name!;
            var rol     = User.FindFirst("role")?.Value ?? "";

            var ctx = await _chain.Ejecutar(pedido, usuario, rol);

            // ── 3. OBSERVER: descuenta stock por cada línea vendida ──
            //    Si stock baja del mínimo → AlertaComprasObserver crea OrdenCompra
            foreach (var linea in pedido.Lineas)
            {
                await _almacen.RegistrarMovimiento(
                    idProducto:     linea.IdProducto,
                    nombreProducto: linea.Producto,
                    cantidad:       linea.Cantidad,
                    tipo:           "Salida",
                    motivo:         $"Venta local - {pedido.NombreCliente}",
                    usuario:        usuario);
            }

            // ── 4. Persiste la venta en BD ──
            var idVenta = await _ventaRepo.GuardarAsync(pedido);

            // ── 5. FACTORY METHOD: emite el documento correcto ──
            //    Singleton ya está dentro del DocumentoCreator
            var datosFactura = pedido.ToDatosFactura(idVenta);
            _docCreator.EmitirDocumento(pedido.TipoComprobante, datosFactura);

            return Ok(new
            {
                IdVenta          = idVenta,
                Total            = $"RD${pedido.Total:F2}",
                OfertasAplicadas = ctx.OfertasAplicadas,
                Ofertas          = pedido.Ofertas.Select(o => o.Descripcion),
                Advertencias     = ctx.Advertencias,
                Comprobante      = pedido.TipoComprobante
            });
        }
        catch (ValidacionVentaException ex)
        {
            // Error controlado de la cadena — devuelve 400 con detalle
            return BadRequest(new
            {
                Error    = ex.Message,
                Codigo   = ex.Codigo,
                Eslabón  = ex.Eslabón,
                Detalle  = ex.Detalle
            });
        }
    }
}

// ── Registro en Program.cs ──
// builder.Services.AddScoped<AutenticacionHandler>();
// builder.Services.AddScoped<ValidacionStockHandler>();
// builder.Services.AddScoped<ValidacionPrecioHandler>();
// builder.Services.AddScoped<AplicacionOfertasHandler>();
// builder.Services.AddScoped<VentaChain>();