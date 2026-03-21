using Dapper;
using AguaMinami.API.Data;
using AguaMinami.Shared.DTOs;

namespace AguaMinami.API.Repositories;

public class OfertaAsignacionRepository : IOfertaAsignacionRepository
{
    private readonly DbConnectionFactory _db;

    public OfertaAsignacionRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<OfertaAsignacionDTO>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<OfertaAsignacionDTO>(
            @"SELECT oa.idAsignacion AS IdAsignacion, oa.idOferta AS IdOferta,
                     oa.idEntidad AS IdEntidad, oa.idVariante AS IdVariante,
                     o.nombre AS NombreOferta
              FROM Oferta_Asignacion oa
              INNER JOIN Oferta o ON oa.idOferta = o.idOferta
              ORDER BY oa.idAsignacion DESC");
    }

    public async Task<OfertaAsignacionDTO?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<OfertaAsignacionDTO>(
            @"SELECT oa.idAsignacion AS IdAsignacion, oa.idOferta AS IdOferta,
                     oa.idEntidad AS IdEntidad, oa.idVariante AS IdVariante,
                     o.nombre AS NombreOferta
              FROM Oferta_Asignacion oa
              INNER JOIN Oferta o ON oa.idOferta = o.idOferta
              WHERE oa.idAsignacion = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(OfertaAsignacionCreateDTO dto)
    {
        using var conn = _db.CreateConnection();
        var nextId = await conn.ExecuteScalarAsync<int>(
            "SELECT ISNULL(MAX(idAsignacion), 0) + 1 FROM Oferta_Asignacion");
        await conn.ExecuteAsync(
            @"INSERT INTO Oferta_Asignacion (idAsignacion, idOferta, idEntidad, idVariante)
              VALUES (@Id, @IdOferta, @IdEntidad, @IdVariante)",
            new { Id = nextId, dto.IdOferta, dto.IdEntidad, dto.IdVariante });
        return nextId;
    }

    public async Task<bool> UpdateAsync(int id, OfertaAsignacionCreateDTO dto)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE Oferta_Asignacion SET idOferta = @IdOferta, idEntidad = @IdEntidad,
                     idVariante = @IdVariante WHERE idAsignacion = @Id",
            new { Id = id, dto.IdOferta, dto.IdEntidad, dto.IdVariante });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteAsync("DELETE FROM Oferta_Asignacion WHERE idAsignacion = @Id", new { Id = id }) > 0;
    }
}
