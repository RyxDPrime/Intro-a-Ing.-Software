// DespachoRutaEspecial.cs — despacho a cliente VIP (supermercado, gobierno)
// Mismo esqueleto que RutaNormal pero con pasos adicionales:
//   - Verifica que el cliente tenga crédito disponible
//   - Usa el HOOK PasosAdicionalesAntesSalida() para agregar confirmación
//   - Genera un documento de entrega especial via Factory Method (patrón 2)

namespace AguaMinami.Application.Dispatch;

public class DespachoRutaEspecial : DespachoBase
{
    private readonly IEmpleadoRepository  _empleados;
    private readonly IClienteRepository   _clientes;
    private readonly IMovimientoRepository _movRepo;
    private readonly StockAlmacen          _almacen;
    private readonly DocumentoCreator      _docCreator;

    protected override string TipoDespacho => "RutaEspecial";

    public DespachoRutaEspecial(
        InventarioInvoker    invoker,
        AppConfiguracion     config,
        IDespachoRepository  repo,
        IEmpleadoRepository  empleados,
        IClienteRepository   clientes,
        IMovimientoRepository movRepo,
        StockAlmacen          almacen,
        DocumentoCreator      docCreator)
        : base(invoker, config, repo)
    {
        _empleados  = empleados;
        _clientes   = clientes;
        _movRepo    = movRepo;
        _almacen    = almacen;
        _docCreator = docCreator;
    }

    // Paso 1: valida personal + cliente VIP con crédito disponible
    protected override async Task ValidarPersonal(
        SolicitudDespacho sol, ResultadoDespacho res)
    {
        if (!sol.IdCliente.HasValue)
            throw new InvalidOperationException(
                "Ruta especial requiere un cliente asignado.");

        var chofer  = await _empleados.ObtenerAsync(sol.IdChofer)
            ?? throw new InvalidOperationException("Chofer no encontrado.");

        var cliente = await _clientes.ObtenerAsync(sol.IdCliente.Value)
            ?? throw new InvalidOperationException("Cliente VIP no encontrado.");

        if (cliente.CuentasPendientes > 0)
            throw new InvalidOperationException(
                $"Cliente {cliente.Nombre} tiene cuentas pendientes. " +
                "No se puede despachar hasta regularizar.");

        res.Pasos.Add($"✓ Chofer: {chofer.Nombre} | Cliente VIP: {cliente.Nombre}");
    }

    // Paso 2: misma carga de productos que RutaNormal
    protected override async Task CargarProductos(
        SolicitudDespacho sol, ResultadoDespacho res)
    {
        foreach (var linea in sol.Productos)
        {
            var datos = new DatosMovimiento(
                linea.IdProducto, linea.NombreProducto, linea.Cantidad,
                $"Carga ruta especial cliente {sol.IdCliente}",
                sol.Usuario, 1);
            _invoker.Encolar(new SalidaInventarioCommand(_almacen, datos, _movRepo));
        }

        await _invoker.EjecutarCola();
        res.TotalBultos = sol.Productos.Sum(p => p.Cantidad);
        res.Pasos.Add($"✓ {res.TotalBultos} bultos cargados para cliente VIP");
    }

    // HOOK: paso adicional exclusivo de ruta especial
    // Genera una nota de entrega que va firmada por el cliente
    protected override async Task PasosAdicionalesAntesSalida(
        SolicitudDespacho sol, ResultadoDespacho res)
    {
        var cliente = await _clientes.ObtenerAsync(sol.IdCliente!.Value);

        // Usa Factory Method (patrón 2) para emitir la nota de entrega
        var datosNota = new DatosFactura(
            IdVenta:       0,
            NombreCliente: cliente!.Nombre,
            RncCliente:    cliente.Rnc,
            NCF:           "",
            Lineas:        sol.Productos.Select(p =>
                new LineaFactura(p.NombreProducto, p.Cantidad, p.PrecioUnitario,
                    p.Cantidad * p.PrecioUnitario)).ToList(),
            Subtotal: sol.Productos.Sum(p => p.Cantidad * p.PrecioUnitario),
            ITBIS:    0m,
            Total:    sol.Productos.Sum(p => p.Cantidad * p.PrecioUnitario)
        );

        _docCreator.EmitirDocumento("FacturaEmpresa", datosNota);
        res.Pasos.Add($"✓ Nota de entrega generada para {cliente.Nombre}");
    }

    protected override async Task RegistrarSalida(
        SolicitudDespacho sol, ResultadoDespacho res)
    {
        res.CodigoSalida = $"ESP-{DateTime.Now:yyyyMMddHHmm}-C{sol.IdCliente}";
        await _repo.GuardarSalidaAsync(new SalidaDespacho
        {
            Codigo      = res.CodigoSalida,
            IdChofer    = sol.IdChofer,
            IdRuta      = sol.IdRuta,
            IdCliente   = sol.IdCliente,
            FechaSalida = DateTime.Now,
            TotalBultos = res.TotalBultos,
            Tipo        = TipoDespacho
        });
        res.Pasos.Add($"✓ Salida especial registrada: {res.CodigoSalida}");
    }

    protected override Task NotificarSistema(
        SolicitudDespacho sol, ResultadoDespacho res)
    {
        res.Pasos.Add("✓ Despacho especial completado y notificado");
        return Task.CompletedTask;
    }
}