using Microsoft.AspNetCore.Mvc;
using AguaMinami.API.Repositories;
using AguaMinami.Shared.DTOs;

namespace AguaMinami.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OfertaController : ControllerBase
{
    private readonly IOfertaRepository _repo;

    public OfertaController(IOfertaRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var oferta = await _repo.GetByIdAsync(id);
        return oferta is null ? NotFound() : Ok(oferta);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OfertaCreateDTO dto)
    {
        var id = await _repo.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] OfertaCreateDTO dto) =>
        await _repo.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) =>
        await _repo.DeleteAsync(id) ? NoContent() : NotFound();
}
