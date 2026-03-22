// DocumentoFactory.cs — Clase abstracta base del patrón
// Define el "Factory Method": Crear()
// Las subclases deciden QUÉ tipo concreto de documento fabrican

using AguaMinami.Infrastructure.Config;

namespace AguaMinami.Application.Documents;

public abstract class DocumentoFactory
{
    // Singleton inyectado: sabe qué impresora usar
    protected readonly AppConfiguracion _config;

    protected DocumentoFactory(AppConfiguracion config)
    {
        _config = config;
    }

    // ── EL Factory Method: cada subclase lo implementa ──
    public abstract IDocumento Crear();

    // ── Operación de plantilla: genera E imprime de una vez ──
    // Llama a Crear() internamente → no sabe qué tipo exacto devuelve
    public void GenerarEImprimir()
    {
        var documento = Crear();                         // Factory Method
        var contenido = documento.Generar();
        var impresora = ObtenerImpresoraPorTipo(documento.TipoDoc);
        documento.Imprimir(impresora);

        Console.WriteLine($"[{documento.TipoDoc}] Enviado a: {impresora}");
    }

    // Decide la impresora según el tipo de documento (del Singleton)
    private string ObtenerImpresoraPorTipo(string tipo) => tipo switch
    {
        "FacturaConsumidor" or "FacturaEmpresa" => _config.ImpresoraFacturas.Nombre,
        "Volante"                                 => _config.ImpresoraFacturas.Nombre,
        _                                         => _config.ImpresoraReportes.Nombre
    };
}

// ── DocumentoCreator.cs — decide QUÉ factory usar según TipoComprobante ──
// (ver pestaña "DocumentoCreator.cs")