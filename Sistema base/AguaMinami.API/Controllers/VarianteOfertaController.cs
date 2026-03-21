using Microsoft.AspNetCore.Mvc;
using AguaMinami.API.Repositories;
using AguaMinami.Shared.DTOs;

namespace AguaMinami.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VarianteOfertaController : ControllerBase
{
    private readonly IVarianteOfertaRepository _repo;

    public VarianteOfertaController(IVarianteOfertaRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(short id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VarianteOfertaCreateDTO dto)
    {
        var id = await _repo.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(short id, [FromBody] VarianteOfertaCreateDTO dto) =>
        await _repo.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(short id) =>
        await _repo.DeleteAsync(id) ? NoContent() : NotFound();
}
