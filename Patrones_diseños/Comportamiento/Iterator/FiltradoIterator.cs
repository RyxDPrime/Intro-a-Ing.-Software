// FiltradoIterator.cs — iterador con filtros de fecha, tipo y usuario
// Solo expone los movimientos que cumplen los criterios del filtro.
// El cliente llama HasNext()/Next() igual que con MovimientoIterator —
// no sabe que internamente hay un salto de elementos filtrados.
//
// InversoIterator — recorre del más reciente al más antiguo.
// Útil para mostrar la pantalla de historial con lo más reciente primero.

namespace AguaMinami.Application.Inventory.Iterators;

public class FiltradoIterator : IMovimientoIterator
{
    private readonly List<MovimientoInventario> _todos;
    private readonly FiltroMovimiento            _filtro;
    private          List<MovimientoInventario> _filtrados = [];
    private int _posicion = -1;

    public FiltradoIterator(
        List<MovimientoInventario> todos,
        FiltroMovimiento           filtro)
    {
        _todos  = todos;
        _filtro = filtro;
        AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        var query = _todos.Where(_filtro.Aplica);

        // Paginación integrada en el iterador
        _filtrados = query
            .OrderByDescending(m => m.Fecha)  // más reciente primero
            .Skip((_filtro.Page - 1) * _filtro.PageSize)
            .Take(_filtro.PageSize)
            .ToList();
    }

    public bool HasNext() => _posicion + 1 < _filtrados.Count;

    public MovimientoInventario Next()
    {
        if (!HasNext())
            throw new InvalidOperationException(
                "No hay más movimientos con los filtros aplicados.");
        return _filtrados[++_posicion];
    }

    public MovimientoInventario? Current =>
        _posicion >= 0 ? _filtrados[_posicion] : null;

    public void Reset()  => _posicion = -1;
    public int  Posicion => _posicion;
    public int  Total    => _filtrados.Count;

    // Cambia la página y reinicia la posición
    public void IrAPagina(int pagina)
    {
        _filtro.Page = pagina;
        AplicarFiltro();
        Reset();
    }
}

// ── InversoIterator — del más reciente al más antiguo ──
public class InversoIterator : IMovimientoIterator
{
    private readonly List<MovimientoInventario> _movimientos;
    private int _posicion;

    public InversoIterator(List<MovimientoInventario> movimientos)
    {
        _movimientos = movimientos.OrderByDescending(m => m.Fecha).ToList();
        _posicion    = -1;
    }

    public bool HasNext() => _posicion + 1 < _movimientos.Count;

    public MovimientoInventario Next()
    {
        if (!HasNext())
            throw new InvalidOperationException("No hay más elementos.");
        return _movimientos[++_posicion];
    }

    public MovimientoInventario? Current =>
        _posicion >= 0 ? _movimientos[_posicion] : null;

    public void Reset()  => _posicion = -1;
    public int  Posicion => _posicion;
    public int  Total    => _movimientos.Count;
}