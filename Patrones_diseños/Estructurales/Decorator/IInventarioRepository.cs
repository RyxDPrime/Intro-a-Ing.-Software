// IInventarioRepository.cs — contrato que todos implementan
// El repositorio real y los decoradores tienen la misma interfaz.
// Los servicios nunca saben con qué capa están hablando.

namespace AguaMinami.Application.Inventory;

public interface IInventarioRepository
{
    // Consultas
    Task<int>                       ObtenerStockActualAsync(int idProducto);
    Task<List<StockProducto>>      ObtenerTodosAsync(int idAlmacen);
    Task<List<MovimientoInventario>> ObtenerHistorialAsync(int idProducto, DateTime? desde);

    // Escrituras
    Task<int> GuardarMovimientoAsync(MovimientoInventario movimiento);
    Task      ActualizarStockAsync(int idProducto, int nuevoStock);
}

public record StockProducto(int IdProducto, string Nombre, int Cantidad, int StockMinimo);

// ── Implementación REAL con Entity Framework Core ──
public class InventarioRepository : IInventarioRepository
{
    private readonly AguaMinamiDbContext _db;

    public InventarioRepository(AguaMinamiDbContext db) => _db = db;

    public async Task<int> ObtenerStockActualAsync(int idProducto) =>
        await _db.StockAlmacen
            .Where(s => s.IdProducto == idProducto)
            .Select(s => s.Cantidad)
            .FirstOrDefaultAsync();

    public async Task<List<StockProducto>> ObtenerTodosAsync(int idAlmacen) =>
        await _db.StockAlmacen
            .Where(s => s.IdAlmacen == idAlmacen)
            .Select(s => new StockProducto(
                s.IdProducto, s.Producto.Nombre, s.Cantidad, s.StockMinimo))
            .ToListAsync();

    public async Task<List<MovimientoInventario>> ObtenerHistorialAsync(
        int idProducto, DateTime? desde)
    {
        var q = _db.MovimientoInventario
            .Where(m => m.IdProducto == idProducto);
        if (desde.HasValue)
            q = q.Where(m => m.Fecha >= desde.Value);
        return await q.OrderByDescending(m => m.Fecha).ToListAsync();
    }

    public async Task<int> GuardarMovimientoAsync(MovimientoInventario movimiento)
    {
        _db.MovimientoInventario.Add(movimiento);
        await _db.SaveChangesAsync();
        return movimiento.Id;
    }

    public async Task ActualizarStockAsync(int idProducto, int nuevoStock)
    {
        var stock = await _db.StockAlmacen
            .FirstAsync(s => s.IdProducto == idProducto);
        stock.Cantidad = nuevoStock;
        await _db.SaveChangesAsync();
    }
}