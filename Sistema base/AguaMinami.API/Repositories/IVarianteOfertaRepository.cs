using AguaMinami.Shared.DTOs;

namespace AguaMinami.API.Repositories;

public interface IVarianteOfertaRepository
{
    Task<IEnumerable<VarianteOfertaDTO>> GetAllAsync();
    Task<VarianteOfertaDTO?> GetByIdAsync(short id);
    Task<short> CreateAsync(VarianteOfertaCreateDTO dto);
    Task<bool> UpdateAsync(short id, VarianteOfertaCreateDTO dto);
    Task<bool> DeleteAsync(short id);
}
