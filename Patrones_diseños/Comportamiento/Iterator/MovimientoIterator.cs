// MovimientoIterator.cs — iterador cronológico simple
// Recorre la lista de movimientos de principio a fin.
// Implementa IEnumerator<T> para compatibilidad con foreach de C#.

namespace AguaMinami.Application.Inventory.Iterators;

public class MovimientoIterator : IMovimientoIterator
{
    private readonly List<MovimientoInventario> _movimientos;
    private int _posicion = -1;

    public MovimientoIterator(List<MovimientoInventario> movimientos)
    {
        // Ordena de más antiguo a más reciente por defecto
        _movimientos = movimientos
            .OrderBy(m => m.Fecha)
            .ToList();
    }

    public bool HasNext() =>
        _posicion + 1 < _movimientos.Count;

    public MovimientoInventario Next()
    {
        if (!HasNext())
            throw new InvalidOperationException(
                "No hay más movimientos en el historial.");

        _posicion++;
        return _movimientos[_posicion];
    }

    public MovimientoInventario? Current =>
        _posicion >= 0 && _posicion < _movimientos.Count
            ? _movimientos[_posicion]
            : null;

    public void Reset()  => _posicion = -1;
    public int  Posicion => _posicion;
    public int  Total    => _movimientos.Count;

    // ── Calcular métricas mientras se recorre ──
    // Suma total de entradas mientras itera — sin cargar todo en memoria
    public ResumenHistorial Resumir()
    {
        Reset();
        var resumen = new ResumenHistorial();

        while (HasNext())
        {
            var mov = Next();
            switch (mov.Tipo)
            {
                case "Entrada": resumen.TotalEntradas += mov.Cantidad; break;
                case "Salida":  resumen.TotalSalidas  += mov.Cantidad; break;
                case "Ajuste":  resumen.TotalAjustes++;              break;
            }
            resumen.Operaciones++;
        }

        Reset();   // deja el iterador listo para volver a usarse
        return resumen;
    }
}

public class ResumenHistorial
{
    public int Operaciones   { get; set; }
    public int TotalEntradas { get; set; }
    public int TotalSalidas  { get; set; }
    public int TotalAjustes  { get; set; }
    public int NetoMovimiento => TotalEntradas - TotalSalidas;
}