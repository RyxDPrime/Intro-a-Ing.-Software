// Eslabón 3: ValidacionPrecioHandler
// Verifica que ningún precio sea cero o negativo (excepto productos gratis).
// Valida también que el total del pedido sea consistente.
// Agrega advertencias si detecta precios inusualmente bajos (posible error).

using AguaMinami.Infrastructure.Config;

namespace AguaMinami.Application.Sales.Handlers;

public class ValidacionPrecioHandler : VentaHandler
{
    private readonly IProductoRepository _productos;
    private readonly AppConfiguracion     _config;

    public ValidacionPrecioHandler(
        IProductoRepository productos,
        AppConfiguracion     config)
    {
        _productos = productos;
        _config    = config;
    }

    public override async Task Manejar(VentaContext ctx)
    {
        foreach (var linea in ctx.Pedido.Lineas.Where(l => !l.EsProductoGratis))
        {
            // Precio cero o negativo en línea no gratis → error
            if (linea.PrecioUnit <= 0m)
                throw new ValidacionVentaException(
                    codigo:  "PRECIO_INVALIDO",
                    eslabón: "ValidacionPrecio",
                    mensaje: $"Precio inválido para '{linea.Producto}': RD${linea.PrecioUnit:F2}",
                    detalle: new { linea.Producto, Precio = linea.PrecioUnit }
                );

            // Compara contra el precio registrado en BD — advierte si difiere >10%
            var precioOficial = await _productos
                .ObtenerPrecioActualAsync(linea.IdProducto);

            if (precioOficial > 0)
            {
                var diferencia = Math.Abs(linea.PrecioUnit - precioOficial) / precioOficial;
                if (diferencia > 0.10m)
                    ctx.Advertencias.Add(
                        $"Precio de '{linea.Producto}' difiere >10% del oficial " +
                        $"(RD${linea.PrecioUnit:F2} vs RD${precioOficial:F2})");
            }
        }

        // Total del pedido debe ser mayor a cero
        if (ctx.Pedido.Total <= 0m && ctx.Pedido.Lineas.Any(l => !l.EsProductoGratis))
            throw new ValidacionVentaException(
                codigo:  "TOTAL_CERO",
                eslabón: "ValidacionPrecio",
                mensaje: "El total del pedido no puede ser RD$0.00"
            );

        // ✓ Precios válidos — continúa
        await Continuar(ctx);
    }
}