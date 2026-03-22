// IPedidoBuilder.cs — contrato del Builder
// Cada método devuelve "this" para encadenamiento fluido:
//   builder.ConCliente(c).AgregarProducto(p1).AgregarProducto(p2).Construir()

namespace AguaMinami.Application.Orders;

public interface IPedidoBuilder
{
    // ── Paso 1: identificar el cliente ──
    IPedidoBuilder ConCliente(
        int    idCliente,
        string nombre,
        string tipoCliente,
        string? rnc          = null,
        string? grupoCliente = null);

    // ── Paso 2: agregar líneas de producto (N veces) ──
    IPedidoBuilder AgregarProducto(
        int     idProducto,
        string  nombre,
        int     cantidad,
        decimal precioUnit);

    // ── Paso 3: aplicar ofertas activas (opcional, N veces) ──
    // Ejemplo: compra 10 Funditas → agrega 1 Fundita gratis
    IPedidoBuilder AplicarOferta(
        int    idOferta,
        string descripcion,
        int    idProductoGratis,
        string nombreProductoGratis,
        int    cantidadGratis);

    // ── Paso 4: asignar el tipo de comprobante fiscal ──
    IPedidoBuilder ConComprobante(string tipo, string ncf);

    // ── Resultado: devuelve el Pedido completamente construido ──
    Pedido Construir();

    // ── Reinicia el builder para reutilizarlo en el mismo request ──
    IPedidoBuilder Reiniciar();
}