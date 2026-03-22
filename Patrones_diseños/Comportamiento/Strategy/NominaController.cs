// NominaController.cs — Strategy + Factory Method + Singleton juntos
// Los tres botones de la Pantalla Principal Nómina (PPN) de tu documento
// mapean directamente a los tres endpoints de este controller.

[ApiController]
[Route("api/nomina")]
[Authorize(Roles = "Administrador")]
public class NominaController : ControllerBase
{
    private readonly NominaContext     _context;
    private readonly DocumentoCreator _docCreator;
    private readonly INominaRepository _repo;

    public NominaController(
        NominaContext     context,
        DocumentoCreator docCreator,
        INominaRepository repo)
    {
        _context    = context;
        _docCreator = docCreator;
        _repo       = repo;
    }

    // POST api/nomina/calcular
    // Body: { "idEmpleado": 411, "tipo": "SueldoNormal", "diasFalto": 3, ... }
    // Mapea al botón "Sueldo Normal" de la Pantalla Principal Nómina (PPN)
    [HttpPost("calcular")]
    public async Task<IActionResult> Calcular([FromBody] CalculoRequest req)
    {
        try
        {
            // Carga datos del empleado desde la BD
            var empleado = await _repo.ObtenerEmpleadoAsync(req.IdEmpleado)
                ?? throw new KeyNotFoundException($"Empleado {req.IdEmpleado} no encontrado");

            var datos = new DatosNomina
            {
                IdEmpleado        = empleado.Id,
                NombreEmpleado    = $"{empleado.Nombre} {empleado.Apellido}",
                Cedula            = empleado.Cedula,
                SueldoMensualBase = empleado.SueldoMensual,
                FechaIngreso      = empleado.FechaIngreso,
                Periodo           = req.Periodo,

                // SueldoNormal
                DiasFalto          = req.DiasFalto,
                HorasFalto         = req.HorasFalto,
                BalancePrestamo    = empleado.BalancePrestamo,
                QuincenasRestantes = req.QuincenasRestantes,

                // Vacaciones
                FechaInicioVacaciones = req.FechaInicioVacaciones,
                DiasFeriados          = req.DiasFeriados ?? [],

                // Regalía Pascual
                SueldosDelAnio   = await _repo.ObtenerSueldosDelAnioAsync(req.IdEmpleado),
                MesesTrabajados  = req.MesesTrabajados
            };

            // ── Strategy: elige el algoritmo según el tipo solicitado ──
            _context.SetStrategyPorNombre(req.Tipo);
            var (resultado, volante) = _context.EjecutarConVolante(datos);

            // Persiste el resultado en Detalle_Nomina
            await _repo.GuardarDetalleAsync(req.IdEmpleado, req.Periodo, resultado);

            return Ok(new
            {
                Estrategia      = resultado.TipoCalculo,
                SueldoNeto      = $"RD${resultado.SueldoNeto:F2}",
                Desglose        = resultado.Desglose
                                      .ToDictionary(kv => kv.Key, kv => $"RD${kv.Value:F2}"),
                FechaFinVacaciones = resultado.FechaFinVacaciones?.ToString("dd/MM/yyyy"),
                PagarAntesDe       = resultado.PagarAntesDe?.ToString("dd/MM/yyyy"),
                DiasVacaciones     = resultado.DiasVacaciones,
                DatosVolante       = volante
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // POST api/nomina/imprimir-volante
    // Conecta Strategy con Factory Method: calcula y emite el volante impreso
    [HttpPost("imprimir-volante")]
    public async Task<IActionResult> ImprimirVolante([FromBody] ImprimirVolanteRequest req)
    {
        var detalle = await _repo.ObtenerDetalleAsync(req.IdEmpleado, req.Periodo)
            ?? throw new KeyNotFoundException("Calcula el sueldo antes de imprimir.");

        // Factory Method (patrón 2) crea el VolanteNomina correcto
        _docCreator.EmitirDocumento("Volante", detalle.ToDatosVolante());

        return Ok(new
        {
            Mensaje    = $"Volante de {detalle.NombreEmpleado} enviado a impresora",
            Impresora  = "(del Singleton AppConfiguracion)",
            Periodo    = req.Periodo
        });
    }
}

public record CalculoRequest(
    int      IdEmpleado,
    string   Tipo,                  // "SueldoNormal"|"Vacaciones"|"RegaliaPascual"
    string   Periodo,
    int      DiasFalto              = 0,
    int      HorasFalto             = 0,
    int      QuincenasRestantes     = 0,
    int      MesesTrabajados        = 12,
    DateTime? FechaInicioVacaciones = null,
    List<DateOnly>? DiasFeriados    = null
);

public record ImprimirVolanteRequest(int IdEmpleado, string Periodo);

// ── Registro en Program.cs ──
// builder.Services.AddScoped<NominaContext>();
// builder.Services.AddScoped<INominaRepository, NominaRepository>();