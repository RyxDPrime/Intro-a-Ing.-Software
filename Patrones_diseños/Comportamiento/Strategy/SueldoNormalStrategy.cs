// SueldoNormalStrategy.cs
// Fórmula exacta de tu documento (sección Nómina):
//   Salario Diario  = Sueldo Mensual Base / 23.83
//   Descuento día   = Salario Diario × días faltados
//   Descuento hora  = (Salario Diario / 8) × horas faltadas
//   Cuota préstamo  = Balance Préstamo / quincenas restantes
//   Sueldo neto     = (Sueldo Mensual / 2) - descuentos - cuota

namespace AguaMinami.Application.Payroll.Strategies;

public class SueldoNormalStrategy : ICalculoNominaStrategy
{
    public string Nombre => "SueldoNormal";

    public ResultadoNomina Calcular(DatosNomina d)
    {
        // ── 1. Base quincenal ──
        var sueldoQuincenal = d.SueldoMensualBase / 2m;

        // ── 2. Salario diario con factor del Código Laboral RD ──
        var salarioDiario = d.SueldoMensualBase / d.FactorDiasLaborables;

        // ── 3. Descuento por días que faltó ──
        var descuentoDias = salarioDiario * d.DiasFalto;

        // ── 4. Descuento por horas que faltó ──
        var salarioPorHora  = salarioDiario / 8m;
        var descuentoHoras  = salarioPorHora * d.HorasFalto;

        // ── 5. Cuota fraccionaria del préstamo ──
        var cuotaPrestamo = d.QuincenasRestantes > 0
            ? d.BalancePrestamo / d.QuincenasRestantes
            : 0m;

        // ── 6. Sueldo neto final ──
        var totalDescuentos = descuentoDias + descuentoHoras + cuotaPrestamo;
        var sueldoNeto      = sueldoQuincenal - totalDescuentos;

        if (sueldoNeto < 0m)
            throw new InvalidOperationException(
                $"El sueldo neto no puede ser negativo para {d.NombreEmpleado}. " +
                $"Descuentos (RD${totalDescuentos:F2}) superan el quincenal (RD${sueldoQuincenal:F2}).");

        return new ResultadoNomina
        {
            TipoCalculo     = Nombre,
            SueldoBase      = sueldoQuincenal,
            TotalDescuentos = totalDescuentos,
            SueldoNeto      = sueldoNeto,
            Desglose = new()
            {
                ["Sueldo quincenal base"]  = sueldoQuincenal,
                ["Salario diario"]          = Math.Round(salarioDiario, 2),
                ["(-) Desc. por días"]       = descuentoDias,
                ["(-) Desc. por horas"]      = descuentoHoras,
                ["(-) Cuota préstamo"]       = cuotaPrestamo,
                ["Sueldo neto"]              = sueldoNeto
            }
        };
    }
}