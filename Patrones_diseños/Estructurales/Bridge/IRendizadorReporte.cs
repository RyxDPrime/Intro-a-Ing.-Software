// IRendizadorReporte.cs — la IMPLEMENTACIÓN del Bridge
// Define cómo se produce la salida, independientemente del tipo de reporte.
// Nuevos formatos = nuevos rendizadores. Los tipos de reporte no cambian.
//
// Diferencia clave vs Abstract Factory (patrón 18):
//   Abstract Factory: crea una familia completa nueva para cada formato.
//   Bridge: el rendizador se puede cambiar EN TIEMPO DE EJECUCIÓN
//           en un reporte ya creado — sin recrear el objeto.

using AguaMinami.Infrastructure.Config;

namespace AguaMinami.Application.Reports.Bridge;

// ── La IMPLEMENTACIÓN: cómo se renderiza ──
public interface IRendizadorReporte
{
    string NombreFormato  { get; }
    string ContentType    { get; }   // para el HTTP response

    void   AbrirDocumento(string titulo);
    void   AgregarSeccion(string nombre);
    void   AgregarFila(string etiqueta, string valor, bool resaltar = false);
    void   AgregarTabla(IEnumerable<string[]> filas, string[] encabezados);
    void   AgregarSeparador();
    string CerrarDocumento();
}

// ══════════════════════════════════════════════════
//  Rendizador 1: Pantalla — produce JSON para React
// ══════════════════════════════════════════════════
public class RendizadorPantalla : IRendizadorReporte
{
    private readonly Dictionary<string, object> _documento = new();
    private readonly List<object>              _secciones  = new();
    private          List<object>?             _seccionActual;

    public string NombreFormato => "Pantalla";
    public string ContentType   => "application/json";

    public void AbrirDocumento(string titulo)
    {
        _documento["titulo"]    = titulo;
        _documento["generadoEn"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        _documento["formato"]   = NombreFormato;
    }

    public void AgregarSeccion(string nombre)
    {
        _seccionActual = new List<object>();
        _secciones.Add(new { nombre, filas = _seccionActual });
    }

    public void AgregarFila(string etiqueta, string valor, bool resaltar = false)
    {
        _seccionActual?.Add(new { etiqueta, valor, resaltar });
    }

    public void AgregarTabla(IEnumerable<string[]> filas, string[] encabezados)
    {
        _seccionActual?.Add(new
        {
            tipo       = "tabla",
            encabezados,
            filas      = filas.ToList()
        });
    }

    public void   AgregarSeparador() { }   // pantalla no necesita separadores

    public string CerrarDocumento()
    {
        _documento["secciones"] = _secciones;
        return System.Text.Json.JsonSerializer.Serialize(_documento,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }
}

// ══════════════════════════════════════════════════
//  Rendizador 2: Texto — produce texto para impresora
//  Usa la impresora configurada en el Singleton
// ══════════════════════════════════════════════════
public class RendizadorTexto : IRendizadorReporte
{
    private readonly StringBuilder  _sb = new();
    private readonly AppConfiguracion _config;
    private const int  ANCHO = 40;
    private const string SEP = "────────────────────────────────────────";

    public string NombreFormato => "Impresora";
    public string ContentType   => "text/plain";

    public RendizadorTexto(AppConfiguracion config) => _config = config;

    public void AbrirDocumento(string titulo)
    {
        _sb.AppendLine("         AGUA MINAMI");
        _sb.AppendLine($"  {titulo.ToUpper().PadLeft((ANCHO + titulo.Length)/2)}");
        _sb.AppendLine(SEP);
        _sb.AppendLine($"Fecha   : {DateTime.Now:dd/MM/yyyy HH:mm}");
        _sb.AppendLine($"Impres. : {_config.ImpresoraReportes.Nombre}");
        _sb.AppendLine(SEP);
    }

    public void AgregarSeccion(string nombre) =>
        _sb.AppendLine($"\n{nombre.ToUpper()}:");

    public void AgregarFila(string etiqueta, string valor, bool resaltar = false)
    {
        var linea = $"  {etiqueta,-20}: {valor}";
        _sb.AppendLine(resaltar ? $"► {linea.TrimStart()}" : linea);
    }

    public void AgregarTabla(IEnumerable<string[]> filas, string[] encabezados)
    {
        _sb.AppendLine("  " + string.Join(" | ", encabezados.Select(h => h.PadRight(14))));
        _sb.AppendLine("  " + new string('-', 36));
        foreach (var fila in filas)
            _sb.AppendLine("  " + string.Join(" | ", fila.Select(c => c.PadRight(14))));
    }

    public void   AgregarSeparador() => _sb.AppendLine(SEP);
    public string CerrarDocumento()  => _sb.ToString();
}
