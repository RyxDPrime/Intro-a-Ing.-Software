// PantallaReporteFactory — genera reportes en formato JSON para React
// Incluye datos estructurados con colores para gráficos y KPIs.

namespace AguaMinami.Application.Reports.Factories;

public class PantallaReporteFactory : IReporteFactory
{
    public string Formato => "Pantalla";

    public IReporteVentas     CrearReporteVentas()     => new VentasPantalla();
    public IReporteNomina     CrearReporteNomina()     => new NominaPantalla();
    public IReporteInventario CrearReporteInventario() => new InventarioPantalla();
    public IReporteGastos     CrearReporteGastos()     => new GastosPantalla();
}

public class VentasPantalla : IReporteVentas
{
    public string Formato => "Pantalla";

    public object Generar(ReporteVentas datos) => new
    {
        Tipo           = "ReporteVentas",
        Formato        = Formato,
        Titulo         = "Reporte de Ventas",
        GeneradoEn     = datos.GeneradoEn.ToString("dd/MM/yyyy HH:mm"),
        GeneradoPor    = datos.GeneradoPor,
        KPIs = new
        {
            TotalVendido       = $"RD${datos.TotalVendido:N2}",
            TotalTransacciones = datos.TotalTransacciones,
            PromedioVenta      = datos.TotalTransacciones > 0
                ? $"RD${datos.TotalVendido / datos.TotalTransacciones:N2}"
                : "RD$0.00"
        },
        // Datos para gráfico de barras en React
        GraficoProductos = datos.VentasPorProducto.Select(v => new
        {
            label = v.Descripcion,
            value = v.Valor,
            color = v.Valor > 10000 ? "#1D9E75" : "#378ADD"
        }),
        GraficoRutas = datos.VentasPorRuta.Select(v => new
        {
            label = v.Descripcion,
            value = v.Valor
        })
    };
}

public class NominaPantalla : IReporteNomina
{
    public string Formato => "Pantalla";

    public object Generar(ReporteNomina datos) => new
    {
        Tipo     = "ReporteNomina",
        Formato  = Formato,
        Periodo  = datos.Periodo,
        KPIs = new
        {
            TotalSueldos     = $"RD${datos.TotalSueldos:N2}",
            TotalDeducciones = $"RD${datos.TotalDeducciones:N2}",
            TotalPrestamos   = $"RD${datos.TotalPrestamos:N2}",
            NetoPagado       = $"RD${datos.TotalSueldos - datos.TotalDeducciones:N2}"
        },
        Empleados = datos.DetalleEmpleados.Select(e => new
        {
            Nombre  = e.Descripcion,
            Neto    = $"RD${e.Valor:N2}",
            Detalle = e.Detalle
        })
    };
}

public class InventarioPantalla : IReporteInventario
{
    public string Formato => "Pantalla";

    public object Generar(ReporteInventario datos) => new
    {
        Tipo            = "ReporteInventario",
        Formato         = Formato,
        ValorTotalStock = $"RD${datos.ValorTotalStock:N2}",
        AlertasCriticas = datos.ProductosBajoMinimo.Count,
        ProductosBajoMinimo = datos.ProductosBajoMinimo.Select(p => new
        {
            Producto    = p.Descripcion,
            StockActual = (int)p.Valor,
            Alerta      = true
        }),
        UltimosMovimientos = datos.Movimientos.Take(10).Select(m => new
        {
            Operacion = m.Descripcion,
            Cantidad  = m.Valor,
            Detalle   = m.Detalle
        })
    };
}

public class GastosPantalla : IReporteGastos
{
    public string Formato => "Pantalla";

    public object Generar(ReporteGastos datos) => new
    {
        Tipo             = "ReporteGastos",
        Formato          = Formato,
        TotalGastos      = $"RD${datos.TotalGastos:N2}",
        TotalCompras     = $"RD${datos.TotalCompras:N2}",
        TotalDevoluciones= $"RD${datos.TotalDevoluciones:N2}",
        Detalle          = datos.Detalle.Select(d => new
        {
            Descripcion = d.Descripcion,
            Monto       = $"RD${d.Valor:N2}",
            Fecha       = d.Detalle
        })
    };
}