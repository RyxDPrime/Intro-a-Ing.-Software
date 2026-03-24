// IReporteFactory.cs — la fábrica abstracta
// Define el contrato para crear familias de reportes.
// Cada factory concreta produce todos los tipos en su propio formato.
// El Proxy (patrón 11) sigue protegiéndolos — solo el Administrador accede.

namespace AguaMinami.Application.Reports.Factories;

// ── Interfaz abstracta — la fábrica de familias ──
public interface IReporteFactory
{
    string Formato { get; }   // "Pantalla" | "Impresora" | "Excel"

    IReporteVentas     CrearReporteVentas();
    IReporteNomina     CrearReporteNomina();
    IReporteInventario CrearReporteInventario();
    IReporteGastos     CrearReporteGastos();
}

// ── Interfaces de los productos (un tipo por cada reporte) ──

public interface IReporteVentas
{
    string Formato   { get; }
    object Generar(ReporteVentas datos);
}

public interface IReporteNomina
{
    string Formato   { get; }
    object Generar(ReporteNomina datos);
}

public interface IReporteInventario
{
    string Formato   { get; }
    object Generar(ReporteInventario datos);
}

public interface IReporteGastos
{
    string Formato   { get; }
    object Generar(ReporteGastos datos);
}

// ── Selector de factory según el destino solicitado ──
public static class ReporteFactorySelector
{
    public static IReporteFactory Obtener(
        string             destino,
        AppConfiguracion  config)
    {
        return destino.ToLower() switch
        {
            "pantalla"   => new PantallaReporteFactory(),
            "impresora"  => new ImprimirReporteFactory(config),
            "excel"      => new ExcelReporteFactory(),
            _ => throw new ArgumentException(
                    $"Formato '{destino}' no soportado. " +
                    "Use: pantalla | impresora | excel")
        };
    }
}