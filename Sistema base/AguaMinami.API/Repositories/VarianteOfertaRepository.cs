using Dapper;
using AguaMinami.API.Data;
using AguaMinami.Shared.DTOs;

namespace AguaMinami.API.Repositories;

public class VarianteOfertaRepository : IVarianteOfertaRepository
{
    private readonly DbConnectionFactory _db;

    public VarianteOfertaRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<VarianteOfertaDTO>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<VarianteOfertaDTO>(
            @"SELECT idVariante AS IdVariante, nombre AS Nombre, descripcion AS Descripcion
              FROM Variantes_Oferta ORDER BY idVariante DESC");
    }

    public async Task<VarianteOfertaDTO?> GetByIdAsync(short id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<VarianteOfertaDTO>(
            @"SELECT idVariante AS IdVariante, nombre AS Nombre, descripcion AS Descripcion
              FROM Variantes_Oferta WHERE idVariante = @Id", new { Id = id });
    }

    public async Task<short> CreateAsync(VarianteOfertaCreateDTO dto)
    {
        using var conn = _db.CreateConnection();
        var nextId = await conn.ExecuteScalarAsync<short>(
            "SELECT ISNULL(MAX(idVariante), 0) + 1 FROM Variantes_Oferta");
        await conn.ExecuteAsync(
            @"INSERT INTO Variantes_Oferta (idVariante, nombre, descripcion)
              VALUES (@Id, @Nombre, @Descripcion)",
            new { Id = nextId, dto.Nombre, dto.Descripcion });
        return nextId;
    }

    public async Task<bool> UpdateAsync(short id, VarianteOfertaCreateDTO dto)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE Variantes_Oferta SET nombre = @Nombre, descripcion = @Descripcion
              WHERE idVariante = @Id",
            new { Id = id, dto.Nombre, dto.Descripcion });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(short id)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteAsync(
            "DELETE FROM Variantes_Oferta WHERE idVariante = @Id", new { Id = id }) > 0;
    }
}
