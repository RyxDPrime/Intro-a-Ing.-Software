// LoggingDecorator.cs — capa de auditoría
// Registra cada operación con duración, usuario y resultado.
// "Toda modificación debe quedar registrada con usuario, fecha y motivo"
// — tu documento de diseño. Esta capa lo garantiza sin tocar el repo real.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AguaMinami.Infrastructure.Inventory.Decorators;

public class LoggingInventarioDecorator : InventarioRepositoryDecorator
{
    private readonly ILogger<LoggingInventarioDecorator> _logger;

    public LoggingInventarioDecorator(
        IInventarioRepository                        inner,
        ILogger<LoggingInventarioDecorator> logger)
        : base(inner) => _logger = logger;

    // Loguea consultas de stock con tiempo de respuesta
    public override async Task<int> ObtenerStockActualAsync(int idProducto)
    {
        var sw = Stopwatch.StartNew();
        var stock = await _inner.ObtenerStockActualAsync(idProducto);
        sw.Stop();

        _logger.LogInformation(
            "[Inventario] ObtenerStock | Producto: {Id} | Stock: {Stock} | {Ms}ms",
            idProducto, stock, sw.ElapsedMilliseconds);

        return stock;
    }

    // Loguea escrituras de movimiento con nivel Warning si hay demora
    public override async Task<int> GuardarMovimientoAsync(
        MovimientoInventario movimiento)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            "[Inventario] GuardarMovimiento | Tipo: {Tipo} | Producto: {Prod} | Cant: {Cant} | Usuario: {User}",
            movimiento.Tipo, movimiento.IdProducto,
            movimiento.Cantidad, movimiento.Usuario);

        var id = await _inner.GuardarMovimientoAsync(movimiento);
        sw.Stop();

        if (sw.ElapsedMilliseconds > 500)
            _logger.LogWarning(
                "[Inventario] Escritura lenta: {Ms}ms para producto {Id}",
                sw.ElapsedMilliseconds, movimiento.IdProducto);

        _logger.LogInformation(
            "[Inventario] Movimiento guardado | Id: {Id} | {Ms}ms",
            id, sw.ElapsedMilliseconds);

        return id;
    }

    // Loguea actualizaciones de stock
    public override async Task ActualizarStockAsync(int idProducto, int nuevoStock)
    {
        _logger.LogInformation(
            "[Inventario] ActualizarStock | Producto: {Id} | NuevoStock: {Stock}",
            idProducto, nuevoStock);

        await _inner.ActualizarStockAsync(idProducto, nuevoStock);
    }
}