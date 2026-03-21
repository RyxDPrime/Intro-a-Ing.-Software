using AguaMinami.Shared.DTOs;

namespace AguaMinami.API.Repositories;

public interface IOfertaRepository
{
    Task<IEnumerable<OfertaDTO>> GetAllAsync();
    Task<OfertaDTO?> GetByIdAsync(int id);
    Task<int> CreateAsync(OfertaCreateDTO dto);
    Task<bool> UpdateAsync(int id, OfertaCreateDTO dto);
    Task<bool> DeleteAsync(int id);
}
