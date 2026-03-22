// PedidoLocalBuilder.cs — construye pedidos de venta en el local
// Maneja la lógica de ITBIS, validaciones de stock local
// y el formato de NCF para consumidor final o empresa

namespace AguaMinami.Application.Orders;

public class PedidoLocalBuilder : IPedidoBuilder
{
    private Pedido _pedido = new();

    public PedidoLocalBuilder()
    {
        _pedido.TipoVenta = "Local";
        _pedido.Fecha     = DateTime.Now;
    }

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
        // Registra la oferta aplicada para trazabilidad en factura
        _pedido.Ofertas.Add(new OfertaAplicada
        {
            IdOferta       = idOferta,
            Descripcion    = descripcion,
            CantidadGratis = cantidadGratis,
            ProductoGratis = nombreProductoGratis
        });

        // Agrega la línea del producto gratis (precio = 0)
        _pedido.Lineas.Add(new LineaPedido
        {
            IdProducto       = idProductoGratis,
            Producto         = $"{nombreProductoGratis} [GRATIS - {descripcion}]",
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
        if (!_pedido.EsValido())
            throw new InvalidOperationException(
                "Pedido incompleto: verifica cliente y productos.");

        return _pedido;
    }

    public IPedidoBuilder Reiniciar()
    {
        _pedido = new Pedido { TipoVenta = "Local", Fecha = DateTime.Now };
        return this;
    }
}