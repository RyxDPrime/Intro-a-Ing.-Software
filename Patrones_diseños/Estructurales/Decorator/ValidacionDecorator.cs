// ValidacionDecorator.cs — capa de reglas de negocio
// Intercepta escrituras y aplica validaciones antes de llegar a BD.
// Centraliza reglas como: no stock negativo, motivo obligatorio,
// cantidad máxima por movimiento. Usa el Singleton para el stock mínimo.

using AguaMinami.Infrastructure.Config;

namespace AguaMinami.Infrastructure.Inventory.Decorators;

public class ValidacionInventarioDecorator : InventarioRepositoryDecorator
{
    private readonly AppConfiguracion _config;
    private const int MAX_CANTIDAD_MOVIMIENTO = 10_000;

    public ValidacionInventarioDecorator(
        IInventarioRepository inner,
        AppConfiguracion      config)
        : base(inner) => _config = config;

    public override async Task<int> GuardarMovimientoAsync(
        MovimientoInventario movimiento)
    {
        // ── Regla 1: motivo obligatorio ──
        if (string.IsNullOrWhiteSpace(movimiento.Motivo))
            throw new ArgumentException(
                "El motivo del movimiento es obligatorio.");

        // ── Regla 2: cantidad debe ser positiva ──
        if (movimiento.Cantidad <= 0)
            throw new ArgumentException(
                $"La cantidad del movimiento debe ser mayor a cero. Recibido: {movimiento.Cantidad}");

        // ── Regla 3: cantidad máxima por movimiento ──
        if (movimiento.Cantidad > MAX_CANTIDAD_MOVIMIENTO)
            throw new ArgumentException(
                $"Cantidad excede el máximo permitido por movimiento ({MAX_CANTIDAD_MOVIMIENTO:N0}).");

        // ── Regla 4: salidas no pueden dejar stock negativo ──
        if (movimiento.Tipo == "Salida")
        {
            var stockActual = await _inner.ObtenerStockActualAsync(movimiento.IdProducto);

            if (stockActual - movimiento.Cantidad < 0)
                throw new InvalidOperationException(
                    $"Stock insuficiente para el movimiento. " +
                    $"Disponible: {stockActual}, solicitado: {movimiento.Cantidad}.");
        }

        // ── Regla 5: ajuste no puede ser negativo ──
        if (movimiento.Tipo == "Ajuste" && movimiento.Cantidad < 0)
            throw new ArgumentException(
                "Un ajuste de inventario no puede establecer stock negativo.");

        // ── Regla 6: usuario registrado ──
        if (string.IsNullOrWhiteSpace(movimiento.Usuario))
            throw new ArgumentException(
                "El usuario responsable del movimiento es obligatorio.");

        // ✓ Todas las validaciones pasaron — delega en el siguiente decorador o repo real
        return await _inner.GuardarMovimientoAsync(movimiento);
    }

    // Valida también la actualización directa de stock
    public override async Task ActualizarStockAsync(int idProducto, int nuevoStock)
    {
        if (nuevoStock < 0)
            throw new ArgumentException(
                $"El stock no puede ser negativo. Valor recibido: {nuevoStock}");

        await _inner.ActualizarStockAsync(idProducto, nuevoStock);
    }
}