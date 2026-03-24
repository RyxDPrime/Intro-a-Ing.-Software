// ImprimirReporteFactory — genera texto formateado para impresora
// Usa la configuración de impresora del Singleton (patrón 1).
// Incluye márgenes, separadores y formato de ticket.

using AguaMinami.Infrastructure.Config;

namespace AguaMinami.Application.Reports.Factories;

public class ImprimirReporteFactory : IReporteFactory
{
    private readonly AppConfiguracion _config;

    public string Formato => "Impresora";

    public ImprimirReporteFactory(AppConfiguracion config) => _config = config;

    public IReporteVentas     CrearReporteVentas()     => new VentasImpreso(_config);
    public IReporteNomina     CrearReporteNomina()     => new NominaImpreso(_config);
    public IReporteInventario CrearReporteInventario() => new InventarioImpreso(_config);
    public IReporteGastos     CrearReporteGastos()     => new GastosImpreso(_config);
}

public class VentasImpreso : IReporteVentas
{
    private readonly AppConfiguracion _config;
    private const string SEP = "─────────────────────────────────";

    public string Formato => "Impresora";
    public VentasImpreso(AppConfiguracion config) => _config = config;

    public object Generar(ReporteVentas datos)
    {
        var sb = new StringBuilder();
        sb.AppendLine("            AGUA MINAMI");
        sb.AppendLine("         REPORTE DE VENTAS");
        sb.AppendLine(SEP);
        sb.AppendLine($"Generado: {datos.GeneradoEn:dd/MM/yyyy HH:mm}");
        sb.AppendLine($"Por     : {datos.GeneradoPor}");
        sb.AppendLine($"Impres. : {_config.ImpresoraReportes.Nombre}");
        sb.AppendLine(SEP);
        sb.AppendLine($"Total vendido   : {_config.MonedaLocal}{datos.TotalVendido:N2}");
        sb.AppendLine($"Transacciones   : {datos.TotalTransacciones}");
        sb.AppendLine(SEP);
        sb.AppendLine("VENTAS POR PRODUCTO:");

        foreach (var v in datos.VentasPorProducto.Take(10))
            sb.AppendLine($"  {v.Descripcion,-20} {_config.MonedaLocal}{v.Valor,8:N2}");

        sb.AppendLine(SEP);
        return new
        {
            Tipo      = "ReporteVentas",
            Formato   = Formato,
            Impresora = _config.ImpresoraReportes.Nombre,
            Contenido = sb.ToString()
        };
    }
}

public class NominaImpreso : IReporteNomina
{
    private readonly AppConfiguracion _config;
    private const string SEP = "─────────────────────────────────";

    public string Formato => "Impresora";
    public NominaImpreso(AppConfiguracion config) => _config = config;

    public object Generar(ReporteNomina datos)
    {
        var sb = new StringBuilder();
        sb.AppendLine("            AGUA MINAMI");
        sb.AppendLine("         REPORTE DE NÓMINA");
        sb.AppendLine(SEP);
        sb.AppendLine($"Período  : {datos.Periodo}");
        sb.AppendLine(SEP);
        sb.AppendLine($"Total sueldos    : {_config.MonedaLocal}{datos.TotalSueldos:N2}");
        sb.AppendLine($"Total deducciones: {_config.MonedaLocal}{datos.TotalDeducciones:N2}");
        sb.AppendLine($"Total préstamos  : {_config.MonedaLocal}{datos.TotalPrestamos:N2}");
        sb.AppendLine(SEP);
        sb.AppendLine("DETALLE EMPLEADOS:");

        foreach (var e in datos.DetalleEmpleados)
            sb.AppendLine($"  {e.Descripcion,-22} {_config.MonedaLocal}{e.Valor,8:N2}");

        sb.AppendLine(SEP);
        return new
        {
            Tipo      = "ReporteNomina",
            Formato   = Formato,
            Impresora = _config.ImpresoraReportes.Nombre,
            Contenido = sb.ToString()
        };
    }
}

public class InventarioImpreso : IReporteInventario
{
    private readonly AppConfiguracion _config;
    private const string SEP = "─────────────────────────────────";
    public string Formato => "Impresora";
    public InventarioImpreso(AppConfiguracion config) => _config = config;

    public object Generar(ReporteInventario datos)
    {
        var sb = new StringBuilder();
        sb.AppendLine("       REPORTE DE INVENTARIO");
        sb.AppendLine(SEP);
        sb.AppendLine($"Valor total stock: {_config.MonedaLocal}{datos.ValorTotalStock:N2}");
        sb.AppendLine($"Productos bajo mínimo: {datos.ProductosBajoMinimo.Count}");
        sb.AppendLine(SEP);
        if (datos.ProductosBajoMinimo.Any())
        {
            sb.AppendLine("⚠ ALERTAS STOCK:");
            foreach (var p in datos.ProductosBajoMinimo)
                sb.AppendLine($"  {p.Descripcion,-20} Stock: {p.Valor}");
        }
        return new
        {
            Tipo = "ReporteInventario", Formato,
            Impresora = _config.ImpresoraReportes.Nombre,
            Contenido = sb.ToString()
        };
    }
}

public class GastosImpreso : IReporteGastos
{
    private readonly AppConfiguracion _config;
    public string Formato => "Impresora";
    public GastosImpreso(AppConfiguracion config) => _config = config;

    public object Generar(ReporteGastos datos)
    {
        var sb = new StringBuilder();
        sb.AppendLine("         REPORTE DE GASTOS");
        sb.AppendLine("─────────────────────────────────");
        sb.AppendLine($"Total compras    : {_config.MonedaLocal}{datos.TotalCompras:N2}");
        sb.AppendLine($"Total devoluciones: {_config.MonedaLocal}{datos.TotalDevoluciones:N2}");
        sb.AppendLine($"Total gastos     : {_config.MonedaLocal}{datos.TotalGastos:N2}");
        return new { Tipo = "ReporteGastos", Formato,
            Impresora = _config.ImpresoraReportes.Nombre, Contenido = sb.ToString() };
    }
}
