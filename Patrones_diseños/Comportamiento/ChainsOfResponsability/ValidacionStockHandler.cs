
// Eslabón 2: ValidacionStockHandler
// Verifica que haya suficiente stock para cada línea del Pedido.
// "Las salidas no pueden generar stock negativo" — tu documento.
// Usa el StockAlmacen del patrón Observer para consultar stock real.

using AguaMinami.Application.Inventory;

namespace AguaMinami.Application.Sales.Handlers;

public class ValidacionStockHandler : VentaHandler
{
    private readonly IStockRepository _stockRepo;

    public ValidacionStockHandler(IStockRepository stockRepo) =>
        _stockRepo = stockRepo;

    public override async Task Manejar(VentaContext ctx)
    {
        var lineasConProblema = new List<object>();

        foreach (var linea in ctx.Pedido.Lineas.Where(l => !l.EsProductoGratis))
        {
            var stockActual = await _stockRepo
                .ObtenerStockActualAsync(linea.IdProducto);

            // Guarda el stock en el contexto para que otros eslabones lo usen
            ctx.StockDisponible[linea.IdProducto] = stockActual;

            if (stockActual < linea.Cantidad)
            {
                lineasConProblema.Add(new
                {
                    Producto       = linea.Producto,
                    Solicitado     = linea.Cantidad,
                    Disponible     = stockActual,
                    Faltante       = linea.Cantidad - stockActual
                });
            }
        }

        if (lineasConProblema.Count > 0)
            throw new ValidacionVentaException(
                codigo:  "STOCK_INSUFICIENTE",
                eslabón: "ValidacionStock",
                mensaje: $"Stock insuficiente para {lineasConProblema.Count} producto(s).",
                detalle: lineasConProblema
            );

        // ✓ Todo el stock disponible — continúa
        await Continuar(ctx);
    }
}