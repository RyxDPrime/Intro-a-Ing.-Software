// DespachoController.cs — el controller elige la subclase correcta
// y llama a Procesar() sin saber qué pasos internos ejecuta.
// Template Method + Command + Observer + Factory + Singleton integrados.

[ApiController]
[Route("api/despacho")]
[Authorize]
public class DespachoController : ControllerBase
{
    private readonly DespachoRutaNormal   _normal;
    private readonly DespachoRutaEspecial _especial;
    private readonly DespachoEmergencia   _emergencia;

    public DespachoController(
        DespachoRutaNormal   normal,
        DespachoRutaEspecial especial,
        DespachoEmergencia   emergencia)
    {
        _normal     = normal;
        _especial   = especial;
        _emergencia = emergencia;
    }

    // POST api/despacho/normal
    // Flujo completo: ValidarPersonal → CargarProductos → RegistrarSalida → Notificar
    [HttpPost("normal")]
    public async Task<IActionResult> DespachoNormal(
        [FromBody] SolicitudDespacho solicitud)
    {
        solicitud.Usuario = User.Identity!.Name!;
        var resultado = await _normal.Procesar(solicitud);
        return resultado.Exitoso ? Ok(resultado) : BadRequest(resultado);
    }

    // POST api/despacho/especial
    // Agrega verificación de crédito + nota de entrega (HOOK activo)
    [HttpPost("especial")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DespachoEspecial(
        [FromBody] SolicitudDespacho solicitud)
    {
        solicitud.Usuario = User.Identity!.Name!;
        var resultado = await _especial.Procesar(solicitud);
        return resultado.Exitoso ? Ok(resultado) : BadRequest(resultado);
    }

    // POST api/despacho/emergencia
    // Sin ayudante, notificación urgente al administrador
    [HttpPost("emergencia")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DespachoUrgente(
        [FromBody] SolicitudDespacho solicitud)
    {
        solicitud.Usuario = User.Identity!.Name!;
        var resultado = await _emergencia.Procesar(solicitud);
        return resultado.Exitoso ? Ok(resultado) : BadRequest(resultado);
    }
}

// ── Registro en Program.cs ──
// builder.Services.AddScoped<DespachoRutaNormal>();
// builder.Services.AddScoped<DespachoRutaEspecial>();
// builder.Services.AddScoped<DespachoEmergencia>();
// builder.Services.AddScoped<IDespachoRepository, DespachoRepository>();

/* ─── Resumen de los 12 patrones activos ────────────────────────────────
 *
 *  Singleton (1)       AppConfiguracion     → config global LAN
 *  Factory Method (2)  DocumentoCreator     → facturas y volantes
 *  Builder (3)         PedidoDirector       → pedidos paso a paso
 *  Observer (4)        StockAlmacen         → alertas stock automáticas
 *  State (5)           OrdenCompra          → ciclo de vida de ordenes
 *  Strategy (6)        NominaContext        → algoritmos de nómina RD
 *  Chain (7)           VentaChain           → validaciones en cascada
 *  Command (8)         InventarioInvoker    → movimientos + deshacer
 *  Decorator (9)       IInventarioRepository → logging + caché + validación
 *  Facade (10)         VentaFacade          → interfaz simplificada
 *  Proxy (11)          ReporteServiceProxy  → control de acceso por rol
 *  Template Method(12) DespachoBase         → proceso de despacho fijo
 * ──────────────────────────────────────────────────────────────────────── */