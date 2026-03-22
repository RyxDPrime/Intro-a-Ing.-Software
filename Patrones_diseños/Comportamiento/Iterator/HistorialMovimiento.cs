// HistorialMovimientos.cs — la COLECCIÓN que crea los iteradores
// Carga los movimientos de la BD y expone distintos tipos de recorrido.
// El cliente nunca toca la lista interna — solo pide iteradores.

namespace AguaMinami.Application.Inventory.Iterators;

public class HistorialMovimientos : IHistorialCollection
{
    private readonly List<MovimientoInventario> _movimientos;

    public int      Total      => _movimientos.Count;
    public DateTime PrimerFecha => _movimientos.Min(m => m.Fecha);
    public DateTime UltimaFecha => _movimientos.Max(m => m.Fecha);

    private HistorialMovimientos(
        List<MovimientoInventario> movimientos)
    {
        _movimientos = movimientos;
    }

    // ── Factory method estático para cargar desde la BD ──
    public static async Task<HistorialMovimientos> CargarAsync(
        IMovimientoRepository repo,
        int?      idProducto = null,
        DateTime? desde      = null)
    {
        var movimientos = await repo.ObtenerHistorialAsync(idProducto, desde);
        return new HistorialMovimientos(movimientos);
    }

    // ── Crea iterador cronológico (antiguo → reciente) ──
    public IMovimientoIterator CrearIterador() =>
        new MovimientoIterator(_movimientos);

    // ── Crea iterador con filtros de fecha, tipo, producto y usuario ──
    public IMovimientoIterator CrearIteradorFiltrado(FiltroMovimiento filtro) =>
        new FiltradoIterator(_movimientos, filtro);

    // ── Crea iterador inverso (reciente → antiguo) ──
    public IMovimientoIterator CrearIteradorInverso() =>
        new InversoIterator(_movimientos);

    // ── Compatibilidad con foreach de C# via IEnumerable ──
    public IEnumerable<MovimientoInventario> ComoEnumerable()
    {
        var it = CrearIterador();
        while (it.HasNext())
            yield return it.Next();
    }
}