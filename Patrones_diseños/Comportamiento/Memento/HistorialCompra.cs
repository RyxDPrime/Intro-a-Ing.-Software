// HistorialOrdenes.cs — el Caretaker
// Guarda y devuelve Mementos sin poder leer su contenido interno.
// Los campos internos del Memento son "internal" — el Caretaker
// no puede acceder a Estado, Costo, ni ningún dato de negocio.

namespace AguaMinami.Application.Purchasing.Memento;

public class HistorialOrdenes
{
    // Stack por orden: cada orden tiene su propia pila de snapshots
    private readonly Dictionary<int, Stack<OrdenMemento>> _historial = new();
    private const int MAX_SNAPSHOTS_POR_ORDEN = 10;

    // ── Guarda un snapshot de la orden ──
    public void Guardar(int idOrden, OrdenMemento memento)
    {
        if (!_historial.ContainsKey(idOrden))
            _historial[idOrden] = new Stack<OrdenMemento>();

        _historial[idOrden].Push(memento);

        // Limita el historial para no crecer indefinidamente
        while (_historial[idOrden].Count > MAX_SNAPSHOTS_POR_ORDEN)
        {
            var lista = _historial[idOrden].ToList();
            lista.RemoveAt(lista.Count - 1);
            _historial[idOrden] = new Stack<OrdenMemento>(lista);
        }
    }

    // ── Devuelve y elimina el último snapshot (deshacer) ──
    public OrdenMemento? Deshacer(int idOrden)
    {
        if (!_historial.TryGetValue(idOrden, out var pila) || pila.Count == 0)
            return null;

        return pila.Pop();
    }

    // ── Mira el último snapshot sin eliminarlo ──
    public OrdenMemento? UltimoSnapshot(int idOrden)
    {
        if (!_historial.TryGetValue(idOrden, out var pila) || pila.Count == 0)
            return null;

        return pila.Peek();
    }

    // ── Lista todos los snapshots de una orden (metadata pública) ──
    public List<string> ListarSnapshots(int idOrden)
    {
        if (!_historial.TryGetValue(idOrden, out var pila))
            return [];

        // Solo accede a los campos públicos del Memento — no a los internos
        return pila.Select(m => m.ToString()).ToList();
    }

    public bool TieneHistorial(int idOrden) =>
        _historial.TryGetValue(idOrden, out var p) && p.Count > 0;

    public int CantidadSnapshots(int idOrden) =>
        _historial.TryGetValue(idOrden, out var p) ? p.Count : 0;
}
