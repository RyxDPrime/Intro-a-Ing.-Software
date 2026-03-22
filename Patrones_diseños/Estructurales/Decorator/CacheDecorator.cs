// CacheDecorator.cs — capa de caché en memoria
// Evita consultas repetidas a BD para el stock actual.
// Crucial en la red LAN de Agua Minami donde la pantalla de inventario
// se refresca frecuentemente mostrando todos los productos.
// Invalida el caché automáticamente al escribir (entrada, salida, ajuste).

using Microsoft.Extensions.Caching.Memory;

namespace AguaMinami.Infrastructure.Inventory.Decorators;

public class CacheInventarioDecorator : InventarioRepositoryDecorator
{
    private readonly IMemoryCache _cache;
    private const int TTL_SEGUNDOS = 30;    // caché de 30 segundos

    private static string ClaveStock(int id)    => $"stock:{id}";
    private static string ClaveTodos(int alma)  => $"stock:todos:{alma}";

    public CacheInventarioDecorator(
        IInventarioRepository inner,
        IMemoryCache          cache)
        : base(inner) => _cache = cache;

    // ObtenerStock: devuelve desde caché si existe, si no va a BD y cachea
    public override async Task<int> ObtenerStockActualAsync(int idProducto)
    {
        var clave = ClaveStock(idProducto);

        if (_cache.TryGetValue(clave, out int stockCacheado))
            return stockCacheado;         // cache HIT — no va a BD

        // cache MISS — consulta BD y guarda resultado
        var stock = await _inner.ObtenerStockActualAsync(idProducto);

        _cache.Set(clave, stock, TimeSpan.FromSeconds(TTL_SEGUNDOS));
        return stock;
    }

    // ObtenerTodos: cachea el listado completo del almacén
    public override async Task<List<StockProducto>> ObtenerTodosAsync(int idAlmacen)
    {
        var clave = ClaveTodos(idAlmacen);

        if (_cache.TryGetValue(clave, out List<StockProducto>? lista) && lista is not null)
            return lista;

        lista = await _inner.ObtenerTodosAsync(idAlmacen);
        _cache.Set(clave, lista, TimeSpan.FromSeconds(TTL_SEGUNDOS));
        return lista;
    }

    // Al guardar un movimiento → invalida el caché del producto afectado
    public override async Task<int> GuardarMovimientoAsync(MovimientoInventario movimiento)
    {
        var id = await _inner.GuardarMovimientoAsync(movimiento);

        // Invalida el caché del producto y el listado completo
        _cache.Remove(ClaveStock(movimiento.IdProducto));
        _cache.Remove(ClaveTodos(movimiento.IdAlmacen));

        return id;
    }

    // Al actualizar stock → invalida caché del producto
    public override async Task ActualizarStockAsync(int idProducto, int nuevoStock)
    {
        await _inner.ActualizarStockAsync(idProducto, nuevoStock);
        _cache.Remove(ClaveStock(idProducto));
    }
}