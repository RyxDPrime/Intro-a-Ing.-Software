// Pedido.cs — el objeto complejo que el Builder construye paso a paso
// Combina datos de: Cliente, Productos, Ofertas, Venta (tipo local o ruta)
// Una vez construido, se pasa directamente a DocumentoFactory (patrón anterior)

namespace AguaMinami.Application.Orders;

public class Pedido
{
    // ── Identificación ──
    public int      Id          { get; internal set; }
    public DateTime Fecha       { get; internal set; }
    public string   TipoVenta   { get; internal set; } = ""; // "Local" | "Ruta"

    // ── Cliente (de tu tabla Cliente en la BD) ──
    public int    IdCliente      { get; internal set; }
    public string NombreCliente  { get; internal set; } = "";
    public string? RncCliente    { get; internal set; }
    public string  TipoCliente   { get; internal set; } = ""; // "Consumidor Final" | "Empresa"
    public string? GrupoCliente  { get; internal set; }        // "Supermercado" | "Gobierno"...

    // ── Líneas de productos ──
    public List<LineaPedido> Lineas { get; internal set; } = [];

    // ── Ofertas aplicadas (de tu tabla Oferta en la BD) ──
    public List<OfertaAplicada> Ofertas { get; internal set; } = [];

    // ── Datos de ruta (solo si TipoVenta == "Ruta") ──
    public int?    IdChofer     { get; internal set; }
    public int?    IdAyudante   { get; internal set; }
    public int?    IdRuta       { get; internal set; }
    public string? CodigoSalida { get; internal set; }

    // ── Totales calculados automáticamente ──
    public decimal Subtotal => Lineas.Sum(l => l.Total);
    public decimal ITBIS    => TipoCliente == "Persona Juridica" ? Subtotal * 0.18m : 0m;
    public decimal Total    => Subtotal + ITBIS;

    // ── Comprobante fiscal ──
    public string TipoComprobante { get; internal set; } = "Consumidor Final";
    public string NCF             { get; internal set; } = "";

    // Valida que el pedido esté completo antes de emitirlo
    public bool EsValido() =>
        IdCliente > 0 &&
        Lineas.Count > 0 &&
        Lineas.All(l => l.Cantidad > 0 && l.PrecioUnit > 0);
}

public class LineaPedido
{
    public int     IdProducto   { get; set; }
    public string  Producto     { get; set; } = "";
    public int     Cantidad     { get; set; }
    public decimal PrecioUnit   { get; set; }
    public decimal Total        => Cantidad * PrecioUnit;
    public bool    EsProductoGratis { get; set; } // true si vino de una oferta
}

public class OfertaAplicada
{
    public int    IdOferta      { get; set; }
    public string Descripcion   { get; set; } = "";  // "Promo Funditas 10+1"
    public int    CantidadGratis { get; set; }
    public string ProductoGratis { get; set; } = "";
}