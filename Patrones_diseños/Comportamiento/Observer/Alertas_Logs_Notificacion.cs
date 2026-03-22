// Tres observers concretos — cada uno reacciona a su manera
// ante el mismo evento StockBajoEvento
// ─────────────────────────────────────────────────────────

namespace AguaMinami.Application.Inventory;

// ══════════════════════════════════════════════════════
//  1. ALERTA DE COMPRAS
//     Crea automáticamente una Orden_Compra en la BD
//     Mapea directamente a CU 004 "Compra de Materia Prima"
// ══════════════════════════════════════════════════════
public class AlertaComprasObserver : IStockObserver
{
    private readonly IOrdenCompraRepository _repo;
    private readonly IProveedorService       _proveedores;

    public string Nombre => "AlertaCompras";

    public AlertaComprasObserver(
        IOrdenCompraRepository repo,
        IProveedorService       proveedores)
    {
        _repo        = repo;
        _proveedores = proveedores;
    }

    public async Task OnStockBajo(StockBajoEvento e)
    {
        // Verifica si ya existe una orden de compra activa para este producto
        var ordenExistente = await _repo.ExisteOrdenActivaAsync(e.IdProducto);
        if (ordenExistente) return;  // no duplicar

        // Obtiene el proveedor principal de este producto
        var proveedor = await _proveedores.ObtenerPrincipalPorProductoAsync(e.IdProducto);

        // Crea la orden de compra con estado "Pendiente"
        var orden = new OrdenCompra
        {
            IdProducto           = e.IdProducto,
            IdProveedor          = proveedor?.Id,
            CantidadSugerida     = e.UnidadesFaltantes * 2,  // doble del faltante
            FechaOrden           = DateTime.Now,
            FechaEntregaEstimada = DateTime.Now.AddDays(3),
            Estado               = "Pendiente",
            GeneradaAutomaticamente = true,
            Motivo = $"Stock bajo: {e.StockActual} unidades (mínimo: {e.StockMinimo})"
        };

        await _repo.CrearAsync(orden);

        Console.WriteLine(
            $"[{Nombre}] Orden de compra creada para " +
            $"{e.NombreProducto} | Stock: {e.StockActual} | Cant. sugerida: {orden.CantidadSugerida}");
    }
}


// ══════════════════════════════════════════════════════
//  2. LOG DE MOVIMIENTO
//     Registra el evento en el historial de auditoría
//     "Toda modificación debe quedar registrada" — tu documento
// ══════════════════════════════════════════════════════
public class LogMovimientoObserver : IStockObserver
{
    private readonly IAuditoriaRepository _auditoria;

    public string Nombre => "LogMovimiento";

    public LogMovimientoObserver(IAuditoriaRepository auditoria) =>
        _auditoria = auditoria;

    public async Task OnStockBajo(StockBajoEvento e)
    {
        var registro = new RegistroAuditoria
        {
            Tipo        = "STOCK_BAJO",
            Descripcion = $"Stock bajo mínimo: {e.NombreProducto} " +
                          $"(actual: {e.StockActual}, mínimo: {e.StockMinimo})",
            Usuario     = e.UsuarioResponsable,
            Fecha       = e.FechaEvento,
            Datos       = System.Text.Json.JsonSerializer.Serialize(e)
        };

        await _auditoria.GuardarAsync(registro);

        Console.WriteLine(
            $"[{Nombre}] Auditoria guardada para {e.NombreProducto} " +
            $"por usuario {e.UsuarioResponsable}");
    }
}


// ══════════════════════════════════════════════════════
//  3. NOTIFICACIÓN AL ADMINISTRADOR
//     Registra la alerta en la tabla de notificaciones
//     El frontend React la muestra en el módulo de inventario
//     "Ver alertas stock mínimo" — CU de tu diagrama
// ══════════════════════════════════════════════════════
public class NotificacionAdminObserver : IStockObserver
{
    private readonly INotificacionRepository _repo;

    public string Nombre => "NotificacionAdmin";

    public NotificacionAdminObserver(INotificacionRepository repo) =>
        _repo = repo;

    public async Task OnStockBajo(StockBajoEvento e)
    {
        var notificacion = new Notificacion
        {
            Tipo      = "ALERTA_STOCK",
            Titulo    = $"Stock mínimo: {e.NombreProducto}",
            Mensaje   = $"Quedan {e.StockActual} unidades " +
                        $"(mínimo requerido: {e.StockMinimo}). " +
                        $"Faltan {e.UnidadesFaltantes} unidades.",
            Leida     = false,
            Fecha     = e.FechaEvento,
            Prioridad = e.StockActual == 0 ? "CRITICA" : "ALTA"
        };

        await _repo.CrearAsync(notificacion);

        Console.WriteLine(
            $"[{Nombre}] Alerta creada: {notificacion.Titulo} | " +
            $"Prioridad: {notificacion.Prioridad}");
    }
}