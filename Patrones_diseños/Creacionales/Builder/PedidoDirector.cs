// PedidoDirector.cs — orquesta los pasos en el orden correcto
// El Director conoce el ORDEN de los pasos, no los detalles.
// Recibe el builder concreto desde fuera → Open/Closed cumplido.

namespace AguaMinami.Application.Orders;

public class PedidoDirector
{
    private IPedidoBuilder _builder;

    public PedidoDirector(IPedidoBuilder builder)
    {
        _builder = builder;
    }

    // Cambia el builder en caliente (mismo director, distinto tipo de venta)
    public void CambiarBuilder(IPedidoBuilder builder) => _builder = builder;

    // ── Plantilla: venta local estándar ──
    // Orden: cliente → productos → ofertas → comprobante → construir
    public Pedido ConstruirVentaLocal(
        ClienteDto           cliente,
        List<ProductoDto>   productos,
        List<OfertaDto>?    ofertas,
        string               tipoComprobante,
        string               ncf)
    {
        _builder.Reiniciar();

        _builder.ConCliente(
            cliente.Id,
            cliente.Nombre,
            cliente.TipoCliente,
            cliente.Rnc,
            cliente.Grupo);

        foreach (var p in productos)
            _builder.AgregarProducto(p.Id, p.Nombre, p.Cantidad, p.PrecioUnit);

        if (ofertas is not null)
            foreach (var o in ofertas)
                _builder.AplicarOferta(
                    o.Id, o.Descripcion,
                    o.IdProductoGratis, o.NombreProductoGratis,
                    o.CantidadGratis);

        _builder.ConComprobante(tipoComprobante, ncf);

        return _builder.Construir();
    }

    // ── Plantilla: salida en ruta ──
    // Agrega paso de despacho (chofer, ayudante, ruta)
    public Pedido ConstruirSalidaRuta(
        ClienteDto           cliente,
        List<ProductoDto>   productos,
        DespachoDto          despacho,
        List<OfertaDto>?    ofertas = null)
    {
        if (_builder is not PedidoRutaBuilder rutaBuilder)
            throw new InvalidOperationException(
                "Para salidas en ruta usa PedidoRutaBuilder.");

        rutaBuilder.Reiniciar();

        // Paso exclusivo de ruta: despacho primero
        rutaBuilder.ConDespacho(
            despacho.IdChofer,
            despacho.IdAyudante,
            despacho.IdRuta,
            despacho.CodigoSalida);

        rutaBuilder.ConCliente(
            cliente.Id,
            cliente.Nombre,
            cliente.TipoCliente);

        foreach (var p in productos)
            rutaBuilder.AgregarProducto(p.Id, p.Nombre, p.Cantidad, p.PrecioUnit);

        if (ofertas is not null)
            foreach (var o in ofertas)
                rutaBuilder.AplicarOferta(
                    o.Id, o.Descripcion,
                    o.IdProductoGratis, o.NombreProductoGratis,
                    o.CantidadGratis);

        // Ruta no genera comprobante fiscal en el momento de la salida
        rutaBuilder.ConComprobante("Consumidor Final", "");

        return rutaBuilder.Construir();
    }
}

// ── DTOs de entrada ──
public record ClienteDto(int Id, string Nombre, string TipoCliente, string? Rnc, string? Grupo);
public record ProductoDto(int Id, string Nombre, int Cantidad, decimal PrecioUnit);
public record OfertaDto(int Id, string Descripcion, int IdProductoGratis, string NombreProductoGratis, int CantidadGratis);
public record DespachoDto(int IdChofer, int IdAyudante, int IdRuta, string CodigoSalida);