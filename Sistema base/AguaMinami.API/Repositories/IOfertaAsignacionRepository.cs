using AguaMinami.Shared.DTOs;

namespace AguaMinami.API.Repositories;

public interface IOfertaAsignacionRepository
{
    Task<IEnumerable<OfertaAsignacionDTO>> GetAllAsync();
    Task<OfertaAsignacionDTO?> GetByIdAsync(int id);
    Task<int> CreateAsync(OfertaAsignacionCreateDTO dto);
    Task<bool> UpdateAsync(int id, OfertaAsignacionCreateDTO dto);
    Task<bool> DeleteAsync(int id);
}
