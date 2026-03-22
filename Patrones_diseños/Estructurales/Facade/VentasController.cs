// VentasController.cs — con el Facade, el controller es trivial.
// Antes: el controller coordinaba Builder + Chain + Command + Factory directamente.
// Ahora: el controller solo delega en el Facade y devuelve la respuesta.
// Los 10 patrones trabajan juntos — el controller no sabe nada de ellos.

[ApiController]
[Route("api/ventas")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly VentaFacade _facade;

    public VentasController(VentaFacade facade) => _facade = facade;

    // POST api/ventas/local
    // Antes: 40+ líneas coordinando subsistemas
    // Ahora: 10 líneas — el Facade hace todo
    [HttpPost("local")]
    public async Task<IActionResult> VentaLocal(
        [FromBody] VentaLocalFacadeRequest req)
    {
        var usuario = User.Identity!.Name!;
        var rol     = User.FindFirst("role")?.Value ?? "";

        var resultado = await _facade.ProcesarVentaLocal(req, usuario, rol);

        return resultado.Exitoso
            ? Ok(resultado)
            : BadRequest(resultado);
    }

    // POST api/ventas/salida-ruta
    [HttpPost("salida-ruta")]
    public async Task<IActionResult> SalidaRuta(
        [FromBody] SalidaRutaFacadeRequest req)
    {
        var usuario = User.Identity!.Name!;
        var rol     = User.FindFirst("role")?.Value ?? "";

        var resultado = await _facade.RegistrarSalidaRuta(req, usuario, rol);

        return resultado.Exitoso ? Ok(resultado) : BadRequest(resultado);
    }

    // POST api/ventas/retorno-ruta
    [HttpPost("retorno-ruta")]
    public async Task<IActionResult> RetornoRuta(
        [FromBody] EntradaRetornoRequest req)
    {
        var usuario = User.Identity!.Name!;
        var resultado = await _facade.RegistrarEntradaRetorno(req, usuario);

        return resultado.Exitoso ? Ok(resultado) : BadRequest(resultado);
    }
}