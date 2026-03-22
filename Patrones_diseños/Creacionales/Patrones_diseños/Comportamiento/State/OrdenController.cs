// OrdenesController.cs — expone las transiciones de estado como endpoints REST
// El frontend React habilita/deshabilita botones según AccionesPermitidas[]

[ApiController]
[Route("api/ordenes")]
[Authorize]
public class OrdenesController : ControllerBase
{
    private readonly OrdenCompraService _servicio;

    public OrdenesController(OrdenCompraService servicio) =>
        _servicio = servicio;

    // GET api/ordenes/42 — devuelve estado actual y acciones permitidas
    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var orden = await _servicio.ObtenerAsync(id);
        return Ok(new
        {
            orden.Id,
            orden.EstadoNombre,
            orden.AccionesPermitidas,   // React usa esto para habilitar botones
            orden.CantidadSolicitada,
            orden.CostoTotal,
            orden.FechaEntregaEstimada,
            orden.RecepcionParcial,
            Historial = orden.Historial.Select(h => new
            {
                h.EstadoAnterior,
                h.EstadoNuevo,
                h.Usuario,
                h.Motivo,
                Fecha = h.Fecha.ToString("dd/MM/yyyy HH:mm")
            })
        });
    }

    // POST api/ordenes/42/procesar — contable aprueba
    [HttpPost("{id}/procesar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Procesar(int id)
    {
        try
        {
            var usuario = User.Identity!.Name!;
            await _servicio.ProcesarAsync(id, usuario);
            return Ok(new { Mensaje = "Orden en proceso", EstadoNuevo = "EnProceso" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // POST api/ordenes/42/recibir — llegó la mercancía
    [HttpPost("{id}/recibir")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Recibir(
        int id, [FromBody] RecepcionRequest req)
    {
        try
        {
            var usuario = User.Identity!.Name!;
            await _servicio.RecibirAsync(id, req.CantidadRecibida, usuario, req.NombreProducto);

            return Ok(new
            {
                Mensaje         = "Mercancía recibida y stock actualizado",
                EstadoNuevo     = "Recibida",
                CantidadIngresada = req.CantidadRecibida
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // POST api/ordenes/42/cancelar — rechazada
    [HttpPost("{id}/cancelar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Cancelar(
        int id, [FromBody] CancelacionRequest req)
    {
        try
        {
            var usuario = User.Identity!.Name!;
            await _servicio.CancelarAsync(id, req.Motivo, usuario);
            return Ok(new { Mensaje = "Orden cancelada", EstadoNuevo = "Cancelada" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}

public record RecepcionRequest(int CantidadRecibida, string NombreProducto);
public record CancelacionRequest(string Motivo);

// ── Registro en Program.cs ──
// builder.Services.AddScoped<OrdenCompraService>();
// builder.Services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();