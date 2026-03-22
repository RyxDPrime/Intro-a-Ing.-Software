// AppConfiguracion.cs
// Singleton thread-safe con double-check locking
// Centraliza TODA la configuración del servidor LAN de Agua Minami

namespace AguaMinami.Infrastructure.Config;

public sealed class AppConfiguracion
{
    // ── La única instancia (volatile para visibilidad entre threads) ──
    private static volatile AppConfiguracion? _instancia;
    private static readonly object _lock = new();

    // ── Constructor privado: nadie puede hacer "new AppConfiguracion()" ──
    private AppConfiguracion()
    {
        CargarConfiguracion();
    }

    // ── Punto de acceso global (thread-safe double-check) ──
    public static AppConfiguracion ObtenerInstancia()
    {
        if (_instancia is null)
        {
            lock (_lock)
            {
                _instancia ??= new AppConfiguracion();
            }
        }
        return _instancia;
    }

    // ════════════════════════════════════════════════════
    //  BASE DE DATOS LOCAL (SQL Server en red LAN)
    // ════════════════════════════════════════════════════
    public string CadenaConexion { get; private set; } = "";
    public string ServidorBD      { get; private set; } = "";
    public string NombreBaseDatos  { get; private set; } = "";
    public int    TimeoutConexion   { get; private set; }

    // ════════════════════════════════════════════════════
    //  RED LAN INTERNA (sin internet, On-Premise)
    // ════════════════════════════════════════════════════
    public string IPServidorLocal   { get; private set; } = "";
    public int    PuertoAPI          { get; private set; }
    public int    PuertoFrontend     { get; private set; }

    // ════════════════════════════════════════════════════
    //  IMPRESORAS (facturas, volantes de nómina, reportes)
    // ════════════════════════════════════════════════════
    public PrinterConfig ImpresoraFacturas  { get; private set; } = new();
    public PrinterConfig ImpresoraReportes  { get; private set; } = new();

    // ════════════════════════════════════════════════════
    //  REGLAS DE NEGOCIO GLOBALES
    // ════════════════════════════════════════════════════
    public int     StockMinimoGlobal    { get; private set; }
    public decimal FactorDiasLaborables { get; private set; } // 23.83 (Código laboral RD)
    public string  MonedaLocal          { get; private set; } = "RD$";
    public string  ZonaHoraria          { get; private set; } = "America/Santo_Domingo";

    // ── Carga desde appsettings.json al iniciar ──
    private void CargarConfiguracion()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        // Base de datos
        CadenaConexion   = config.GetConnectionString("AguaMinamiDB") ?? throw new InvalidOperationException("Cadena de conexión no encontrada");
        ServidorBD       = config["Database:Servidor"]    ?? "localhost";
        NombreBaseDatos  = config["Database:Nombre"]      ?? "AguaMinamiDB";
        TimeoutConexion  = int.Parse(config["Database:Timeout"] ?? "30");

        // Red LAN
        IPServidorLocal  = config["Red:IPServidor"]    ?? "192.168.1.100";
        PuertoAPI        = int.Parse(config["Red:PuertoAPI"]  ?? "5000");
        PuertoFrontend   = int.Parse(config["Red:PuertoWeb"]  ?? "3000");

        // Impresoras
        ImpresoraFacturas = new PrinterConfig
        {
            Nombre    = config["Impresoras:Facturas:Nombre"]    ?? "HP Inkjet",
            Tipo      = config["Impresoras:Facturas:Tipo"]      ?? "Inyeccion",
            Predeterminada = true
        };
        ImpresoraReportes = new PrinterConfig
        {
            Nombre    = config["Impresoras:Reportes:Nombre"]    ?? "HP LaserJet",
            Tipo      = config["Impresoras:Reportes:Tipo"]      ?? "Laser",
            Predeterminada = false
        };

        // Reglas de negocio
        StockMinimoGlobal    = int.Parse(config["Negocio:StockMinimo"]          ?? "10");
        FactorDiasLaborables = decimal.Parse(config["Negocio:FactorDiasLaborables"] ?? "23.83");
    }

    // ── Permite recargar config sin reiniciar el servidor ──
    public void Recargar()
    {
        lock (_lock)
        {
            CargarConfiguracion();
        }
    }
}