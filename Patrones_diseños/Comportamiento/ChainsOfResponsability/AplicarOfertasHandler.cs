// Eslabón 4: AplicacionOfertasHandler
// Evalúa las ofertas activas y vigentes para el tipo de venta.
// "Si compras X cantidad, llevas Y gratis" — tu documento CU 008.
// Si una oferta aplica, agrega la línea gratis al Pedido (via Builder).
// Es el último eslabón — si pasa, el Pedido está listo para guardarse.

namespace AguaMinami.Application.Sales.Handlers;

public class AplicacionOfertasHandler : VentaHandler
{
    private readonly IOfertaRepository _ofertas;

    public AplicacionOfertasHandler(IOfertaRepository ofertas) =>
        _ofertas = ofertas;

    public override async Task Manejar(VentaContext ctx)
    {
        var hoy         = DateTime.Today;
        var tipoVenta   = ctx.Pedido.TipoVenta;  // "Local" | "Ruta"

        // Carga solo ofertas activas, vigentes y que aplican a este tipo de venta
        var ofertasActivas = await _ofertas
            .ObtenerActivasParaTipoAsync(tipoVenta, hoy);

        foreach (var oferta in ofertasActivas)
        {
            // Busca si el pedido tiene suficiente cantidad del producto requerido
            var lineaCalificante = ctx.Pedido.Lineas
                .Where(l => l.IdProducto == oferta.IdProducto && !l.EsProductoGratis)
                .Sum(l => l.Cantidad);

            if (lineaCalificante < oferta.CantidadMinima) continue;

            // Calcula cuántos productos gratis aplican
            // Ej: compra 20 Funditas con oferta "10+1" → 2 Funditas gratis
            var vecesAplica     = lineaCalificante / oferta.CantidadMinima;
            var cantidadGratis  = vecesAplica * oferta.CantidadGratis;

            // Verifica que haya stock del producto gratis también
            var stockGratis = ctx.StockDisponible.GetValueOrDefault(
                oferta.IdProductoGratis,
                await _ofertas.ObtenerStockProductoAsync(oferta.IdProductoGratis));

            if (stockGratis < cantidadGratis)
            {
                ctx.Advertencias.Add(
                    $"Oferta '{oferta.Descripcion}': stock insuficiente del producto gratis " +
                    $"(disponible: {stockGratis}, requerido: {cantidadGratis}). No aplicada.");
                continue;
            }

            // ✓ Aplica la oferta — agrega línea gratis al Pedido
            ctx.Pedido.Lineas.Add(new LineaPedido
            {
                IdProducto       = oferta.IdProductoGratis,
                Producto         = $"{oferta.NombreProductoGratis} [GRATIS - {oferta.Descripcion}]",
                Cantidad         = cantidadGratis,
                PrecioUnit       = 0m,
                EsProductoGratis = true
            });

            ctx.Pedido.Ofertas.Add(new OfertaAplicada
            {
                IdOferta       = oferta.Id,
                Descripcion    = oferta.Descripcion,
                CantidadGratis = cantidadGratis,
                ProductoGratis = oferta.NombreProductoGratis
            });

            ctx.OfertasAplicadas = true;
        }

        // ✓ Último eslabón — no llama a Continuar()
        // El Pedido ya está validado y con ofertas aplicadas
        await Continuar(ctx);
    }
}