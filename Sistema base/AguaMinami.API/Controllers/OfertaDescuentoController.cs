using Microsoft.AspNetCore.Mvc;
using AguaMinami.API.Repositories;
using AguaMinami.Shared.DTOs;

namespace AguaMinami.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OfertaDescuentoController : ControllerBase
{
    private readonly IOfertaDescuentoRepository _repo;

    public OfertaDescuentoController(IOfertaDescuentoRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _repo.GetAllAsync());

    [HttpGet("{idOferta}")]
    public async Task<IActionResult> GetById(int idOferta)
    {
        var item = await _repo.GetByIdAsync(idOferta);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OfertaDescuentoCreateDTO dto) =>
        await _repo.CreateAsync(dto) ? Created() : BadRequest("No se pudo crear el registro.");

    [HttpPut("{idOferta}")]
    public async Task<IActionResult> Update(int idOferta, [FromBody] OfertaDescuentoCreateDTO dto) =>
        await _repo.UpdateAsync(idOferta, dto) ? NoContent() : NotFound();

    [HttpDelete("{idOferta}")]
    public async Task<IActionResult> Delete(int idOferta) =>
        await _repo.DeleteAsync(idOferta) ? NoContent() : NotFound();
}
