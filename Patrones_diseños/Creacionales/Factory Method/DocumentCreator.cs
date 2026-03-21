// DocumentoCreator.cs — el "director" del patrón
// Recibe el TipoComprobante de la BD y devuelve la factory correcta.
// El controlador nunca toca los tipos concretos — solo llama a Creator.

using AguaMinami.Infrastructure.Config;

namespace AguaMinami.Application.Documents;

public class DocumentoCreator
{
    private readonly AppConfiguracion _config;

    public DocumentoCreator(AppConfiguracion config) => _config = config;

    // ── Resuelve la factory según TipoComprobante (entidad de tu BD) ──
    public DocumentoFactory ObtenerFactory(string tipoComprobante, object datos)
    {
        return tipoComprobante switch
        {
            "Consumidor Final" when datos is DatosFactura df
                => new FacturaConsumidorFactory(df, _config),

            "Persona Juridica" when datos is DatosFactura df
                => new FacturaEmpresaFactory(df, _config),

            "Volante" when datos is DatosVolante dv
                => new VolanteNominaFactory(dv, _config),

            _ => throw new ArgumentException(
                    $"Tipo de comprobante no soportado: {tipoComprobante}")
        };
    }

    // ── Shortcut: obtiene, crea e imprime en una línea ──
    public void EmitirDocumento(string tipoComprobante, object datos)
    {
        var factory  = ObtenerFactory(tipoComprobante, datos);
        var documento = factory.Crear();

        Console.WriteLine(documento.VistaPrevia());
        factory.GenerarEImprimir();
    }
}