// ──────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/plantillas")]
[Authorize]
public class PlantillasController : ControllerBase
{
    private readonly RegistroPlantillas _registro;
    private readonly VentaFacade _facade;           // patrón 10

    // GET /api/plantillas?clienteId=5
    // Devuelve las plantillas disponibles para el cliente
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int clienteId)
    {
        await _registro.CargarPorClienteAsync(clienteId);
        var lista = _registro.ListarTodas().Select(p => new
        {
            p.Id,
            p.Nombre,
            tipo = p is PlantillaPedidoSemanal s ? $"Semanal ({s.DiaSemana})" : "Ruta"
        });
        return Ok(lista);
    }

    // POST /api/plantillas/{id}/usar
    // Clona la plantilla y abre un borrador del pedido
    [HttpPost("{id:int}/usar")]
    public async Task<IActionResult> UsarPlantilla(int id,
        [FromQuery] int clienteId)
    {
        await _registro.CargarPorClienteAsync(clienteId);

        var clon = _registro.ObtenerClon(id);
        if (clon is null)
            return NotFound($"Plantilla {id} no encontrada.");

        // El clon es independiente — aquí podemos ajustar cantidades
        // sin afectar la plantilla guardada
        var pedidoBase = clon.ConvertirAPedido();

        // Pasamos el pedido base al Facade para completarlo con el Builder,
        // validarlo con la Chain y emitir el comprobante (patrones 3, 7, 10, 21)
        var borrador = await _facade.CrearBorradorDesdePlantillaAsync(
                           pedidoBase, clon);

        return Ok(borrador);
    }

    // POST /api/plantillas/guardar-pedido/{pedidoId}
    // Guarda un pedido existente como nueva plantilla
    [HttpPost("guardar-pedido/{pedidoId:int}")]
    public async Task<IActionResult> GuardarComoPlantiilla(
        int pedidoId, [FromBody] GuardarPlantillaRequest req)
    {
        var pedido = await _facade.ObtenerPedidoAsync(pedidoId);

        var nuevaPlantilla = new PlantillaPedidoSemanal(
            id:        0,
            nombre:    req.Nombre,
            clienteId: pedido.ClienteId,
            diaSemana: req.DiaSemana ?? "Lunes",
            lineas:    pedido.Lineas.Select(l => new LineaPlantilla
            {
                ProductoId     = l.Producto.Id,
                NombreProducto = l.Producto.Nombre,
                CantidadBase   = l.Cantidad
            }).ToList()
        );

        await _registro.GuardarAsync(nuevaPlantilla);
        return Ok(new { mensaje = "Plantilla guardada.", nombre = req.Nombre });
    }
}

public class GuardarPlantillaRequest
{
    public string  Nombre    { get; set; } = "";
    public string? DiaSemana { get; set; }
}
