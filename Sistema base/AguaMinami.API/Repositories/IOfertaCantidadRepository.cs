using AguaMinami.Shared.DTOs;

namespace AguaMinami.API.Repositories;

public interface IOfertaCantidadRepository
{
    Task<IEnumerable<OfertaCantidadDTO>> GetAllAsync();
    Task<OfertaCantidadDTO?> GetByIdAsync(int idOferta);
    Task<bool> CreateAsync(OfertaCantidadCreateDTO dto);
    Task<bool> UpdateAsync(int idOferta, OfertaCantidadCreateDTO dto);
    Task<bool> DeleteAsync(int idOferta);
}
