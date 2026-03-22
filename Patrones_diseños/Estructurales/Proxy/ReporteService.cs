// ReporteService.cs — implementación REAL con Entity Framework
// Consulta directamente la BD y construye los reportes.
// Nunca valida roles — eso es responsabilidad del Proxy.

namespace AguaMinami.Application.Reports;

public class ReporteService : IReporteService
{
    private readonly AguaMinamiDbContext _db;
    private readonly AppConfiguracion    _config;    // Singleton patrón 1

    public ReporteService(AguaMinamiDbContext db, AppConfiguracion config)
    {
        _db     = db;
        _config = config;
    }

    public async Task<ReporteVentas> GenerarReporteVentas(FiltroReporte filtro)
    {
        var query = _db.Pedidos
            .Where(p => p.Fecha >= filtro.Desde && p.Fecha <= filtro.Hasta);

        if (filtro.IdProducto.HasValue)
            query = query.Where(p =>
                p.Lineas.Any(l => l.IdProducto == filtro.IdProducto));

        if (filtro.IdRuta.HasValue)
            query = query.Where(p => p.IdRuta == filtro.IdRuta);

        var ventas = await query.ToListAsync();

        return new ReporteVentas
        {
            TotalVendido       = ventas.Sum(v => v.Total),
            TotalTransacciones = ventas.Count,
            VentasPorProducto  = ventas
                .SelectMany(v => v.Lineas)
                .GroupBy(l => l.Producto)
                .Select(g => new LineaReporte(g.Key, g.Sum(l => l.Total)))
                .OrderByDescending(l => l.Valor)
                .ToList(),
            VentasPorRuta = ventas
                .Where(v => v.IdRuta.HasValue)
                .GroupBy(v => v.IdRuta!)
                .Select(g => new LineaReporte(
                    $"Ruta {g.Key}", g.Sum(v => v.Total)))
                .ToList(),
            FiltroAplicado = filtro,
            GeneradoEn     = DateTime.Now
        };
    }

    public async Task<ReporteNomina> GenerarReporteNomina(int anio, int mes)
    {
        var detalles = await _db.DetallesNomina
            .Where(d => d.Anio == anio && d.Mes == mes)
            .Include(d => d.Empleado)
            .ToListAsync();

        return new ReporteNomina
        {
            TotalSueldos     = detalles.Sum(d => d.SueldoNeto),
            TotalDeducciones = detalles.Sum(d => d.TotalDescuentos),
            TotalPrestamos   = detalles.Sum(d => d.CuotaPrestamo),
            Periodo          = $"{mes}/{anio}",
            DetalleEmpleados = detalles.Select(d => new LineaReporte(
                $"{d.Empleado.Nombre} {d.Empleado.Apellido}",
                d.SueldoNeto,
                $"Descuentos: {_config.MonedaLocal}{d.TotalDescuentos:F2}"
            )).ToList()
        };
    }

    public async Task<ReporteInventario> GenerarReporteInventario(FiltroReporte filtro)
    {
        var stocks = await _db.StockAlmacen
            .Include(s => s.Producto)
            .ToListAsync();

        var movimientos = await _db.MovimientoInventario
            .Where(m => m.Fecha >= filtro.Desde && m.Fecha <= filtro.Hasta)
            .ToListAsync();

        return new ReporteInventario
        {
            ProductosBajoMinimo = stocks
                .Where(s => s.Cantidad <= s.StockMinimo)
                .Select(s => new LineaReporte(
                    s.Producto.Nombre, s.Cantidad,
                    $"Mínimo: {s.StockMinimo}"))
                .ToList(),
            Movimientos = movimientos
                .Select(m => new LineaReporte(
                    $"[{m.Tipo}] {m.NombreProducto}", m.Cantidad,
                    m.Motivo))
                .ToList(),
            ValorTotalStock = stocks.Sum(s =>
                s.Cantidad * s.Producto.PrecioUnitario)
        };
    }

    public async Task<ReporteGastos> GenerarReporteGastos(FiltroReporte filtro)
    {
        var ordenes = await _db.OrdenesCompra
            .Where(o => o.FechaOrden >= filtro.Desde &&
                        o.FechaOrden <= filtro.Hasta &&
                        o.Estado == "Recibida")
            .ToListAsync();

        var totalCompras = ordenes.Sum(o => o.CostoTotal);

        return new ReporteGastos
        {
            TotalCompras      = totalCompras,
            TotalDevoluciones = 0m,    // se completará con módulo de devoluciones
            TotalGastos       = totalCompras,
            Detalle = ordenes.Select(o => new LineaReporte(
                $"Orden #{o.Id} - Producto {o.IdProducto}",
                o.CostoTotal,
                o.FechaOrden.ToString("dd/MM/yyyy")
            )).ToList()
        };
    }
}