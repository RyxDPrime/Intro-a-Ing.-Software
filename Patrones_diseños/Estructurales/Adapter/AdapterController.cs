[ApiController]
[Route("api/facturacion")]
public class FacturacionController : ControllerBase
{
    private readonly IComprobanteElectronico _comprobante;  // solo conoce la interfaz
    private readonly VentaFacade _facade;                   // patrón 10

    // POST /api/facturacion/emitir
    [HttpPost("emitir")]
    public async Task<IActionResult> EmitirComprobante([FromBody] EmitirRequest req)
    {
        var pedido = await _facade.ObtenerPedidoAsync(req.PedidoId);

        // El controller no sabe si está hablando con DgiiLibreriaAdapter
        // o con ComprobanteElectronicoOfflineAdapter — no le importa.
        var resultado = await _comprobante.EmitirAsync(pedido, req.TipoNcf);

        if (!resultado.Exitoso)
            return BadRequest(new { error = resultado.Error });

        return Ok(new
        {
            ncf             = resultado.NCF,
            codigoSeguridad = resultado.CodigoSeguridad,
            fechaEmision    = resultado.FechaEmision,
            offline         = resultado.NCF.StartsWith("TEMP")
        });
    }

    // POST /api/facturacion/anular
    [HttpPost("anular")]
    public async Task<IActionResult> AnularComprobante([FromBody] AnularRequest req)
    {
        var anulado = await _comprobante.AnularAsync(req.NCF, req.Razon);
        return anulado ? Ok() : BadRequest("No se pudo anular el comprobante.");
    }

    // GET /api/facturacion/estado/{ncf}
    [HttpGet("estado/{ncf}")]
    public async Task<IActionResult> ConsultarEstado(string ncf)
    {
        var estado = await _comprobante.ConsultarEstadoAsync(ncf);
        return Ok(new { ncf, estado = estado.ToString() });
    }
}
