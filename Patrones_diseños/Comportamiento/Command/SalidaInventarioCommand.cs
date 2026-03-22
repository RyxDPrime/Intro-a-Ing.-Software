// SalidaInventarioCommand.cs
// Registra una salida del almacén.
// Valida stock antes de ejecutar — no permite stock negativo.
// Deshacer() registra una entrada por la misma cantidad.
// Caso típico: venta local procesada por la VentaChain (patrón Chain).

namespace AguaMinami.Application.Inventory.Commands;

public class SalidaInventarioCommand : IInventarioCommand
{
    private readonly StockAlmacen        _almacen;
    private readonly DatosMovimiento     _datos;
    private readonly IMovimientoRepository _repo;
    private          ResultadoCommand?    _resultadoEjecucion;

    public string   Descripcion    => $"Salida: {_datos.Cantidad} x {_datos.NombreProducto} | {_datos.Motivo}";
    public string   Tipo           => "Salida";
    public DateTime FechaCreacion  => DateTime.Now;
    public bool     PuedeDeshacerse => true;

    public SalidaInventarioCommand(
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
        // StockAlmacen ya valida que no quede negativo (lanza excepción si falla)
        var resultado = await _almacen.RegistrarMovimiento(
            idProducto:            _datos.IdProducto,
            nombreProducto:        _datos.NombreProducto,
            cantidad:              _datos.Cantidad,
            tipo:                  "Salida",
            motivo:                _datos.Motivo,
            usuario:               _datos.Usuario,
            stockMinimoEspecifico: _datos.StockMinimoEspecifico
        );

        _resultadoEjecucion = new ResultadoCommand
        {
            Exitoso       = true,
            Mensaje       = $"Salida registrada. Stock: {resultado.StockAnterior} → {resultado.StockNuevo}",
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
                "No se puede deshacer: el comando no fue ejecutado aún.");

        // Operación inversa: entrada por la misma cantidad
        var resultado = await _almacen.RegistrarMovimiento(
            idProducto:    _datos.IdProducto,
            nombreProducto: _datos.NombreProducto,
            cantidad:      _datos.Cantidad,
            tipo:          "Entrada",
            motivo:        $"[DESHACER] {_datos.Motivo}",
            usuario:       _datos.Usuario
        );

        return new ResultadoCommand
        {
            Exitoso       = true,
            Mensaje       = $"Salida deshecha. Stock: {resultado.StockAnterior} → {resultado.StockNuevo}",
            StockAnterior = resultado.StockAnterior,
            StockNuevo    = resultado.StockNuevo
        };
    }
}