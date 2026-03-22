
using AguaMinami.Application.Sales.Handlers;
using AguaMinami.Application.Orders;

namespace AguaMinami.Application.Sales;

// ── VentaChain: ensambla los 4 eslabones en orden ──
public class VentaChain
{
    private readonly VentaHandler _cabeza;

    public VentaChain(
        AutenticacionHandler    auth,
        ValidacionStockHandler  stock,
        ValidacionPrecioHandler precio,
        AplicacionOfertasHandler ofertas)
    {
        // Encadenamiento fluido: auth → stock → precio → ofertas
        auth
            .SetSiguiente(stock)
            .SetSiguiente(precio)
            .SetSiguiente(ofertas);

        _cabeza = auth;    // el primer eslabón es el punto de entrada
    }

    // Ejecuta la cadena completa sobre el contexto
    public async Task<VentaContext> Ejecutar(
        Pedido pedido, string usuario, string rol)
    {
        var ctx = new VentaContext(pedido, usuario, rol);
        await _cabeza.Manejar(ctx);
        return ctx;   // contexto enriquecido con stock, advertencias, ofertas
    }
}