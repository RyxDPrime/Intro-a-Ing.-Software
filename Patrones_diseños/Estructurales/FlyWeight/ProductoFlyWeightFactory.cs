// ---- La factory: garantiza una sola instancia por producto ----
public class ProductoFlyweightFactory
{
    // Diccionario estático: clave = Id del producto en BD
    private static readonly Dictionary<int, ProductoFlyweight> _cache = new();
    private static readonly SemaphoreSlim _lock = new(1, 1);

    private readonly IProductoRepository _repo;
    private readonly AppConfiguracion _config;   // Singleton (patrón 1)

    public ProductoFlyweightFactory(IProductoRepository repo, AppConfiguracion config)
    {
        _repo   = repo;
        _config = config;
    }

    // Obtiene el flyweight del cache o lo crea si no existe todavía
    public async Task<ProductoFlyweight> ObtenerAsync(int productoId)
    {
        if (_cache.TryGetValue(productoId, out var cached))
            return cached;   // Cache hit: sin consulta a BD

        await _lock.WaitAsync();
        try
        {
            // Double-check dentro del lock (igual que el Singleton, patrón 1)
            if (_cache.TryGetValue(productoId, out cached))
                return cached;

            var p = await _repo.ObtenerPorIdAsync(productoId)
                    ?? throw new KeyNotFoundException(
                           $"Producto {productoId} no existe.");

            var flyweight = new ProductoFlyweight(
                p.Id, p.Nombre, p.Codigo, p.Unidad,
                p.PrecioBase, p.Categoria, p.StockMinimo);

            _cache[productoId] = flyweight;
            return flyweight;
        }
        finally { _lock.Release(); }
    }

    // Pre-carga todo el catálogo al arrancar el servidor
    public async Task PrecargarCatalogoAsync()
    {
        var productos = await _repo.ObtenerTodosAsync();
        await _lock.WaitAsync();
        try
        {
            foreach (var p in productos)
                if (!_cache.ContainsKey(p.Id))
                    _cache[p.Id] = new ProductoFlyweight(
                        p.Id, p.Nombre, p.Codigo, p.Unidad,
                        p.PrecioBase, p.Categoria, p.StockMinimo);
        }
        finally { _lock.Release(); }
    }

    // Invalida un flyweight cuando se edita el producto en BD
    public void Invalidar(int productoId) => _cache.Remove(productoId);

    // Info de diagnóstico para el endpoint de admin
    public EstadoCache ObtenerEstado() => new()
    {
        TotalProductosCacheados = _cache.Count,
        ProductosCacheados      = _cache.Values
            .Select(f => new { f.Id, f.Nombre, f.Codigo })
            .ToList<object>()
    };
}

public class EstadoCache
{
    public int  TotalProductosCacheados  { get; set; }
    public List<object> ProductosCacheados { get; set; } = new();
}
