// DTOs del Facade — lo único que el frontend necesita conocer
// Entrada simple, salida completa con todo lo que necesita la UI.
// El frontend React no ve Pedido, VentaContext, ResultadoCommand ni nada interno.

namespace AguaMinami.Application.Sales;

// ── Request de venta local (lo que envía el frontend) ──
public class VentaLocalFacadeRequest
{
    public int     IdCliente        { get; set; }
    public string  TipoComprobante  { get; set; } = "Consumidor Final";
    public string  NCF              { get; set; } = "";
    public List<LineaVentaDto> Lineas { get; set; } = [];
}

public record LineaVentaDto(
    int     IdProducto,
    string  NombreProducto,
    int     Cantidad,
    decimal PrecioUnitario
);

// ── Request de salida en ruta ──
public class SalidaRutaFacadeRequest
{
    public int  IdChofer    { get; set; }
    public int  IdAyudante  { get; set; }
    public int  IdRuta      { get; set; }
    public List<LineaVentaDto> Productos { get; set; } = [];
}

// ── Request de entrada de retorno de ruta ──
public record EntradaRetornoRequest(
    string              CodigoSalida,
    List<LineaVentaDto> ProductosRetornados
);

// ── Respuesta unificada del Facade ──
public class VentaFacadeResponse
{
    public bool   Exitoso          { get; set; }
    public string Mensaje          { get; set; } = "";
    public int?   IdTransaccion    { get; set; }

    // Totales
    public string Subtotal         { get; set; } = "";
    public string ITBIS            { get; set; } = "";
    public string Total            { get; set; } = "";

    // Ofertas aplicadas automáticamente
    public List<string> OfertasAplicadas { get; set; } = [];

    // Alertas de stock bajo generadas por el Observer
    public List<string> AlertasStock    { get; set; } = [];

    // Advertencias no bloqueantes (precios que difieren, etc.)
    public List<string> Advertencias     { get; set; } = [];

    // Documento emitido
    public string? TipoDocumento    { get; set; }
    public string? VistaPrevia      { get; set; }
}