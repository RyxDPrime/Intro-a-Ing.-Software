// DespachoRutaNormal.cs — el despacho estándar de cada día
// Requiere chofer + ayudante + ruta asignada.
// Carga todos los productos de la lista y descuenta stock via Command.
// Mapea al caso "Salida" de tu CU 002 Registrar Ventas.

namespace AguaMinami.Application.Dispatch;

public class DespachoRutaNormal : DespachoBase
{
    private readonly IEmpleadoRepository  _empleados;
    private readonly IMovimientoRepository _movRepo;
    private readonly StockAlmacen          _almacen;

    protected override string TipoDespacho => "RutaNormal";

    public DespachoRutaNormal(
        InventarioInvoker    invoker,
        AppConfiguracion     config,
        IDespachoRepository  repo,
        IEmpleadoRepository  empleados,
        IMovimientoRepository movRepo,
        StockAlmacen          almacen)
        : base(invoker, config, repo)
    {
        _empleados = empleados;
        _movRepo   = movRepo;
        _almacen   = almacen;
    }

    // Paso 1: chofer Y ayudante son obligatorios en ruta normal
    protected override async Task ValidarPersonal(
        SolicitudDespacho sol, ResultadoDespacho res)
    {
        if (!sol.IdAyudante.HasValue)
            throw new InvalidOperationException(
                "Ruta normal requiere chofer y ayudante.");

        var chofer    = await _empleados.ObtenerAsync(sol.IdChofer);
        var ayudante  = await _empleados.ObtenerAsync(sol.IdAyudante.Value);

        if (chofer is null || !chofer.Activo)
            throw new InvalidOperationException(
                $"Chofer {sol.IdChofer} no disponible.");

        if (ayudante is null || !ayudante.Activo)
            throw new InvalidOperationException(
                $"Ayudante {sol.IdAyudante} no disponible.");

        res.Pasos.Add($"✓ Personal validado: {chofer.Nombre} + {ayudante.Nombre}");
    }

    // Paso 2: encola todos los productos como Commands de salida
    protected override async Task CargarProductos(
        SolicitudDespacho sol, ResultadoDespacho res)
    {
        foreach (var linea in sol.Productos)
        {
            var datos = new DatosMovimiento(
                linea.IdProducto, linea.NombreProducto,
                linea.Cantidad,
                $"Carga ruta {sol.IdRuta}",
                sol.Usuario, 1);

            _invoker.Encolar(new SalidaInventarioCommand(_almacen, datos, _movRepo));
        }

        var resultados = await _invoker.EjecutarCola();
        res.TotalBultos = sol.Productos.Sum(p => p.Cantidad);

        foreach (var r in resultados.Where(r => r.AlertaStock))
            res.AlertasStock.Add($"Stock bajo tras carga: {r.StockNuevo} unidades");

        res.Pasos.Add($"✓ {res.TotalBultos} bultos cargados");
    }

    // Paso 3: registra la salida en BD con código único
    protected override async Task RegistrarSalida(
        SolicitudDespacho sol, ResultadoDespacho res)
    {
        res.CodigoSalida = $"SAL-{DateTime.Now:yyyyMMddHHmm}-R{sol.IdRuta}";

        await _repo.GuardarSalidaAsync(new SalidaDespacho
        {
            Codigo      = res.CodigoSalida,
            IdChofer    = sol.IdChofer,
            IdAyudante  = sol.IdAyudante,
            IdRuta      = sol.IdRuta,
            FechaSalida = DateTime.Now,
            TotalBultos = res.TotalBultos,
            Tipo        = TipoDespacho
        });

        res.Pasos.Add($"✓ Salida registrada: {res.CodigoSalida}");
    }

    // Paso 4: notifica al sistema — el Observer ya fue activado en CargarProductos
    protected override Task NotificarSistema(
        SolicitudDespacho sol, ResultadoDespacho res)
    {
        res.Pasos.Add($"✓ Sistema notificado — {res.AlertasStock.Count} alertas generadas");
        return Task.CompletedTask;
    }
}