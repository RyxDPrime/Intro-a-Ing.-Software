// ExcelReporteFactory — genera CSV compatible con Excel
// Los jefes de Agua Minami pueden abrir los reportes directamente
// en Excel para hacer sus propios análisis.

namespace AguaMinami.Application.Reports.Factories;

public class ExcelReporteFactory : IReporteFactory
{
    public string Formato => "Excel";

    public IReporteVentas     CrearReporteVentas()     => new VentasExcel();
    public IReporteNomina     CrearReporteNomina()     => new NominaExcel();
    public IReporteInventario CrearReporteInventario() => new InventarioExcel();
    public IReporteGastos     CrearReporteGastos()     => new GastosExcel();
}

public class VentasExcel : IReporteVentas
{
    public string Formato => "Excel";

    public object Generar(ReporteVentas datos)
    {
        var csv = new StringBuilder();
        csv.AppendLine("sep=,");  // hint para Excel en español
        csv.AppendLine("AGUA MINAMI - REPORTE DE VENTAS");
        csv.AppendLine($"Generado,{datos.GeneradoEn:dd/MM/yyyy HH:mm}");
        csv.AppendLine($"Por,{datos.GeneradoPor}");
        csv.AppendLine();
        csv.AppendLine("RESUMEN");
        csv.AppendLine($"Total Vendido,{datos.TotalVendido}");
        csv.AppendLine($"Transacciones,{datos.TotalTransacciones}");
        csv.AppendLine();
        csv.AppendLine("VENTAS POR PRODUCTO");
        csv.AppendLine("Producto,Valor RD$");

        foreach (var v in datos.VentasPorProducto)
            csv.AppendLine($"\"{v.Descripcion}\",{v.Valor}");

        csv.AppendLine();
        csv.AppendLine("VENTAS POR RUTA");
        csv.AppendLine("Ruta,Valor RD$");
        foreach (var r in datos.VentasPorRuta)
            csv.AppendLine($"\"{r.Descripcion}\",{r.Valor}");

        return new
        {
            Tipo      = "ReporteVentas",
            Formato   = Formato,
            NombreArchivo = $"ventas_agua_minami_{DateTime.Now:yyyyMMdd}.csv",
            Contenido = csv.ToString(),
            Bytes     = Encoding.UTF8.GetBytes(csv.ToString())
        };
    }
}

public class NominaExcel : IReporteNomina
{
    public string Formato => "Excel";

    public object Generar(ReporteNomina datos)
    {
        var csv = new StringBuilder();
        csv.AppendLine("sep=,");
        csv.AppendLine("AGUA MINAMI - REPORTE DE NÓMINA");
        csv.AppendLine($"Período,{datos.Periodo}");
        csv.AppendLine();
        csv.AppendLine("RESUMEN");
        csv.AppendLine($"Total Sueldos,{datos.TotalSueldos}");
        csv.AppendLine($"Total Deducciones,{datos.TotalDeducciones}");
        csv.AppendLine($"Total Préstamos,{datos.TotalPrestamos}");
        csv.AppendLine($"Neto Pagado,{datos.TotalSueldos - datos.TotalDeducciones}");
        csv.AppendLine();
        csv.AppendLine("DETALLE POR EMPLEADO");
        csv.AppendLine("Empleado,Sueldo Neto RD$,Detalle");

        foreach (var e in datos.DetalleEmpleados)
            csv.AppendLine($"\"{e.Descripcion}\",{e.Valor},\"{e.Detalle}\"");

        return new
        {
            Tipo      = "ReporteNomina",
            Formato   = Formato,
            NombreArchivo = $"nomina_agua_minami_{datos.Periodo.Replace(" ","_")}.csv",
            Bytes     = Encoding.UTF8.GetBytes(csv.ToString())
        };
    }
}

public class InventarioExcel : IReporteInventario
{
    public string Formato => "Excel";

    public object Generar(ReporteInventario datos)
    {
        var csv = new StringBuilder();
        csv.AppendLine("sep=,");
        csv.AppendLine("AGUA MINAMI - INVENTARIO");
        csv.AppendLine($"Valor Total Stock,{datos.ValorTotalStock}");
        csv.AppendLine();
        csv.AppendLine("ALERTAS STOCK");
        csv.AppendLine("Producto,Stock Actual,Estado");
        foreach (var p in datos.ProductosBajoMinimo)
            csv.AppendLine($"\"{p.Descripcion}\",{p.Valor},BAJO MÍNIMO");
        csv.AppendLine();
        csv.AppendLine("MOVIMIENTOS");
        csv.AppendLine("Operación,Cantidad,Detalle");
        foreach (var m in datos.Movimientos)
            csv.AppendLine($"\"{m.Descripcion}\",{m.Valor},\"{m.Detalle}\"");

        return new { Tipo = "ReporteInventario", Formato,
            NombreArchivo = $"inventario_{DateTime.Now:yyyyMMdd}.csv",
            Bytes = Encoding.UTF8.GetBytes(csv.ToString()) };
    }
}

public class GastosExcel : IReporteGastos
{
    public string Formato => "Excel";

    public object Generar(ReporteGastos datos)
    {
        var csv = new StringBuilder();
        csv.AppendLine("sep=,");
        csv.AppendLine("AGUA MINAMI - GASTOS");
        csv.AppendLine($"Total Compras,{datos.TotalCompras}");
        csv.AppendLine($"Total Devoluciones,{datos.TotalDevoluciones}");
        csv.AppendLine($"Total Gastos,{datos.TotalGastos}");
        csv.AppendLine();
        csv.AppendLine("Descripción,Monto RD$,Fecha");
        foreach (var d in datos.Detalle)
            csv.AppendLine($"\"{d.Descripcion}\",{d.Valor},\"{d.Detalle}\"");

        return new { Tipo = "ReporteGastos", Formato,
            NombreArchivo = $"gastos_{DateTime.Now:yyyyMMdd}.csv",
            Bytes = Encoding.UTF8.GetBytes(csv.ToString()) };
    }
}
