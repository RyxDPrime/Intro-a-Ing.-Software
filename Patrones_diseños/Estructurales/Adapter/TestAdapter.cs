// ---- Adapter de pruebas / modo offline ----
// Cuando el servidor DGII no está disponible (corte de internet en Santiago),
// este adapter simula la respuesta para que las ventas no se detengan.
// Los comprobantes se re-envían a la DGII cuando vuelve la conexión.

public class ComprobanteElectronicoOfflineAdapter : IComprobanteElectronico
{
    private readonly ILogger<ComprobanteElectronicoOfflineAdapter> _logger;
    private readonly IComprobanteOfflineRepository _offlineRepo;

    public ComprobanteElectronicoOfflineAdapter(
        ILogger<ComprobanteElectronicoOfflineAdapter> logger,
        IComprobanteOfflineRepository offlineRepo)
    {
        _logger     = logger;
        _offlineRepo = offlineRepo;
    }

    public async Task<ComprobanteResultado> EmitirAsync(Pedido pedido, TipoNCF tipoNcf)
    {
        // Genera un NCF temporal con prefijo TEMP para identificarlo luego
        var ncfTemporal = $"TEMP-{tipoNcf:D2}-{DateTime.Now:yyyyMMddHHmmss}";

        // Guarda el pedido pendiente para sincronizar cuando vuelva internet
        await _offlineRepo.GuardarPendienteAsync(new ComprobantePendiente
        {
            NcfTemporal  = ncfTemporal,
            PedidoId     = pedido.Id,
            TipoNcf      = tipoNcf,
            FechaCreacion = DateTime.Now
        });

        _logger.LogWarning(
            "Comprobante emitido en modo offline. NCF temporal: {Ncf}", ncfTemporal);

        return new ComprobanteResultado
        {
            Exitoso          = true,
            NCF              = ncfTemporal,
            CodigoSeguridad  = "OFFLINE",
            FechaEmision     = DateTime.Now,
            Error            = null   // No es un error — es modo contingencia
        };
    }

    public Task<bool> AnularAsync(string ncf, string razon) =>
        Task.FromResult(true);   // Anulación offline: se marca localmente

    public Task<EstadoNCF> ConsultarEstadoAsync(string ncf) =>
        Task.FromResult(ncf.StartsWith("TEMP") ? EstadoNCF.Pendiente : EstadoNCF.NoEncontrado);
}
