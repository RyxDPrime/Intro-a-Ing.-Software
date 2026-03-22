// NominaContext.cs — el CONTEXTO del patrón Strategy
// Recibe cualquier ICalculoNominaStrategy y la ejecuta.
// No sabe nada de fórmulas — solo orquesta.

using AguaMinami.Infrastructure.Config;
using AguaMinami.Application.Payroll.Strategies;

namespace AguaMinami.Application.Payroll;

public class NominaContext
{
    private ICalculoNominaStrategy _strategy;
    private readonly AppConfiguracion _config;

    public NominaContext(AppConfiguracion config)
    {
        _config   = config;
        _strategy = new SueldoNormalStrategy();  // default
    }

    // Cambia la estrategia en caliente
    public void SetStrategy(ICalculoNominaStrategy strategy) =>
        _strategy = strategy;

    // Selecciona la estrategia según el nombre del botón de la UI
    public void SetStrategyPorNombre(string tipo) =>
        _strategy = tipo switch
        {
            "SueldoNormal"    => new SueldoNormalStrategy(),
            "Vacaciones"      => new VacacionesStrategy(),
            "RegaliaPascual"  => new RegaliaPascualStrategy(),
            _ => throw new ArgumentException($"Tipo desconocido: {tipo}")
        };

    // Ejecuta la estrategia activa inyectando el factor del Singleton
    public ResultadoNomina Ejecutar(DatosNomina datos)
    {
        // Siempre usa el factor del Singleton — si cambia en config, se refleja aquí
        datos.FactorDiasLaborables = _config.FactorDiasLaborables;

        return _strategy.Calcular(datos);
    }

    // Shortcut: calcula y genera el volante en un paso
    // Integra Strategy + Factory Method del patrón 2
    public (ResultadoNomina resultado, DatosVolante volante) EjecutarConVolante(
        DatosNomina datos)
    {
        var resultado = Ejecutar(datos);

        var volante = new DatosVolante(
            IdEmpleado:     datos.IdEmpleado,
            NombreEmpleado: datos.NombreEmpleado,
            Cedula:         datos.Cedula,
            Periodo:        datos.Periodo,
            SueldoBase:     resultado.SueldoBase,
            DescuentoDias:  resultado.Desglose.GetValueOrDefault("(-) Desc. por días"),
            DescuentoHoras: resultado.Desglose.GetValueOrDefault("(-) Desc. por horas"),
            CuotaPrestamo:  resultado.Desglose.GetValueOrDefault("(-) Cuota préstamo"),
            SueldoNeto:     resultado.SueldoNeto
        );

        return (resultado, volante);
    }

    public string EstrategiaActiva => _strategy.Nombre;
}