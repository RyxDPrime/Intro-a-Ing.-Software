// AjusteInventarioCommand.cs
// Corrige el stock a un valor directo (conteo físico vs sistema).
// "Ajustar inventario" — tu CU 001 Gestionar Inventario.
// Deshacer() restaura el valor anterior al ajuste.
// PuedeDeshacerse = true solo si el stock anterior sigue siendo válido.

namespace AguaMinami.Application.Inventory.Commands;

public class AjusteInventarioCommand : IInventarioCommand
{
    private readonly StockAlmacen        _almacen;
    private readonly DatosMovimiento     _datos;
    private readonly IMovimientoRepository _repo;
    private          ResultadoCommand?    _resultadoEjecucion;

    public string   Descripcion    => $"Ajuste: {_datos.NombreProducto} → {_datos.Cantidad} unidades | {_datos.Motivo}";
    public string   Tipo           => "Ajuste";
    public DateTime FechaCreacion  => DateTime.Now;
    public bool     PuedeDeshacerse => _resultadoEjecucion is not null;

    public AjusteInventarioCommand(
        StockAlmacen        almacen,
        DatosMovimiento     datos,
        IMovimientoRepository repo)
    {
        _almacen = almacen;
        _datos   = datos;
        _repo    = repo;
    }

    public async Task<ResultadoCommand> Ejecutar()
    {
        // "Ajuste" en StockAlmacen establece el valor directamente
        var resultado = await _almacen.RegistrarMovimiento(
            idProducto:    _datos.IdProducto,
            nombreProducto: _datos.NombreProducto,
            cantidad:      _datos.Cantidad,
            tipo:          "Ajuste",
            motivo:        _datos.Motivo,
            usuario:       _datos.Usuario
        );

        _resultadoEjecucion = new ResultadoCommand
        {
            Exitoso       = true,
            Mensaje       = $"Ajuste aplicado. Stock: {resultado.StockAnterior} → {resultado.StockNuevo}",
            StockAnterior = resultado.StockAnterior,
            StockNuevo    = resultado.StockNuevo,
            AlertaStock   = resultado.AlertaGenerada
        };

        return _resultadoEjecucion;
    }

    public async Task<ResultadoCommand> Deshacer()
    {
        if (_resultadoEjecucion is null)
            throw new InvalidOperationException(
                "No se puede deshacer: el ajuste no fue ejecutado aún.");

        // Restaura el stock al valor anterior al ajuste
        var resultado = await _almacen.RegistrarMovimiento(
            idProducto:    _datos.IdProducto,
            nombreProducto: _datos.NombreProducto,
            cantidad:      _resultadoEjecucion.StockAnterior,
            tipo:          "Ajuste",
            motivo:        $"[DESHACER AJUSTE] Restaurando a {_resultadoEjecucion.StockAnterior} unidades",
            usuario:       _datos.Usuario
        );

        return new ResultadoCommand
        {
            Exitoso       = true,
            Mensaje       = $"Ajuste deshecho. Stock restaurado: {resultado.StockNuevo}",
            StockAnterior = resultado.StockAnterior,
            StockNuevo    = resultado.StockNuevo
        };
    }
}