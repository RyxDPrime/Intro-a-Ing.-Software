// StockAlmacen.cs — el SUJETO observado
// Cuando RegistrarMovimiento() baja el stock del mínimo configurado
// en el Singleton, llama a Notificar() y todos los observers reaccionan.
// Mapea directamente a Stock_Almacen y Movimiento_Inventario de tu BD.

using AguaMinami.Infrastructure.Config;

namespace AguaMinami.Application.Inventory;

public class StockAlmacen : IStockObservable
{
    private readonly List<IStockObserver> _observers = [];
    private readonly AppConfiguracion         _config;
    private readonly IMovimientoRepository     _repo;

    // Estado interno: stock actual por producto en este almacén
    private readonly Dictionary<int, int> _stockActual = [];

    public int    IdAlmacen { get; }
    public string Nombre    { get; }

    public StockAlmacen(
        int                    idAlmacen,
        string                 nombre,
        AppConfiguracion       config,
        IMovimientoRepository  repo)
    {
        IdAlmacen = idAlmacen;
        Nombre    = nombre;
        _config   = config;
        _repo     = repo;
    }

    // ── Gestión de observers ──
    public void Suscribir(IStockObserver observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public void Desuscribir(IStockObserver observer) =>
        _observers.Remove(observer);

    // Notifica a TODOS los observers en paralelo
    public async Task Notificar(StockBajoEvento evento)
    {
        var tareas = _observers.Select(o => o.OnStockBajo(evento));
        await Task.WhenAll(tareas);
    }

    // ════════════════════════════════════════════════════════
    //  OPERACIÓN PRINCIPAL: registra un movimiento de inventario
    //  Tipos: "Entrada" | "Salida" | "Ajuste"
    //  Mapea al caso de uso 001 Gestionar Inventario de tu documento
    // ════════════════════════════════════════════════════════
    public async Task<MovimientoResultado> RegistrarMovimiento(
        int    idProducto,
        string nombreProducto,
        int    cantidad,
        string tipo,            // "Entrada" | "Salida" | "Ajuste"
        string motivo,
        string usuario,
        int?   stockMinimoEspecifico = null)
    {
        // 1. Calcular nuevo stock
        var stockAnterior = _stockActual.GetValueOrDefault(idProducto, 0);
        var nuevoStock = tipo switch
        {
            "Entrada" => stockAnterior + cantidad,
            "Salida"  => stockAnterior - cantidad,
            "Ajuste"  => cantidad,   // ajuste directo al valor
            _          => throw new ArgumentException($"Tipo inválido: {tipo}")
        };

        if (nuevoStock < 0)
            throw new InvalidOperationException(
                $"Stock insuficiente para {nombreProducto}. " +
                $"Disponible: {stockAnterior}, solicitado: {cantidad}");

        // 2. Persistir el movimiento en la BD
        var movimiento = new MovimientoInventario
        {
            IdProducto    = idProducto,
            IdAlmacen     = IdAlmacen,
            Tipo          = tipo,
            Cantidad      = cantidad,
            StockAnterior = stockAnterior,
            StockResultante = nuevoStock,
            Motivo        = motivo,
            Usuario       = usuario,
            Fecha         = DateTime.Now
        };
        await _repo.GuardarMovimientoAsync(movimiento);

        // 3. Actualizar stock en memoria
        _stockActual[idProducto] = nuevoStock;

        // 4. Verificar si el stock bajó del mínimo configurado
        //    Usa el Singleton para el valor global, o uno específico por producto
        var minimo = stockMinimoEspecifico ?? _config.StockMinimoGlobal;

        if (nuevoStock <= minimo && tipo != "Entrada")
        {
            var evento = new StockBajoEvento(
                IdProducto:         idProducto,
                NombreProducto:     nombreProducto,
                StockActual:        nuevoStock,
                StockMinimo:        minimo,
                IdAlmacen:          IdAlmacen,
                TipoMovimiento:     tipo,
                UsuarioResponsable: usuario,
                FechaEvento:        DateTime.Now
            );

            // Notifica a TODOS los observers registrados
            await Notificar(evento);
        }

        return new MovimientoResultado(
            StockAnterior:   stockAnterior,
            StockNuevo:      nuevoStock,
            AlertaGenerada:  nuevoStock <= minimo && tipo != "Entrada"
        );
    }

    // Carga el stock inicial desde la BD al arrancar
    public async Task InicializarStockAsync()
    {
        var stocks = await _repo.ObtenerStockActualAsync(IdAlmacen);
        foreach (var s in stocks)
            _stockActual[s.IdProducto] = s.Cantidad;
    }
}

public record MovimientoResultado(
    int  StockAnterior,
    int  StockNuevo,
    bool AlertaGenerada);