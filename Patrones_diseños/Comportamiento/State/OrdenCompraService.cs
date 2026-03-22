// OrdenCompraService.cs — lógica de aplicación
// Coordina: State (transiciones) + Observer (notifica al almacén
// cuando llega mercancía) + Singleton (config global)

using AguaMinami.Application.Inventory;
using AguaMinami.Infrastructure.Config;

namespace AguaMinami.Application.Purchasing;

public class OrdenCompraService
{
    private readonly IOrdenCompraRepository _repo;
    private readonly StockAlmacen           _almacen;
    private readonly AppConfiguracion        _config;

    public OrdenCompraService(
        IOrdenCompraRepository repo,
        StockAlmacen           almacen,
        AppConfiguracion        config)
    {
        _repo    = repo;
        _almacen = almacen;
        _config  = config;
    }

    // ── Carga la orden y restaura su estado desde la BD ──
    public async Task<OrdenCompra> ObtenerAsync(int idOrden)
    {
        var dto = await _repo.ObtenerPorIdAsync(idOrden)
            ?? throw new KeyNotFoundException($"Orden {idOrden} no encontrada");

        // Restaura el objeto con el estado guardado en BD
        var orden = new OrdenCompra(dto.Estado)
        {
            Id                    = dto.Id,
            IdProducto            = dto.IdProducto,
            IdProveedor           = dto.IdProveedor,
            CantidadSolicitada    = dto.CantidadSolicitada,
            CantidadRecibida      = dto.CantidadRecibida,
            CostoTotal            = dto.CostoTotal,
            FechaOrden            = dto.FechaOrden,
            FechaEntregaEstimada  = dto.FechaEntregaEstimada,
            FechaProcesado        = dto.FechaProcesado,
            FechaRecepcion        = dto.FechaRecepcion,
            UsuarioCreacion       = dto.UsuarioCreacion,
            UsuarioProceso        = dto.UsuarioProceso,
            UsuarioRecepcion      = dto.UsuarioRecepcion,
            MotivoCancelacion     = dto.MotivoCancelacion,
            GeneradaAutomaticamente = dto.GeneradaAutomaticamente
        };
        return orden;
    }

    // ── Procesar: contable aprueba la orden ──
    public async Task ProcesarAsync(int idOrden, string usuario)
    {
        var orden = await ObtenerAsync(idOrden);
        orden.Procesar(usuario);               // delega en EstadoPendiente
        await _repo.ActualizarEstadoAsync(orden);
    }

    // ── Recibir: llegó la mercancía al almacén ──
    // Después de transicionar, registra la entrada en StockAlmacen
    // El Observer verificará si el stock sigue bajo mínimo
    public async Task RecibirAsync(
        int    idOrden,
        int    cantidadRecibida,
        string usuario,
        string nombreProducto)
    {
        var orden = await ObtenerAsync(idOrden);

        // 1. Transiciona el estado (delega en EstadoEnProceso)
        orden.Recibir(cantidadRecibida, usuario);
        await _repo.ActualizarEstadoAsync(orden);

        // 2. Registra la entrada en el inventario — activa observers si corresponde
        await _almacen.RegistrarMovimiento(
            idProducto:     orden.IdProducto,
            nombreProducto: nombreProducto,
            cantidad:       cantidadRecibida,
            tipo:           "Entrada",
            motivo:         $"Recepción orden #{idOrden}",
            usuario:        usuario
        );

        // 3. Guarda historial de transiciones en BD
        await _repo.GuardarHistorialAsync(orden.Historial);
    }

    // ── Cancelar: rechazada por contable o proveedor sin stock ──
    public async Task CancelarAsync(
        int    idOrden,
        string motivo,
        string usuario)
    {
        var orden = await ObtenerAsync(idOrden);
        orden.Cancelar(motivo, usuario);       // delega en estado actual
        await _repo.ActualizarEstadoAsync(orden);
        await _repo.GuardarHistorialAsync(orden.Historial);
    }
}