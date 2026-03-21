using Dapper;
using AguaMinami.API.Data;
using AguaMinami.Shared.DTOs;

namespace AguaMinami.API.Repositories;

public class OfertaDescuentoRepository : IOfertaDescuentoRepository
{
    private readonly DbConnectionFactory _db;

    public OfertaDescuentoRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<OfertaDescuentoDTO>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<OfertaDescuentoDTO>(
            @"SELECT od.idOferta AS IdOferta, od.porcentajeDesc AS PorcentajeDesc,
                     od.montoFijo AS MontoFijo, od.topeDescuento AS TopeDescuento,
                     od.esAcumulable AS EsAcumulable, o.nombre AS NombreOferta
              FROM OfertaDescuento od
              INNER JOIN Oferta o ON od.idOferta = o.idOferta
              ORDER BY od.idOferta DESC");
    }

    public async Task<OfertaDescuentoDTO?> GetByIdAsync(int idOferta)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<OfertaDescuentoDTO>(
            @"SELECT od.idOferta AS IdOferta, od.porcentajeDesc AS PorcentajeDesc,
                     od.montoFijo AS MontoFijo, od.topeDescuento AS TopeDescuento,
                     od.esAcumulable AS EsAcumulable, o.nombre AS NombreOferta
              FROM OfertaDescuento od
              INNER JOIN Oferta o ON od.idOferta = o.idOferta
              WHERE od.idOferta = @Id", new { Id = idOferta });
    }

    public async Task<bool> CreateAsync(OfertaDescuentoCreateDTO dto)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"INSERT INTO OfertaDescuento (idOferta, porcentajeDesc, montoFijo, topeDescuento, esAcumulable)
              VALUES (@IdOferta, @PorcentajeDesc, @MontoFijo, @TopeDescuento, @EsAcumulable)", dto);
        return rows > 0;
    }

    public async Task<bool> UpdateAsync(int idOferta, OfertaDescuentoCreateDTO dto)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE OfertaDescuento SET porcentajeDesc = @PorcentajeDesc, montoFijo = @MontoFijo,
                     topeDescuento = @TopeDescuento, esAcumulable = @EsAcumulable
              WHERE idOferta = @IdOferta",
            new { IdOferta = idOferta, dto.PorcentajeDesc, dto.MontoFijo, dto.TopeDescuento, dto.EsAcumulable });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int idOferta)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteAsync("DELETE FROM OfertaDescuento WHERE idOferta = @Id", new { Id = idOferta }) > 0;
    }
}
