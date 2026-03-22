// IReporteService.cs — interfaz compartida por el servicio real y el proxy
// El controller nunca sabe con cuál está hablando.
// Mapea a "Generación de reportes" del CU de tu documento —
// solo el Administrador tiene acceso.

namespace AguaMinami.Application.Reports;

public interface IReporteService
{
    // Reporte de ventas: total vendido por período, por producto, por ruta
    Task<ReporteVentas>    GenerarReporteVentas(FiltroReporte filtro);

    // Reporte de nómina: sueldos pagados, deducciones, préstamos
    Task<ReporteNomina>    GenerarReporteNomina(int anio, int mes);

    // Reporte de inventario: stock actual, movimientos, alertas
    Task<ReporteInventario> GenerarReporteInventario(FiltroReporte filtro);

    // Reporte de gastos: compras, devoluciones, gastos operativos
    Task<ReporteGastos>    GenerarReporteGastos(FiltroReporte filtro);
}

// ── Filtro compartido por todos los reportes ──
public record FiltroReporte(
    DateTime Desde,
    DateTime Hasta,
    int?     IdEmpleado  = null,
    int?     IdProducto  = null,
    int?     IdRuta      = null
);

// ── Modelos de respuesta ──
public class ReporteVentas
{
    public decimal             TotalVendido      { get; set; }
    public int                 TotalTransacciones { get; set; }
    public List<LineaReporte>  VentasPorProducto  { get; set; } = [];
    public List<LineaReporte>  VentasPorRuta      { get; set; } = [];
    public FiltroReporte       FiltroAplicado     { get; set; } = null!;
    public DateTime            GeneradoEn         { get; set; }
    public string              GeneradoPor        { get; set; } = "";
}

public class ReporteNomina
{
    public decimal             TotalSueldos      { get; set; }
    public decimal             TotalDeducciones  { get; set; }
    public decimal             TotalPrestamos    { get; set; }
    public List<LineaReporte>  DetalleEmpleados  { get; set; } = [];
    public string              Periodo           { get; set; } = "";
}

public class ReporteInventario
{
    public List<LineaReporte>  ProductosBajoMinimo { get; set; } = [];
    public List<LineaReporte>  Movimientos         { get; set; } = [];
    public decimal             ValorTotalStock     { get; set; }
}

public class ReporteGastos
{
    public decimal             TotalGastos         { get; set; }
    public decimal             TotalCompras        { get; set; }
    public decimal             TotalDevoluciones   { get; set; }
    public List<LineaReporte>  Detalle             { get; set; } = [];
}

public record LineaReporte(string Descripcion, decimal Valor, string? Detalle = null);