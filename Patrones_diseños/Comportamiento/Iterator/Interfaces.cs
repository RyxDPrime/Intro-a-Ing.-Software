// Interfaces del patrón Iterator para el historial de Agua Minami.
// El cliente recorre movimientos sin saber si vienen de un array,
// una lista paginada o un cursor de BD.

namespace AguaMinami.Application.Inventory.Iterators;

// ── El ITERADOR: recorre la colección ──
public interface IMovimientoIterator
{
    // ¿Hay más elementos?
    bool HasNext();

    // Devuelve el elemento actual y avanza al siguiente
    MovimientoInventario Next();

    // Elemento actual sin avanzar
    MovimientoInventario? Current { get; }

    // Reinicia al primer elemento
    void Reset();

    // Posición actual en la colección
    int Posicion { get; }

    // Total de elementos que recorrerá este iterador
    int Total { get; }
}

// ── La COLECCIÓN: crea iteradores ──
public interface IHistorialCollection
{
    // Iterador simple — recorre todos cronológicamente
    IMovimientoIterator CrearIterador();

    // Iterador con filtros opcionales
    IMovimientoIterator CrearIteradorFiltrado(FiltroMovimiento filtro);

    // Iterador inverso — del más reciente al más antiguo
    IMovimientoIterator CrearIteradorInverso();
}

// ── Filtro de movimientos ──
public class FiltroMovimiento
{
    public DateTime? Desde       { get; set; }
    public DateTime? Hasta       { get; set; }
    public string?   Tipo        { get; set; }  // "Entrada"|"Salida"|"Ajuste"
    public int?      IdProducto  { get; set; }
    public string?   Usuario     { get; set; }
    public int       PageSize    { get; set; } = 50;  // paginación
    public int       Page        { get; set; } = 1;

    // Aplica el filtro a un movimiento
    public bool Aplica(MovimientoInventario m) =>
        (Desde      is null || m.Fecha >= Desde)      &&
        (Hasta      is null || m.Fecha <= Hasta)      &&
        (Tipo       is null || m.Tipo == Tipo)        &&
        (IdProducto is null || m.IdProducto == IdProducto) &&
        (Usuario    is null || m.Usuario == Usuario);
}