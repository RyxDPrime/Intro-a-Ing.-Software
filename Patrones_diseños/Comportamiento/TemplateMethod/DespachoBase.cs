// DespachoBase.cs — la PLANTILLA del proceso de despacho
// Define el algoritmo completo con pasos fijos e invariantes.
// Las subclases implementan los pasos abstractos — nunca el orden.
// Mapea al CU de Registro de Ventas / Salida en Ruta de tu documento.

using AguaMinami.Application.Inventory.Commands;
using AguaMinami.Infrastructure.Config;

namespace AguaMinami.Application.Dispatch;

public abstract class DespachoBase
{
    protected readonly InventarioInvoker  _invoker;
    protected readonly AppConfiguracion   _config;
    protected readonly IDespachoRepository _repo;

    protected DespachoBase(
        InventarioInvoker  invoker,
        AppConfiguracion   config,
        IDespachoRepository repo)
    {
        _invoker = invoker;
        _config  = config;
        _repo    = repo;
    }

    // ════════════════════════════════════════════════════════
    //  MÉTODO PLANTILLA — sealed: el orden nunca cambia
    //  1. ValidarPersonal
    //  2. CargarProductos
    //  3. RegistrarSalida
    //  4. NotificarSistema
    // ════════════════════════════════════════════════════════
    public sealed async Task<ResultadoDespacho> Procesar(
        SolicitudDespacho solicitud)
    {
        var resultado = new ResultadoDespacho
        {
            TipoDespacho = TipoDespacho,
            FechaInicio  = DateTime.Now
        };

        try
        {
            // ── Paso 1: valida que el personal esté disponible ──
            await ValidarPersonal(solicitud, resultado);

            // ── Paso 2: carga los productos al vehículo ──
            await CargarProductos(solicitud, resultado);

            // ── Paso 3 (hook opcional): pasos adicionales por tipo ──
            await PasosAdicionalesAntesSalida(solicitud, resultado);

            // ── Paso 4: registra la salida en BD y descuenta inventario ──
            await RegistrarSalida(solicitud, resultado);

            // ── Paso 5: notifica al sistema (Observer activo) ──
            await NotificarSistema(solicitud, resultado);

            resultado.Exitoso = true;
            resultado.FechaFin = DateTime.Now;
        }
        catch (Exception ex)
        {
            resultado.Exitoso = false;
            resultado.Error   = ex.Message;
            resultado.FechaFin = DateTime.Now;
        }

        return resultado;
    }

    // ── Nombre del tipo de despacho (cada subclase lo define) ──
    protected abstract string TipoDespacho { get; }

    // ── Pasos abstractos: OBLIGATORIOS en cada subclase ──
    protected abstract Task ValidarPersonal(
        SolicitudDespacho sol, ResultadoDespacho res);

    protected abstract Task CargarProductos(
        SolicitudDespacho sol, ResultadoDespacho res);

    protected abstract Task RegistrarSalida(
        SolicitudDespacho sol, ResultadoDespacho res);

    protected abstract Task NotificarSistema(
        SolicitudDespacho sol, ResultadoDespacho res);

    // ── Hook: paso opcional — las subclases pueden sobreescribirlo ──
    // Por defecto no hace nada; DespachoRutaEspecial lo usa para
    // pedir confirmación adicional del cliente VIP.
    protected virtual Task PasosAdicionalesAntesSalida(
        SolicitudDespacho sol, ResultadoDespacho res) =>
        Task.CompletedTask;
}

// ── Modelos de entrada y salida ──
public class SolicitudDespacho
{
    public int                  IdChofer     { get; set; }
    public int?                 IdAyudante   { get; set; }
    public int                  IdRuta       { get; set; }
    public int?                 IdCliente    { get; set; }   // para ruta especial
    public string               Usuario      { get; set; } = "";
    public List<LineaVentaDto>  Productos    { get; set; } = [];
    public string?              Observaciones { get; set; }
}

public class ResultadoDespacho
{
    public bool           Exitoso       { get; set; }
    public string         TipoDespacho  { get; set; } = "";
    public string?        CodigoSalida  { get; set; }
    public string?        Error         { get; set; }
    public DateTime      FechaInicio   { get; set; }
    public DateTime      FechaFin      { get; set; }
    public List<string>  Pasos         { get; set; } = [];
    public List<string>  AlertasStock  { get; set; } = [];
    public int            TotalBultos   { get; set; }
}