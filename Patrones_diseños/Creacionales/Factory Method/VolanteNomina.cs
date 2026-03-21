

// ══════════════════════════════════════════════
//  3. VOLANTE DE NÓMINA
//     Cálculo con factor 23.83 del Código Laboral RD
// ══════════════════════════════════════════════
public class VolanteNomina : IDocumento
{
    private readonly DatosVolante _datos;

    public VolanteNomina(DatosVolante datos) => _datos = datos;

    public string   Titulo       => "Volante de Pago";
    public string   TipoDoc      => "Volante";
    public DateTime FechaEmision => DateTime.Now;

    public string Generar()
    {
        return $"""
            AGUA MINAMI — VOLANTE DE PAGO
            Empleado : {_datos.NombreEmpleado}
            Cédula   : {_datos.Cedula}
            Período  : {_datos.Periodo}
            ──────────────────────────────────
            Sueldo base quincenal : RD${_datos.SueldoBase:F2}
            (-) Días que faltó    : RD${_datos.DescuentoDias:F2}
            (-) Horas que faltó   : RD${_datos.DescuentoHoras:F2}
            (-) Cuota préstamo    : RD${_datos.CuotaPrestamo:F2}
            ──────────────────────────────────
            SUELDO NETO           : RD${_datos.SueldoNeto:F2}
            """;
    }

    public void Imprimir(string nombreImpresora) =>
        PrintService.Enviar(Generar(), nombreImpresora);

    public string VistaPrevia() =>
        $"VOLANTE | {_datos.NombreEmpleado} | {_datos.Periodo} | Neto: RD${_datos.SueldoNeto:F2}";
}

public class VolanteNominaFactory : DocumentoFactory
{
    private readonly DatosVolante _datos;

    public VolanteNominaFactory(DatosVolante datos, AppConfiguracion config)
        : base(config) => _datos = datos;

    public override IDocumento Crear() => new VolanteNomina(_datos);
}