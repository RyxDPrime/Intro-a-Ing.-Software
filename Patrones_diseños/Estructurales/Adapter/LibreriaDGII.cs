// ---- El Adapter: traduce nuestra interfaz al SDK externo ----
public class DgiiLibreriaAdapter : IComprobanteElectronico
{
    private readonly DgiiEcfSdk.EcfClient _sdk;
    private readonly AppConfiguracion _config;   // Singleton (patrón 1)

    public DgiiLibreriaAdapter(AppConfiguracion config)
    {
        _config = config;
        // El SDK se inicializa con los datos de la empresa de Agua Minami
        _sdk = new DgiiEcfSdk.EcfClient(
            rnc:              config.RncEmpresa,
            certificadoPath:  config.RutaCertificadoDigital,
            ambiente:         config.AmbienteDgii   // "TesteCF" o "eCF"
        );
    }

    public async Task<ComprobanteResultado> EmitirAsync(Pedido pedido, TipoNCF tipoNcf)
    {
        // TRADUCCIÓN: convertir nuestro Pedido al EcfRequest del SDK
        var request = new DgiiEcfSdk.EcfRequest
        {
            TipoECF        = ((int)tipoNcf).ToString(),
            RncEmisor      = _config.RncEmpresa,
            RncComprador   = pedido.Cliente?.Rnc ?? "",
            NombreComprador = pedido.Cliente?.Nombre ?? "Consumidor Final",
            FechaEmision   = DateTime.Now,
            MontoTotal     = pedido.Total,
            Itbis          = CalcularItbis(pedido, tipoNcf),
            Ambiente       = _config.AmbienteDgii,
            Items          = pedido.Lineas.Select(l => new DgiiEcfSdk.LineaEcf
            {
                CodigoItem         = l.Producto.Codigo,
                DescripcionItem    = l.Producto.Nombre,
                CantidadItem       = l.Cantidad,
                PrecioUnitarioItem = l.PrecioNegociado > 0
                                        ? l.PrecioNegociado
                                        : l.Producto.PrecioBase,
                MontoItem          = l.Total
            }).ToList()
        };

        var response = await _sdk.GenerarECF(request);

        // TRADUCCIÓN inversa: convertir EcfResponse a nuestro ComprobanteResultado
        return new ComprobanteResultado
        {
            Exitoso          = response.Success,
            NCF              = response.CodigoNCF,
            CodigoSeguridad  = response.CodigoSeg,
            XmlFirmado       = response.XmlResponse,
            FechaEmision     = DateTime.Now,
            Error            = response.Success
                                ? null
                                : $"Error {response.CodigoError}: {response.MensajeError}"
        };
    }

    public async Task<bool> AnularAsync(string ncf, string razonAnulacion)
    {
        // La API del SDK usa un código numérico; nosotros usamos texto descriptivo
        var codigoMotivo = razonAnulacion.Contains("duplicado") ? 1
                         : razonAnulacion.Contains("dato")      ? 2
                         : 3;   // Otros motivos

        var response = await _sdk.AnularNCF(ncf, codigoMotivo);
        return response.Success;
    }

    public async Task<EstadoNCF> ConsultarEstadoAsync(string ncf)
    {
        var response = await _sdk.ConsultarNCF(_config.RncEmpresa, ncf);

        // El SDK devuelve strings; nosotros usamos enum
        return response.Estado switch
        {
            "VALIDO"    => EstadoNCF.Valido,
            "ANULADO"   => EstadoNCF.Anulado,
            "PENDIENTE" => EstadoNCF.Pendiente,
            _           => EstadoNCF.NoEncontrado
        };
    }

    private decimal CalcularItbis(Pedido pedido, TipoNCF tipo)
    {
        // Solo facturas empresariales llevan ITBIS 18% según DGII
        if (tipo != TipoNCF.FacturaEmpresa) return 0;
        return pedido.Total * 0.18m;
    }
}
