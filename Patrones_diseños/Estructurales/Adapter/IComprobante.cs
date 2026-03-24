// ---- Nuestra interfaz interna (Target) ----
// El resto del sistema SOLO conoce esta interfaz — nunca el SDK externo.
public interface IComprobanteElectronico
{
    Task<ComprobanteResultado> EmitirAsync(Pedido pedido, TipoNCF tipoNcf);
    Task<bool>                 AnularAsync(string ncf, string razonAnulacion);
    Task<EstadoNCF>            ConsultarEstadoAsync(string ncf);
}

public class ComprobanteResultado
{
    public bool    Exitoso       { get; set; }
    public string  NCF           { get; set; } = "";   // e.g. "E310000000001"
    public string  CodigoSeguridad { get; set; } = "";
    public string? Error         { get; set; }
    public DateTime FechaEmision { get; set; }
    public string  XmlFirmado    { get; set; } = "";   // XML del e-CF para auditoría
}

public enum TipoNCF
{
    FacturaConsumidorFinal = 32,   // E32 según DGII
    FacturaEmpresa         = 31,   // E31
    NotaDebito             = 33,   // E33
    NotaCredito            = 34    // E34
}

public enum EstadoNCF { Valido, Anulado, Pendiente, NoEncontrado }