// ReporteServiceProxy.cs — el PROXY de control de acceso
// Implementa la misma interfaz que ReporteService.
// Antes de delegar cualquier llamada, verifica:
//   1. Que el usuario esté autenticado
//   2. Que su rol tenga permiso para ese tipo de reporte
//   3. Registra en auditoría cada acceso (exitoso o bloqueado)
//
// "El Asistente no tendrá permiso para acceder a los reportes"
// — tu documento de diseño, sección de roles.

using AguaMinami.Infrastructure.Config;
using Microsoft.Extensions.Logging;

namespace AguaMinami.Application.Reports;

public class ReporteServiceProxy : IReporteService
{
    private readonly IReporteService                   _real;
    private readonly IHttpContextAccessor               _http;
    private readonly IAuditoriaRepository               _auditoria;
    private readonly ILogger<ReporteServiceProxy>        _logger;

    // Roles con acceso a reportes — tu documento: solo Administrador
    private static readonly HashSet<string> _rolesPermitidos =
        ["Administrador"];

    // Reportes que requieren rol Administrador obligatoriamente
    private static readonly HashSet<string> _reportesRestringidos =
        ["Ventas", "Nomina", "Gastos", "Inventario"];

    public ReporteServiceProxy(
        IReporteService             real,
        IHttpContextAccessor         http,
        IAuditoriaRepository         auditoria,
        ILogger<ReporteServiceProxy>  logger)
    {
        _real      = real;
        _http      = http;
        _auditoria = auditoria;
        _logger    = logger;
    }

    // ── Método central de verificación ──
    private async Task VerificarAcceso(string tipoReporte)
    {
        var user = _http.HttpContext?.User;

        // 1. Debe estar autenticado
        if (user?.Identity?.IsAuthenticated != true)
        {
            await RegistrarAcceso(tipoReporte, "Anónimo", false);
            throw new UnauthorizedAccessException(
                "Debe iniciar sesión para acceder a reportes.");
        }

        var nombreUsuario = user.Identity.Name ?? "Desconocido";
        var rol           = user.FindFirst("role")?.Value ?? "";

        // 2. Debe tener rol permitido
        if (!_rolesPermitidos.Contains(rol))
        {
            await RegistrarAcceso(tipoReporte, nombreUsuario, false, rol);

            _logger.LogWarning(
                "[Proxy] Acceso denegado a reporte '{Tipo}' para usuario '{User}' con rol '{Rol}'",
                tipoReporte, nombreUsuario, rol);

            throw new UnauthorizedAccessException(
                $"El rol '{rol}' no tiene permiso para acceder al reporte de {tipoReporte}. " +
                "Solo el Administrador puede generar reportes financieros.");
        }

        // 3. Acceso permitido — registra el acceso exitoso
        await RegistrarAcceso(tipoReporte, nombreUsuario, true, rol);

        _logger.LogInformation(
            "[Proxy] Acceso permitido a reporte '{Tipo}' para '{User}' ({Rol})",
            tipoReporte, nombreUsuario, rol);
    }

    // Registra cada intento de acceso en la tabla de auditoría
    private async Task RegistrarAcceso(
        string tipoReporte,
        string usuario,
        bool   exitoso,
        string rol = "")
    {
        await _auditoria.GuardarAsync(new RegistroAuditoria
        {
            Tipo        = exitoso ? "REPORTE_ACCESO" : "REPORTE_BLOQUEADO",
            Descripcion = $"Reporte {tipoReporte} | Usuario: {usuario} | Rol: {rol}",
            Fecha       = DateTime.Now,
            Datos       = exitoso ? "PERMITIDO" : "DENEGADO"
        });
    }

    // ── Los cuatro métodos del proxy: verifican acceso antes de delegar ──

    public async Task<ReporteVentas> GenerarReporteVentas(FiltroReporte filtro)
    {
        await VerificarAcceso("Ventas");
        var reporte = await _real.GenerarReporteVentas(filtro);

        // El Proxy puede enriquecer la respuesta: agrega quién generó el reporte
        reporte.GeneradoPor = _http.HttpContext?.User.Identity?.Name ?? "";
        return reporte;
    }

    public async Task<ReporteNomina> GenerarReporteNomina(int anio, int mes)
    {
        await VerificarAcceso("Nomina");
        return await _real.GenerarReporteNomina(anio, mes);
    }

    public async Task<ReporteInventario> GenerarReporteInventario(FiltroReporte filtro)
    {
        await VerificarAcceso("Inventario");
        return await _real.GenerarReporteInventario(filtro);
    }

    public async Task<ReporteGastos> GenerarReporteGastos(FiltroReporte filtro)
    {
        await VerificarAcceso("Gastos");
        return await _real.GenerarReporteGastos(filtro);
    }
}