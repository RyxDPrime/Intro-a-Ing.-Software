using AguaMinami.Shared.DTOs;

namespace AguaMinami.API.Repositories;

public interface IOfertaDescuentoRepository
{
    Task<IEnumerable<OfertaDescuentoDTO>> GetAllAsync();
    Task<OfertaDescuentoDTO?> GetByIdAsync(int idOferta);
    Task<bool> CreateAsync(OfertaDescuentoCreateDTO dto);
    Task<bool> UpdateAsync(int idOferta, OfertaDescuentoCreateDTO dto);
    Task<bool> DeleteAsync(int idOferta);
}
