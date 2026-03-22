// IInventarioCommand.cs — contrato del patrón Command
// Cada acción de inventario es un objeto que sabe ejecutarse y deshacerse.
// Mapea al CU 001 "Gestionar Inventario" de tu documento:
// tipos: Entrada, Salida, Ajuste.

namespace AguaMinami.Application.Inventory.Commands;

public interface IInventarioCommand
{
    string   Descripcion  { get; }   // para el historial y auditoría
    string   Tipo         { get; }   // "Entrada" | "Salida" | "Ajuste"
    DateTime FechaCreacion { get; }

    // Ejecuta la acción sobre el inventario
    Task<ResultadoCommand> Ejecutar();

    // Revierte la acción (operación inversa)
    Task<ResultadoCommand> Deshacer();

    // Indica si este comando puede deshacerse
    bool PuedeDeshacerse { get; }
}

// ── Resultado estándar de cualquier comando ──
public class ResultadoCommand
{
    public bool   Exitoso       { get; set; }
    public string Mensaje       { get; set; } = "";
    public int    StockAnterior { get; set; }
    public int    StockNuevo    { get; set; }
    public bool   AlertaStock   { get; set; }
    public int    IdMovimiento  { get; set; }
}

// ── Datos base compartidos por todos los comandos ──
public record DatosMovimiento(
    int    IdProducto,
    string NombreProducto,
    int    Cantidad,
    string Motivo,
    string Usuario,
    int    IdAlmacen,
    int?   StockMinimoEspecifico = null
);