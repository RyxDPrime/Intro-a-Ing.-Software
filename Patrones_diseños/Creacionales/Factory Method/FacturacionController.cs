// FacturacionController.cs — cómo lo usa el endpoint real
// El controlador no sabe nada de FacturaConsumidorFinal ni VolanteNomina.
// Solo habla con DocumentoCreator → principio Open/Closed cumplido.

[ApiController]
[Route("api/[controller]")]
public class FacturacionController : ControllerBase
{
    private readonly DocumentoCreator _creator;
    private readonly IVentaService    _ventas;
    private readonly INominaService   _nomina;

    public FacturacionController(
        DocumentoCreator creator,
        IVentaService    ventas,
        INominaService   nomina)
    {
        _creator = creator;
        _ventas  = ventas;
        _nomina  = nomina;
    }

    // POST api/facturacion/emitir
    // Body: { "idVenta": 42, "tipoComprobante": "Consumidor Final" }
    [HttpPost("emitir")]
    public async Task<IActionResult> EmitirFactura([FromBody] EmitirFacturaRequest req)
    {
        var venta = await _ventas.ObtenerAsync(req.IdVenta);
        if (venta is null) return NotFound();

        var datos = new DatosFactura(
            IdVenta:       venta.Id,
            NombreCliente: venta.NombreCliente,
            RncCliente:    venta.RncCliente,
            NCF:           venta.NCF,
            Lineas:        venta.Lineas.Select(l => new LineaFactura(
                               l.Producto, l.Cantidad, l.PrecioUnit, l.Total)).ToList(),
            Subtotal:      venta.Subtotal,
            ITBIS:         venta.ITBIS,
            Total:         venta.Total
        );

        // El Creator elige la factory correcta según tipoComprobante.
        // Si mañana agregan "Gobierno" o "Zona Franca", solo se añade
        // un nuevo case en DocumentoCreator — el controlador no cambia.
        var factory  = _creator.ObtenerFactory(req.TipoComprobante, datos);
        var documento = factory.Crear();

        return Ok(new
        {
            VistaPrevia  = documento.VistaPrevia(),
            Tipo         = documento.TipoDoc,
            FechaEmision = documento.FechaEmision
        });
    }

    // POST api/facturacion/volante-nomina
    // Emite el volante de pago de un empleado
    [HttpPost("volante-nomina")]
    public async Task<IActionResult> EmitirVolante([FromBody] int idDetalleNomina)
    {
        var detalle = await _nomina.ObtenerDetalleAsync(idDetalleNomina);
        if (detalle is null) return NotFound();

        var datos = new DatosVolante(
            IdEmpleado:      detalle.IdEmpleado,
            NombreEmpleado:  detalle.NombreEmpleado,
            Cedula:          detalle.Cedula,
            Periodo:         detalle.Periodo,
            SueldoBase:      detalle.SueldoBase,
            DescuentoDias:   detalle.DescuentoDias,
            DescuentoHoras:  detalle.DescuentoHoras,
            CuotaPrestamo:   detalle.CuotaPrestamo,
            SueldoNeto:      detalle.SueldoNeto
        );

        _creator.EmitirDocumento("Volante", datos);

        return Ok($"Volante de {detalle.NombreEmpleado} enviado a impresora");
    }
}

// ── Registro en Program.cs ──
// builder.Services.AddScoped<DocumentoCreator>();
// builder.Services.AddScoped<IVentaService, VentaService>();
// builder.Services.AddScoped<INominaService, NominaService>();

public record EmitirFacturaRequest(int IdVenta, string TipoComprobante);