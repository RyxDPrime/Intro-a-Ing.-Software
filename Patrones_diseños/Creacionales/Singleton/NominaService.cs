// ── NominaService.cs — otro módulo usando el MISMO Singleton ──
public class NominaService
{
    private readonly AppConfiguracion _config;

    public NominaService(AppConfiguracion config) => _config = config;

    public decimal CalcularSalarioDiario(decimal sueldoMensual)
    {
        // Usa el factor 23.83 del Código Laboral RD, centralizado en el Singleton
        return sueldoMensual / _config.FactorDiasLaborables;
    }

    public PrinterConfig ObtenerImpresoraVolantes() =>
        _config.ImpresoraFacturas;  // Siempre la misma instancia de impresora
}