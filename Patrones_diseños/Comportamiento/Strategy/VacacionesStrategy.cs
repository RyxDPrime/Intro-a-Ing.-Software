// VacacionesStrategy.cs
// Implementa el Art. 177 del Código de Trabajo Dominicano
// exactamente como está en tu documento de diseño:
//
//   1–5 años trabajando  → 14 días de salario
//   +5 años trabajando   → 18 días de salario
//   Menos de 1 año (pero +5 meses) → escala proporcional:
//     +5 meses → 6 días
//     +6 meses → 7 días  ... hasta +11 meses → 12 días
//
//   Monto = Salario Diario × Días correspondientes
//   Pagar antes de = día anterior al inicio de vacaciones
//   FechaFin = inicio + días laborables (saltar domingos y feriados)

namespace AguaMinami.Application.Payroll.Strategies;

public class VacacionesStrategy : ICalculoNominaStrategy
{
    public string Nombre => "Vacaciones";

    public ResultadoNomina Calcular(DatosNomina d)
    {
        if (d.FechaInicioVacaciones is null)
            throw new ArgumentException(
                "FechaInicioVacaciones es requerida para calcular vacaciones.");

        var hoy         = DateOnly.FromDateTime(DateTime.Today);
        var fechaIngreso = DateOnly.FromDateTime(d.FechaIngreso);
        var inicio       = DateOnly.FromDateTime(d.FechaInicioVacaciones.Value);

        // ── 1. Años y meses trabajados ──
        var anios  = hoy.Year  - fechaIngreso.Year;
        var meses  = hoy.Month - fechaIngreso.Month;
        if (meses < 0) { anios--; meses += 12; }
        var totalMeses = anios * 12 + meses;

        // ── 2. Días de vacaciones según Art. 177 ──
        var diasVacaciones = DeterminarDiasVacaciones(anios, totalMeses);

        if (diasVacaciones == 0)
            throw new InvalidOperationException(
                $"{d.NombreEmpleado} tiene menos de 5 meses trabajando " +
                "y no aplica para vacaciones según el Art. 177.");

        // ── 3. Salario diario y monto de vacaciones ──
        var salarioDiario    = d.SueldoMensualBase / d.FactorDiasLaborables;
        var montoVacaciones  = salarioDiario * diasVacaciones;

        // ── 4. Fecha fin (saltando domingos y feriados) ──
        var fechaFin = CalcularFechaFin(inicio, diasVacaciones, d.DiasFeriados);

        // ── 5. Fecha límite de pago (día anterior al inicio) ──
        var pagarAntesDe = inicio.AddDays(-1);

        return new ResultadoNomina
        {
            TipoCalculo       = Nombre,
            SueldoBase        = montoVacaciones,
            TotalDescuentos   = 0m,
            SueldoNeto        = montoVacaciones,
            DiasVacaciones    = diasVacaciones,
            FechaFinVacaciones = fechaFin.ToDateTime(TimeOnly.MinValue),
            PagarAntesDe      = pagarAntesDe.ToDateTime(TimeOnly.MinValue),
            Desglose = new()
            {
                ["Salario diario"]         = Math.Round(salarioDiario, 2),
                ["Días de vacaciones"]      = diasVacaciones,
                ["Monto vacaciones"]        = Math.Round(montoVacaciones, 2),
                ["Años trabajados"]          = anios,
                ["Meses trabajados"]         = totalMeses
            }
        };
    }

    // ── Art. 177: tabla de días según antigüedad ──
    private static int DeterminarDiasVacaciones(int anios, int totalMeses)
    {
        if (anios >= 5)  return 18;     // más de 5 años → 18 días
        if (anios >= 1)  return 14;     // 1–5 años → 14 días

        // Menos de 1 año — escala proporcional por meses
        return totalMeses switch
        {
            >= 11 => 12,
            >= 10 => 11,
            >= 9  => 10,
            >= 8  => 9,
            >= 7  => 8,
            >= 6  => 7,
            >= 5  => 6,
            _      => 0    // menos de 5 meses: no aplica
        };
    }

    // ── Calcula la fecha de fin saltando domingos y feriados ──
    private static DateOnly CalcularFechaFin(
        DateOnly       inicio,
        int            diasLaborables,
        List<DateOnly> feriados)
    {
        var fecha       = inicio;
        var diasContados = 0;

        while (diasContados < diasLaborables)
        {
            // Domingos y feriados son descanso legal — no cuentan
            if (fecha.DayOfWeek != DayOfWeek.Sunday &&
                !feriados.Contains(fecha))
            {
                diasContados++;
            }
            if (diasContados < diasLaborables)
                fecha = fecha.AddDays(1);
        }

        return fecha;
    }
}