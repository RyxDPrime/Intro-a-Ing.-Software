// RepositoryDecorator.cs — clase base abstracta para todos los decoradores
// Implementa IInventarioRepository delegando en _inner por defecto.
// Los decoradores concretos solo sobreescriben los métodos que les interesan.

namespace AguaMinami.Infrastructure.Inventory.Decorators;

public abstract class InventarioRepositoryDecorator : IInventarioRepository
{
    // El repositorio envuelto — puede ser el real o OTRO decorador
    protected readonly IInventarioRepository _inner;

    protected InventarioRepositoryDecorator(IInventarioRepository inner) =>
        _inner = inner;

    // Por defecto: delega en el repositorio envuelto sin hacer nada extra.
    // Los decoradores concretos sobreescriben solo lo que necesitan.
    public virtual Task<int> ObtenerStockActualAsync(int idProducto) =>
        _inner.ObtenerStockActualAsync(idProducto);

    public virtual Task<List<StockProducto>> ObtenerTodosAsync(int idAlmacen) =>
        _inner.ObtenerTodosAsync(idAlmacen);

    public virtual Task<List<MovimientoInventario>> ObtenerHistorialAsync(
        int idProducto, DateTime? desde) =>
        _inner.ObtenerHistorialAsync(idProducto, desde);

    public virtual Task<int> GuardarMovimientoAsync(MovimientoInventario movimiento) =>
        _inner.GuardarMovimientoAsync(movimiento);

    public virtual Task ActualizarStockAsync(int idProducto, int nuevoStock) =>
        _inner.ActualizarStockAsync(idProducto, nuevoStock);
}