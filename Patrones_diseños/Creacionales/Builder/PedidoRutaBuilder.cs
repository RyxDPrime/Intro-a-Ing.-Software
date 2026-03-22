// PedidoRutaBuilder.cs — construye pedidos de salida en ruta
// Agrega chofer, ayudante, ruta y código de salida
// (campos propios de Despacho y Ruta en tu diagrama de clases)

namespace AguaMinami.Application.Orders;

public class PedidoRutaBuilder : IPedidoBuilder
{
    private Pedido _pedido = new();

    public PedidoRutaBuilder()
    {
        _pedido.TipoVenta = "Ruta";
        _pedido.Fecha     = DateTime.Now;
    }

    // ── Paso exclusivo de ruta: asignar despacho ──
    public PedidoRutaBuilder ConDespacho(
        int    idChofer,
        int    idAyudante,
        int    idRuta,
        string codigoSalida)
    {
        _pedido.IdChofer      = idChofer;
        _pedido.IdAyudante    = idAyudante;
        _pedido.IdRuta        = idRuta;
        _pedido.CodigoSalida  = codigoSalida;
        return this;
    }

    // ── Implementación de la interfaz ──
    public IPedidoBuilder ConCliente(
        int     idCliente,
        string  nombre,
        string  tipoCliente,
        string? rnc          = null,
        string? grupoCliente = null)
    {
        _pedido.IdCliente     = idCliente;
        _pedido.NombreCliente = nombre;
        _pedido.TipoCliente   = tipoCliente;
        _pedido.RncCliente    = rnc;
        _pedido.GrupoCliente  = grupoCliente;
        return this;
    }

    public IPedidoBuilder AgregarProducto(
        int     idProducto,
        string  nombre,
        int     cantidad,
        decimal precioUnit)
    {
        _pedido.Lineas.Add(new LineaPedido
        {
            IdProducto = idProducto,
            Producto   = nombre,
            Cantidad   = cantidad,
            PrecioUnit = precioUnit
        });
        return this;
    }

    public IPedidoBuilder AplicarOferta(
        int    idOferta,
        string descripcion,
        int    idProductoGratis,
        string nombreProductoGratis,
        int    cantidadGratis)
    {
        _pedido.Ofertas.Add(new OfertaAplicada
        {
            IdOferta       = idOferta,
            Descripcion    = descripcion,
            CantidadGratis = cantidadGratis,
            ProductoGratis = nombreProductoGratis
        });
        _pedido.Lineas.Add(new LineaPedido
        {
            IdProducto       = idProductoGratis,
            Producto         = $"{nombreProductoGratis} [GRATIS]",
            Cantidad         = cantidadGratis,
            PrecioUnit       = 0m,
            EsProductoGratis = true
        });
        return this;
    }

    public IPedidoBuilder ConComprobante(string tipo, string ncf)
    {
        _pedido.TipoComprobante = tipo;
        _pedido.NCF             = ncf;
        return this;
    }

    public Pedido Construir()
    {
        if (_pedido.IdChofer is null)
            throw new InvalidOperationException(
                "Pedido de ruta requiere chofer. Llama ConDespacho() primero.");

        if (!_pedido.EsValido())
            throw new InvalidOperationException(
                "Pedido incompleto: verifica cliente y productos.");

        return _pedido;
    }

    public IPedidoBuilder Reiniciar()
    {
        _pedido = new Pedido { TipoVenta = "Ruta", Fecha = DateTime.Now };
        return this;
    }
}