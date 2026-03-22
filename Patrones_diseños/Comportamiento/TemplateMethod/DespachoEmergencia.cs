// DespachoEmergencia.cs — despacho urgente sin ayudante
// Solo el chofer, sin verificación de crédito, sin nota de entrega.
// Útil cuando un cliente llama urgente y no hay tiempo para el flujo completo.
// El mismo Procesar() de la plantilla — solo cambia el CÓMO de cada paso.

namespace AguaMinami.Application.Dispatch;

public class DespachoEmergencia : DespachoBase
{
    private readonly IEmpleadoRepository  _empleados;
    private readonly IMovimientoRepository _movRepo;
    private readonly StockAlmacen          _almacen;

    protected override string TipoDespacho => "Emergencia";

    public DespachoEmergencia(
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

    // Paso 1: solo verifica que el chofer exista — sin ayudante obligatorio
    protected override async Task ValidarPersonal(
        SolicitudDespacho sol, ResultadoDespacho res)
    {
        var chofer = await _empleados.ObtenerAsync(sol.IdChofer)
            ?? throw new InvalidOperationException("Chofer no encontrado.");

        // Sin ayudante — es emergencia, se acepta con solo el chofer
        res.Pasos.Add($"⚡ Emergencia — solo chofer: {chofer.Nombre}");

        if (!string.IsNullOrWhiteSpace(sol.Observaciones))
            res.Pasos.Add($"⚡ Motivo: {sol.Observaciones}");
    }

    // Paso 2: carga solo lo mínimo necesario — sin verificar mínimos de oferta
    protected override async Task CargarProductos(
        SolicitudDespacho sol, ResultadoDespacho res)
    {
        foreach (var linea in sol.Productos)
        {
            var datos = new DatosMovimiento(
                linea.IdProducto, linea.NombreProducto, linea.Cantidad,
                $"[EMERGENCIA] {sol.Observaciones ?? "Despacho urgente"}",
                sol.Usuario, 1);
            _invoker.Encolar(new SalidaInventarioCommand(_almacen, datos, _movRepo));
        }

        var resultados = await _invoker.EjecutarCola();
        res.TotalBultos = sol.Productos.Sum(p => p.Cantidad);

        foreach (var r in resultados.Where(r => r.AlertaStock))
            res.AlertasStock.Add($"⚡ Alerta stock en emergencia: {r.StockNuevo} uds.");

        res.Pasos.Add($"⚡ {res.TotalBultos} bultos cargados (emergencia)");
    }

    // Paso 3: registra con prefijo EMRG para distinguirla en historial
    protected override async Task RegistrarSalida(
        SolicitudDespacho sol, ResultadoDespacho res)
    {
        res.CodigoSalida = $"EMRG-{DateTime.Now:yyyyMMddHHmmss}";

        await _repo.GuardarSalidaAsync(new SalidaDespacho
        {
            Codigo       = res.CodigoSalida,
            IdChofer     = sol.IdChofer,
            IdRuta       = sol.IdRuta,
            FechaSalida  = DateTime.Now,
            TotalBultos  = res.TotalBultos,
            Tipo         = TipoDespacho,
            Observaciones = sol.Observaciones
        });

        res.Pasos.Add($"⚡ Emergencia registrada: {res.CodigoSalida}");
    }

    // Paso 4: notificación prioritaria — el Administrador debe ser informado
    protected override async Task NotificarSistema(
        SolicitudDespacho sol, ResultadoDespacho res)
    {
        await _repo.CrearNotificacionUrgente(
            $"EMERGENCIA: Salida {res.CodigoSalida} sin ayudante — {sol.Observaciones}");
        res.Pasos.Add("⚡ Administrador notificado de despacho de emergencia");
    }

    // Hook NO sobreescrito: DespachoEmergencia no necesita pasos adicionales
}