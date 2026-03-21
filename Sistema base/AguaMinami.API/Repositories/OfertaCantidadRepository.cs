using Dapper;
using AguaMinami.API.Data;
using AguaMinami.Shared.DTOs;

namespace AguaMinami.API.Repositories;

public class OfertaCantidadRepository : IOfertaCantidadRepository
{
    private readonly DbConnectionFactory _db;

    public OfertaCantidadRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<OfertaCantidadDTO>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<OfertaCantidadDTO>(
            @"SELECT oc.idOferta AS IdOferta, oc.idVariante AS IdVariante,
                     oc.cantRequerida AS CantRequerida, oc.cantGratis AS CantGratis,
                     oc.esAcumulable AS EsAcumulable,
                     o.nombre AS NombreOferta, pv.nombreVariante AS NombreVariante
              FROM OfertaCantidad oc
              INNER JOIN Oferta o ON oc.idOferta = o.idOferta
              LEFT JOIN Producto_Variante pv ON oc.idVariante = pv.idVariante
              ORDER BY oc.idOferta DESC");
    }

    public async Task<OfertaCantidadDTO?> GetByIdAsync(int idOferta)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<OfertaCantidadDTO>(
            @"SELECT oc.idOferta AS IdOferta, oc.idVariante AS IdVariante,
                     oc.cantRequerida AS CantRequerida, oc.cantGratis AS CantGratis,
                     oc.esAcumulable AS EsAcumulable,
                     o.nombre AS NombreOferta, pv.nombreVariante AS NombreVariante
              FROM OfertaCantidad oc
              INNER JOIN Oferta o ON oc.idOferta = o.idOferta
              LEFT JOIN Producto_Variante pv ON oc.idVariante = pv.idVariante
              WHERE oc.idOferta = @Id", new { Id = idOferta });
    }

    public async Task<bool> CreateAsync(OfertaCantidadCreateDTO dto)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"INSERT INTO OfertaCantidad (idOferta, idVariante, cantRequerida, cantGratis, esAcumulable)
              VALUES (@IdOferta, @IdVariante, @CantRequerida, @CantGratis, @EsAcumulable)", dto);
        return rows > 0;
    }

    public async Task<bool> UpdateAsync(int idOferta, OfertaCantidadCreateDTO dto)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE OfertaCantidad SET idVariante = @IdVariante, cantRequerida = @CantRequerida,
                     cantGratis = @CantGratis, esAcumulable = @EsAcumulable
              WHERE idOferta = @IdOferta",
            new { IdOferta = idOferta, dto.IdVariante, dto.CantRequerida, dto.CantGratis, dto.EsAcumulable });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int idOferta)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteAsync("DELETE FROM OfertaCantidad WHERE idOferta = @Id", new { Id = idOferta }) > 0;
    }
}
