// Cuatro módulos manejadores — cada uno reacciona a los eventos que le importan.
// Ninguno conoce la existencia de los demás.
// ───────────────────────────────────────────────────────────────────────

namespace AguaMinami.Application.Mediator.Handlers;

// ── 1. Módulo Inventario: descuenta stock cuando se registra una venta ──
public class InventarioVentaHandler : IManejadorEvento<VentaRegistradaEvento>
{
    private readonly InventarioInvoker    _invoker;
    private readonly StockAlmacen         _almacen;
    private readonly IMovimientoRepository _movRepo;

    public string Nombre => "ModuloInventario";

    public InventarioVentaHandler(
        InventarioInvoker    invoker,
        StockAlmacen         almacen,
        IMovimientoRepository movRepo)
    {
        _invoker = invoker;
        _almacen = almacen;
        _movRepo = movRepo;
    }

    public async Task Manejar(VentaRegistradaEvento evento)
    {
        // Encola una SalidaCommand por cada línea vendida (patrón Command)
        foreach (var linea in evento.Lineas)
        {
            var datos = new DatosMovimiento(
                linea.IdProducto, linea.NombreProducto, linea.Cantidad,
                $"Venta #{evento.IdVenta} - {evento.NombreCliente}",
                evento.Originador, 1);

            _invoker.Encolar(new SalidaInventarioCommand(_almacen, datos, _movRepo));
        }

        await _invoker.EjecutarCola();
    }
}

// ── 2. Módulo Facturación: emite el documento al registrar la venta ──
public class FacturacionVentaHandler : IManejadorEvento<VentaRegistradaEvento>
{
    private readonly DocumentoCreator _docCreator;

    public string Nombre => "ModuloFacturacion";

    public FacturacionVentaHandler(DocumentoCreator docCreator) =>
        _docCreator = docCreator;

    public Task Manejar(VentaRegistradaEvento evento)
    {
        // Factory Method (patrón 2) elige el tipo de documento correcto
        var datos = new DatosFactura(
            IdVenta:       evento.IdVenta,
            NombreCliente: evento.NombreCliente,
            RncCliente:    null,
            NCF:           evento.NCF,
            Lineas:        evento.Lineas.Select(l =>
                new LineaFactura(l.NombreProducto, l.Cantidad,
                    l.PrecioUnitario, l.Cantidad * l.PrecioUnitario)).ToList(),
            Subtotal: evento.Total,
            ITBIS:    0m,
            Total:    evento.Total);

        _docCreator.EmitirDocumento(evento.TipoComprobante, datos);
        return Task.CompletedTask;
    }
}

// ── 3. Módulo Notificaciones: alerta al admin si la venta es grande ──
public class NotificacionVentaHandler : IManejadorEvento<VentaRegistradaEvento>
{
    private readonly INotificacionRepository _repo;
    private const decimal UMBRAL_VENTA_GRANDE = 5000m;

    public string Nombre => "ModuloNotificaciones";

    public NotificacionVentaHandler(INotificacionRepository repo) => _repo = repo;

    public async Task Manejar(VentaRegistradaEvento evento)
    {
        if (evento.Total < UMBRAL_VENTA_GRANDE) return;

        await _repo.CrearAsync(new Notificacion
        {
            Tipo      = "VENTA_GRANDE",
            Titulo    = $"Venta destacada: RD${evento.Total:F2}",
            Mensaje   = $"Cliente: {evento.NombreCliente} | Venta #{evento.IdVenta}",
            Prioridad = "MEDIA",
            Leida     = false,
            Fecha     = DateTime.Now
        });
    }
}

// ── 4. Módulo Reportes: actualiza KPIs al recibir mercancía ──
public class ReporteMercanciaHandler : IManejadorEvento<MercanciaRecibidaEvento>
{
    private readonly IKpiRepository _kpis;

    public string Nombre => "ModuloReportes";

    public ReporteMercanciaHandler(IKpiRepository kpis) => _kpis = kpis;

    public async Task Manejar(MercanciaRecibidaEvento evento)
    {
        await _kpis.ActualizarStockKpiAsync(
            evento.IdProducto,
            evento.StockNuevo,
            evento.OcurrioEn);
    }
}

// ── 5. Nómina calculada: registra el gasto financiero ──
public class GastoNominaHandler : IManejadorEvento<NominaCalculadaEvento>
{
    private readonly IGastoRepository _gastos;

    public string Nombre => "ModuloGastos";

    public GastoNominaHandler(IGastoRepository gastos) => _gastos = gastos;

    public async Task Manejar(NominaCalculadaEvento evento)
    {
        await _gastos.RegistrarAsync(new GastoNomina
        {
            IdEmpleado     = evento.IdEmpleado,
            NombreEmpleado = evento.NombreEmpleado,
            Monto          = evento.SueldoNeto,
            Tipo           = evento.TipoCalculo,
            Periodo        = evento.Periodo,
            Fecha          = evento.OcurrioEn
        });
    }
}