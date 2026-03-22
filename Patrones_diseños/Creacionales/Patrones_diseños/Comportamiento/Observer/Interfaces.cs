// Interfaces del patrón Observer para el inventario de Agua Minami
// ─────────────────────────────────────────────────────────────────

namespace AguaMinami.Application.Inventory;

// Evento que viaja del sujeto a todos los observers
// Contiene toda la info que un observer puede necesitar
public record StockBajoEvento(
    int     IdProducto,
    string  NombreProducto,
    int     StockActual,
    int     StockMinimo,
    int     IdAlmacen,
    string  TipoMovimiento,   // "Salida" | "Ajuste"
    string  UsuarioResponsable,
    DateTime FechaEvento
)
{
    // Cuántas unidades faltan para llegar al mínimo
    public int UnidadesFaltantes => StockMinimo - StockActual;
};

// ── El SUJETO: quien puede ser observado ──
public interface IStockObservable
{
    void Suscribir(IStockObserver observer);
    void Desuscribir(IStockObserver observer);
    Task Notificar(StockBajoEvento evento);
}

// ── El OBSERVER: quien reacciona al evento ──
public interface IStockObserver
{
    string Nombre { get; }  // para logs: "AlertaCompras", "Log", "Admin"
    Task OnStockBajo(StockBajoEvento evento);
}