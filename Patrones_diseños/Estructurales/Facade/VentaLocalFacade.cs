// Método 2 del Facade: RegistrarSalidaRuta
// Misma fachada, distinto flujo interno - usa PedidoRutaBuilder
// y encola múltiples salidas como batch de Commands.

using AguaMinami.Application.Inventory;
using AguaMinami.Application.Inventory.Commands;
using AguaMinami.Application.Orders;
using AguaMinami.Application.Sales.Handlers;

namespace AguaMinami.Application.Sales;

public partial class VentaFacade
{

public async Task<VentaFacadeResponse> RegistrarSalidaRuta(
    SalidaRutaFacadeRequest req,
    string                  usuario,
    string                  rol)
{
    try
    {
        // ── 1. BUILDER con builder de ruta ──
        var rutaBuilder = new PedidoRutaBuilder();
        _director.CambiarBuilder(rutaBuilder);

        var codigoSalida = $"SAL-{DateTime.Now:yyyyMMddHHmm}-{req.IdRuta}";

        var pedido = _director.ConstruirSalidaRuta(
            cliente:   new ClienteDto(0, "Ruta", "Consumidor Final", null, null),
            productos: req.Productos.Select(p =>
                new ProductoDto(p.IdProducto, p.NombreProducto,
                    p.Cantidad, p.PrecioUnitario)).ToList(),
            despacho: new DespachoDto(
                req.IdChofer, req.IdAyudante, req.IdRuta, codigoSalida));

        // ── 2. CHAIN: valida auth y stock para la ruta ──
        var ctx = await _chain.Ejecutar(pedido, usuario, rol);

        // ── 3. COMMAND batch: encola todas las salidas ──
        foreach (var linea in pedido.Lineas.Where(l => !l.EsProductoGratis))
        {
            var datos = new DatosMovimiento(
                linea.IdProducto, linea.Producto, linea.Cantidad,
                $"Salida ruta {req.IdRuta} - código {codigoSalida}",
                usuario, 1);

            _invoker.Encolar(new SalidaInventarioCommand(_almacen, datos, _movRepo));
        }

        var resultados = await _invoker.EjecutarCola();
        var exitosos   = resultados.Where(r => r.Exitoso).ToList();
        var alertas    = resultados
            .Where(r => r.AlertaStock)
            .Select(r => $"Stock bajo tras salida en ruta: {r.StockNuevo} unidades")
            .ToList();

        // ── 4. Persiste la salida en BD ──
        var idSalida = await _ventaRepo.GuardarSalidaRutaAsync(pedido);

        return new VentaFacadeResponse
        {
            Exitoso       = true,
            Mensaje       = $"Salida en ruta registrada. Código: {codigoSalida}",
            IdTransaccion = idSalida,
            Total         = $"RD${pedido.Total:F2}",
            AlertasStock  = alertas,
            Advertencias  = ctx.Advertencias
        };
    }
    catch (ValidacionVentaException ex)
    {
        return new VentaFacadeResponse
            { Exitoso = false, Mensaje = $"[{ex.Codigo}] {ex.Message}" };
    }
}

// Método 3 del Facade: RegistrarEntradaRetorno
// Cuando el camion regresa de la ruta registra lo que volvio
// y actualiza el inventario con entradas (Command inverso).
public async Task<VentaFacadeResponse> RegistrarEntradaRetorno(
    EntradaRetornoRequest req,
    string               usuario)
{
    var alertas = new List<string>();

    foreach (var linea in req.ProductosRetornados)
    {
        var datos = new DatosMovimiento(
            linea.IdProducto, linea.NombreProducto, linea.Cantidad,
            $"Retorno ruta - código salida {req.CodigoSalida}",
            usuario, 1);

        var cmd = new EntradaInventarioCommand(_almacen, datos, _movRepo);
        var res = await _invoker.Ejecutar(cmd);

        if (res.AlertaStock)
            alertas.Add($"Stock bajo en '{linea.NombreProducto}'");
    }

    var idEntrada = await _ventaRepo.GuardarEntradaRetornoAsync(req.CodigoSalida);

    return new VentaFacadeResponse
    {
        Exitoso       = true,
        Mensaje       = $"Retorno de ruta registrado correctamente",
        IdTransaccion = idEntrada,
        AlertasStock  = alertas
    };
}

}