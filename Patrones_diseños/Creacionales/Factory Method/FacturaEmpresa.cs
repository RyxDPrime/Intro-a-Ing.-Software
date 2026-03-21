
// ══════════════════════════════════════════════
//  2. FACTURA EMPRESA (con RNC, ITBIS desglosado)
//     TipoComprobante: "Persona Jurídica"
// ══════════════════════════════════════════════
public class FacturaEmpresa : IDocumento
{
    private readonly DatosFactura _datos;

    public FacturaEmpresa(DatosFactura datos) => _datos = datos;

    public string   Titulo       => "Factura — Persona Jurídica";
    public string   TipoDoc      => "FacturaEmpresa";
    public DateTime FechaEmision => DateTime.Now;

    public string Generar()
    {
        // Incluye RNC del cliente y ITBIS por línea (requerimiento fiscal RD)
        var lineas = string.Join("\n", _datos.Lineas.Select(l =>
            $"  {l.Producto,-20} x{l.Cantidad,3}  RD${l.Total,8:F2}  ITBIS:{l.Total * 0.18m,6:F2}"));

        return $"""
            AGUA MINAMI
            NCF: {_datos.NCF}
            Cliente: {_datos.NombreCliente}
            RNC    : {_datos.RncCliente}
            ─────────────────────────────────────
            {lineas}
            ─────────────────────────────────────
            Subtotal : RD${_datos.Subtotal:F2}
            ITBIS18% : RD${_datos.ITBIS:F2}
            TOTAL    : RD${_datos.Total:F2}
            """;
    }

    public void Imprimir(string nombreImpresora) =>
        PrintService.Enviar(Generar(), nombreImpresora);

    public string VistaPrevia() =>
        $"EMPRESA | RNC: {_datos.RncCliente} | NCF: {_datos.NCF} | Total: RD${_datos.Total:F2}";
}

public class FacturaEmpresaFactory : DocumentoFactory
{
    private readonly DatosFactura _datos;

    public FacturaEmpresaFactory(DatosFactura datos, AppConfiguracion config)
        : base(config) => _datos = datos;

    public override IDocumento Crear() => new FacturaEmpresa(_datos);
}
