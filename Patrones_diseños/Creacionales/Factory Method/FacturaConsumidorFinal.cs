
namespace AguaMinami.Application.Documents;

// ══════════════════════════════════════════════
//  1. FACTURA CONSUMIDOR FINAL
//     TipoComprobante: "Consumidor Final"
// ══════════════════════════════════════════════
public class FacturaConsumidorFinal : IDocumento
{
    private readonly DatosFactura _datos;

    public FacturaConsumidorFinal(DatosFactura datos) => _datos = datos;

    public string   Titulo       => "Factura — Consumidor Final";
    public string   TipoDoc      => "FacturaConsumidor";
    public DateTime FechaEmision => DateTime.Now;

    public string Generar()
    {
        // Sin RNC del cliente, sin desglose de ITBIS en línea
        var lineas = string.Join("\n", _datos.Lineas.Select(l =>
            $"  {l.Producto,-20} x{l.Cantidad,3}  RD${l.Total,8:F2}"));

        return $"""
            AGUA MINAMI
            NCF: {_datos.NCF}
            Cliente: {_datos.NombreCliente}
            ─────────────────────────────
            {lineas}
            ─────────────────────────────
            Subtotal : RD${_datos.Subtotal:F2}
            ITBIS    : RD${_datos.ITBIS:F2}
            TOTAL    : RD${_datos.Total:F2}
            """;
    }

    public void Imprimir(string nombreImpresora) =>
        PrintService.Enviar(Generar(), nombreImpresora);

    public string VistaPrevia() => $"CONSUMIDOR FINAL | NCF: {_datos.NCF} | Total: RD${_datos.Total:F2}";
}

// ── Factory correspondiente ──
public class FacturaConsumidorFactory : DocumentoFactory
{
    private readonly DatosFactura _datos;

    public FacturaConsumidorFactory(DatosFactura datos, AppConfiguracion config)
        : base(config) => _datos = datos;

    public override IDocumento Crear() => new FacturaConsumidorFinal(_datos);
}
