// RegaliaPascualStrategy.cs
// También llamada "doble sueldo" — obligatoria antes del 20 de diciembre.
// Fórmula: promedio de los sueldos pagados en el año en curso.
// Si el empleado tiene menos de 1 año, se calcula proporcional
// al tiempo trabajado (meses / 12).

namespace AguaMinami.Application.Payroll.Strategies;

public class RegaliaPascualStrategy : ICalculoNominaStrategy
{
    public string Nombre => "RegaliaPascual";

    public ResultadoNomina Calcular(DatosNomina d)
    {
        if (d.SueldosDelAnio.Count == 0)
            throw new ArgumentException(
                "Se requiere el historial de sueldos del año para " +
                "calcular la Regalía Pascual.");

        // ── 1. Promedio de los sueldos pagados en el año ──
        var promedioSueldo = d.SueldosDelAnio.Average();

        // ── 2. Si tiene menos de 1 año → proporcional ──
        var meses = d.MesesTrabajados > 0 ? d.MesesTrabajados : 12;
        var factor = meses >= 12 ? 1m : meses / 12m;

        var montoRegalía = promedioSueldo * factor;

        // ── 3. Fecha límite: 20 de diciembre del año en curso ──
        var anioActual    = DateTime.Today.Year;
        var fechaLimite   = new DateTime(anioActual, 12, 20);
        var diasRestantes = (int)(fechaLimite - DateTime.Today).TotalDays;

        return new ResultadoNomina
        {
            TipoCalculo     = Nombre,
            SueldoBase      = promedioSueldo,
            TotalDescuentos = 0m,
            SueldoNeto      = Math.Round(montoRegalía, 2),
            Desglose = new()
            {
                ["Promedio sueldos del año"] = Math.Round(promedioSueldo, 2),
                ["Meses trabajados"]         = meses,
                ["Factor proporcional"]       = Math.Round(factor, 4),
                ["Monto regalía pascual"]    = Math.Round(montoRegalía, 2),
                ["Días para fecha límite"]    = diasRestantes
            }
        };
    }
}