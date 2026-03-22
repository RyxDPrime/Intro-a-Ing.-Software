// ICalculoNominaStrategy.cs — contrato del patrón Strategy
// Todos los algoritmos de nómina implementan esta interfaz.
// NominaContext no sabe qué fórmula usa cada uno — solo llama Calcular().

namespace AguaMinami.Application.Payroll;

public interface ICalculoNominaStrategy
{
    string Nombre { get; }   // "SueldoNormal" | "Vacaciones" | "RegaliaPascual"
    ResultadoNomina Calcular(DatosNomina datos);
}

// ── Datos de entrada: lo que necesita cualquier cálculo ──
public class DatosNomina
{
    // Datos del empleado (de tu tabla Empleado + Detalle_Nomina)
    public int      IdEmpleado        { get; set; }
    public string   NombreEmpleado    { get; set; } = "";
    public string   Cedula            { get; set; } = "";
    public decimal  SueldoMensualBase { get; set; }
    public DateTime FechaIngreso      { get; set; }
    public string   Periodo          { get; set; } = ""; // "2da Quincena Febrero 2026"

    // Para SueldoNormal
    public int     DiasFalto         { get; set; }
    public int     HorasFalto        { get; set; }
    public decimal BalancePrestamo   { get; set; }
    public int     QuincenasRestantes { get; set; }

    // Para Vacaciones
    public DateTime? FechaInicioVacaciones { get; set; }
    public List<DateOnly> DiasFeriados       { get; set; } = [];

    // Para Regalía Pascual
    public List<decimal> SueldosDelAnio     { get; set; } = [];
    public int            MesesTrabajados     { get; set; }

    // Factor del Código Laboral RD (del Singleton)
    public decimal FactorDiasLaborables { get; set; } = 23.83m;
}

// ── Resultado estándar que devuelve cualquier estrategia ──
public class ResultadoNomina
{
    public string  TipoCalculo       { get; set; } = "";
    public decimal SueldoBase        { get; set; }
    public decimal TotalDescuentos   { get; set; }
    public decimal SueldoNeto        { get; set; }
    public string  Moneda            { get; set; } = "RD$";
    public DateTime FechaCalculo     { get; set; } = DateTime.Now;

    // Desglose detallado para el volante de pago
    public Dictionary<string, decimal> Desglose { get; set; } = [];

    // Fechas (para vacaciones)
    public DateTime? FechaFinVacaciones  { get; set; }
    public DateTime? PagarAntesDe        { get; set; }
    public int?      DiasVacaciones       { get; set; }
}