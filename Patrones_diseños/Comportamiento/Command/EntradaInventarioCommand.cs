// EntradaInventarioCommand.cs
// Registra una entrada de mercancía al almacén.
// Deshacer() registra una salida por la misma cantidad.
// Caso típico: llegó la mercancía de una OrdenCompra (patrón State).

using AguaMinami.Application.Inventory;

namespace AguaMinami.Application.Inventory.Commands;

public class EntradaInventarioCommand : IInventarioCommand
{
    private readonly StockAlmacen        _almacen;
    private readonly DatosMovimiento     _datos;
    private readonly IMovimientoRepository _repo;

    // Guardamos el resultado para poder deshacerlo
    private ResultadoCommand? _resultadoEjecucion;

    public string   Descripcion   => $"Entrada: {_datos.Cantidad} x {_datos.NombreProducto} | {_datos.Motivo}";
    public string   Tipo          => "Entrada";
    public DateTime FechaCreacion => DateTime.Now;
    public bool     PuedeDeshacerse => true;

    public EntradaInventarioCommand(
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
        // Usa el StockAlmacen del patrón Observer — notifica si sube del mínimo
        var resultado = await _almacen.RegistrarMovimiento(
            idProducto:              _datos.IdProducto,
            nombreProducto:          _datos.NombreProducto,
            cantidad:                _datos.Cantidad,
            tipo:                    "Entrada",
            motivo:                  _datos.Motivo,
            usuario:                 _datos.Usuario,
            stockMinimoEspecifico:   _datos.StockMinimoEspecifico
        );

        _resultadoEjecucion = new ResultadoCommand
        {
            Exitoso       = true,
            Mensaje       = $"Entrada registrada. Stock: {resultado.StockAnterior} → {resultado.StockNuevo}",
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

        // Operación inversa: salida por la misma cantidad
        var resultado = await _almacen.RegistrarMovimiento(
            idProducto:    _datos.IdProducto,
            nombreProducto: _datos.NombreProducto,
            cantidad:      _datos.Cantidad,
            tipo:          "Salida",
            motivo:        $"[DESHACER] {_datos.Motivo}",
            usuario:       _datos.Usuario
        );

        return new ResultadoCommand
        {
            Exitoso       = true,
            Mensaje       = $"Entrada deshecha. Stock: {resultado.StockAnterior} → {resultado.StockNuevo}",
            StockAnterior = resultado.StockAnterior,
            StockNuevo    = resultado.StockNuevo
        };
    }
}