// IDocumento.cs — Contrato que todos los documentos deben cumplir
// Facturas, volantes de nómina, reportes: todos son IDocumento

namespace AguaMinami.Application.Documents;

public interface IDocumento
{
    string  Titulo      { get; }
    string  TipoDoc     { get; }   // "FacturaConsumidor" | "FacturaEmpresa" | "Volante"
    DateTime FechaEmision { get; }

    // Genera el contenido del documento (HTML listo para imprimir)
    string Generar();

    // Envía a la impresora configurada en AppConfiguracion (Singleton)
    void Imprimir(string nombreImpresora);

    // Vista previa como texto plano (para pantalla antes de imprimir)
    string VistaPrevia();
}

// ── DTOs de entrada para cada tipo de documento ──

public record DatosFactura(
    int             IdVenta,
    string          NombreCliente,
    string?         RncCliente,       // null si es consumidor final
    string          NCF,              // Número de Comprobante Fiscal
    List<LineaFactura> Lineas,
    decimal         Subtotal,
    decimal         ITBIS,
    decimal         Total
);

public record DatosVolante(
    int      IdEmpleado,
    string   NombreEmpleado,
    string   Cedula,
    string   Periodo,           // "2da Quincena de Febrero 2026"
    decimal  SueldoBase,
    decimal  DescuentoDias,
    decimal  DescuentoHoras,
    decimal  CuotaPrestamo,
    decimal  SueldoNeto
);

public record LineaFactura(
    string  Producto,
    int     Cantidad,
    decimal PrecioUnit,
    decimal Total
);