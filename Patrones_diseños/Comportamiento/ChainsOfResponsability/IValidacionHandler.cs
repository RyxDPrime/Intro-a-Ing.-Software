// Base abstracta del patrón Chain of Responsibility
// Cada eslabón conoce solo al siguiente — no sabe cuántos hay.
// El Pedido (del Builder) viaja por toda la cadena.

namespace AguaMinami.Application.Sales.Handlers;

// ── Contexto que viaja por toda la cadena ──
public class VentaContext
{
    public Pedido      Pedido   { get; }
    public string      Usuario  { get; }
    public string      Rol      { get; }

    // Se va enriqueciendo al pasar por cada eslabón
    public Dictionary<int, int> StockDisponible { get; } = [];
    public List<string>        Advertencias    { get; } = [];
    public bool                 OfertasAplicadas { get; set; }

    public VentaContext(Pedido pedido, string usuario, string rol)
    {
        Pedido  = pedido;
        Usuario = usuario;
        Rol     = rol;
    }
}

// ── Clase base abstracta para todos los eslabones ──
public abstract class VentaHandler
{
    private VentaHandler? _siguiente;

    // Encadena el siguiente eslabón (fluent API)
    public VentaHandler SetSiguiente(VentaHandler siguiente)
    {
        _siguiente = siguiente;
        return siguiente;  // devuelve el siguiente para encadenar con fluent
    }

    // Cada subclase implementa su validación aquí
    public abstract Task Manejar(VentaContext ctx);

    // Pasa al siguiente eslabón (o termina si no hay más)
    protected async Task Continuar(VentaContext ctx)
    {
        if (_siguiente is not null)
            await _siguiente.Manejar(ctx);
    }
}

// ── Excepción específica con código de error para el frontend ──
public class ValidacionVentaException : Exception
{
    public string Codigo     { get; }   // "AUTH_FAIL"|"STOCK_INSUF"|"PRECIO_INV"|"OFERTA_EXP"
    public string Eslabón    { get; }   // qué handler falló
    public object? Detalle   { get; }   // datos adicionales (qué producto, cuánto falta)

    public ValidacionVentaException(
        string  codigo,
        string  eslabón,
        string  mensaje,
        object? detalle = null)
        : base(mensaje)
    {
        Codigo  = codigo;
        Eslabón = eslabón;
        Detalle = detalle;
    }
}