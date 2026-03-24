// ---- Abstracciones concretas ----

public class ReporteVentas : ReporteBase
{
    private readonly IVentaRepository _ventaRepo;

    public ReporteVentas(IRendizador rendizador, AppConfiguracion config,
                         IVentaRepository ventaRepo)
        : base(rendizador, config) => _ventaRepo = ventaRepo;

    protected override async Task<DatosReporte> ObtenerDatosAsync(FiltroReporte filtro)
    {
        var ventas = await _ventaRepo.ObtenerPorPeriodoAsync(filtro.FechaInicio, filtro.FechaFin);

        return new DatosReporte
        {
            Columnas = ["Fecha", "Cliente", "Productos", "Total RD$", "Comprobante"],
            Filas    = ventas.Select(v => new Dictionary<string, object>
            {
                ["Fecha"]        = v.Fecha.ToString("dd/MM/yyyy"),
                ["Cliente"]      = v.Cliente.Nombre,
                ["Productos"]    = string.Join(", ", v.Lineas.Select(l => l.Producto.Nombre)),
                ["Total RD$"]    = v.Total.ToString("N2"),
                ["Comprobante"]  = v.TipoComprobante
            }).ToList(),
            Resumen = new Dictionary<string, object>
            {
                ["Total ventas"]     = ventas.Count,
                ["Monto total RD$"]  = ventas.Sum(v => v.Total).ToString("N2"),
                ["Promedio por venta"] = ventas.Average(v => v.Total).ToString("N2")
            }
        };
    }

    protected override string ObtenerTitulo(FiltroReporte f) =>
        $"Reporte_Ventas_{f.FechaInicio:MMMyyyy}_{f.FechaFin:MMMyyyy}";
}

// ──────────────────────────────────────────────

public class ReporteNomina : ReporteBase
{
    private readonly INominaRepository _nominaRepo;

    public ReporteNomina(IRendizador rendizador, AppConfiguracion config,
                         INominaRepository nominaRepo)
        : base(rendizador, config) => _nominaRepo = nominaRepo;

    protected override async Task<DatosReporte> ObtenerDatosAsync(FiltroReporte filtro)
    {
        var registros = await _nominaRepo.ObtenerPorPeriodoAsync(
                            filtro.FechaInicio, filtro.FechaFin);

        return new DatosReporte
        {
            Columnas = ["Empleado", "Cargo", "Sueldo base", "Descuentos", "Neto RD$", "Tipo nómina"],
            Filas    = registros.Select(r => new Dictionary<string, object>
            {
                ["Empleado"]    = $"{r.Empleado.Nombre} {r.Empleado.Apellido}",
                ["Cargo"]       = r.Empleado.Cargo,
                ["Sueldo base"] = r.SueldoBase.ToString("N2"),
                ["Descuentos"]  = r.TotalDescuentos.ToString("N2"),
                ["Neto RD$"]    = r.SueldoNeto.ToString("N2"),
                ["Tipo nómina"] = r.TipoNomina   // Sueldo Normal / Vacaciones / Regalía
            }).ToList(),
            Resumen = new Dictionary<string, object>
            {
                ["Total empleados"] = registros.Select(r => r.EmpleadoId).Distinct().Count(),
                ["Masa salarial RD$"] = registros.Sum(r => r.SueldoNeto).ToString("N2"),
                ["Factor días (Art. 177)"] = _config.FactorDiasLaborables  // 23.83 del Singleton
            }
        };
    }

    protected override string ObtenerTitulo(FiltroReporte f) =>
        $"Reporte_Nomina_{f.FechaInicio:MMMyyyy}_{f.FechaFin:MMMyyyy}";
}
