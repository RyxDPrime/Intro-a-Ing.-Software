using Dapper;
using AguaMinami.API.Data;
using AguaMinami.Shared.DTOs;

namespace AguaMinami.API.Repositories;

public class OfertaRepository : IOfertaRepository
{
    private readonly DbConnectionFactory _db;

    public OfertaRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<OfertaDTO>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<OfertaDTO>(
            @"SELECT idOferta AS IdOferta, nombre AS Nombre, estado AS Estado,
                     fechaInicio AS FechaInicio, fechaFin AS FechaFin, descripcion AS Descripcion
              FROM Oferta ORDER BY idOferta DESC");
    }

    public async Task<OfertaDTO?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<OfertaDTO>(
            @"SELECT idOferta AS IdOferta, nombre AS Nombre, estado AS Estado,
                     fechaInicio AS FechaInicio, fechaFin AS FechaFin, descripcion AS Descripcion
              FROM Oferta WHERE idOferta = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(OfertaCreateDTO dto)
    {
        using var conn = _db.CreateConnection();
        // Obtener el próximo ID (la tabla no usa IDENTITY)
        var nextId = await conn.ExecuteScalarAsync<int>(
            "SELECT ISNULL(MAX(idOferta), 0) + 1 FROM Oferta");

        await conn.ExecuteAsync(
            @"INSERT INTO Oferta (idOferta, nombre, estado, fechaInicio, fechaFin, descripcion)
              VALUES (@Id, @Nombre, @Estado, @FechaInicio, @FechaFin, @Descripcion)",
            new { Id = nextId, dto.Nombre, dto.Estado, dto.FechaInicio, dto.FechaFin, dto.Descripcion });

        return nextId;
    }

    public async Task<bool> UpdateAsync(int id, OfertaCreateDTO dto)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE Oferta SET nombre = @Nombre, estado = @Estado,
                     fechaInicio = @FechaInicio, fechaFin = @FechaFin, descripcion = @Descripcion
              WHERE idOferta = @Id",
            new { Id = id, dto.Nombre, dto.Estado, dto.FechaInicio, dto.FechaFin, dto.Descripcion });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync("DELETE FROM Oferta WHERE idOferta = @Id", new { Id = id });
        return rows > 0;
    }
}
