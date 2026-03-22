// HistorialController.cs — usa los iteradores sin conocer la implementación interna
// Mapea exactamente a la pantalla de historial con filtro de fecha de tu documento
// (el formulario con historial a la izquierda filtrando por fecha del CU 001).

[ApiController]
[Route("api/historial")]
[Authorize]
public class HistorialController : ControllerBase
{
    private readonly IMovimientoRepository _repo;

    public HistorialController(IMovimientoRepository repo) => _repo = repo;

    // GET api/historial/producto/3?desde=2026-01-01
    // Historial de un producto específico con filtro de fecha
    // Mapea al filtro por fecha del formulario de inventario de tu documento
    [HttpGet("producto/{idProducto}")]
    public async Task<IActionResult> HistorialProducto(
        int       idProducto,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] string?   tipo  = null,
        [FromQuery] int       page  = 1)
    {
        var historial = await HistorialMovimientos.CargarAsync(
            _repo, idProducto, desde);

        // Elige el iterador según los filtros recibidos
        IMovimientoIterator iterador;

        if (desde.HasValue || hasta.HasValue || tipo is not null)
        {
            var filtro = new FiltroMovimiento
            {
                Desde      = desde,
                Hasta      = hasta,
                Tipo       = tipo,
                IdProducto = idProducto,
                Page       = page,
                PageSize   = 20
            };
            iterador = historial.CrearIteradorFiltrado(filtro);
        }
        else
        {
            iterador = historial.CrearIteradorInverso();  // reciente primero
        }

        // Recorre con el iterador — nunca accede al array interno
        var movimientos = new List<object>();
        while (iterador.HasNext())
        {
            var m = iterador.Next();
            movimientos.Add(new
            {
                m.Id,
                m.Tipo,
                m.Cantidad,
                m.StockAnterior,
                m.StockResultante,
                m.Motivo,
                m.Usuario,
                Fecha = m.Fecha.ToString("dd/MM/yyyy HH:mm")
            });
        }

        return Ok(new
        {
            IdProducto   = idProducto,
            Total        = iterador.Total,
            Pagina       = page,
            Movimientos  = movimientos
        });
    }

    // GET api/historial/resumen/3
    // Resumen de entradas, salidas y ajustes de un producto
    // El iterador calcula las métricas mientras recorre — sin LINQ extra
    [HttpGet("resumen/{idProducto}")]
    public async Task<IActionResult> Resumen(int idProducto)
    {
        var historial = await HistorialMovimientos.CargarAsync(
            _repo, idProducto);

        var iterador = (MovimientoIterator)historial.CrearIterador();
        var resumen  = iterador.Resumir();   // un solo recorrido, todas las métricas

        return Ok(new
        {
            IdProducto       = idProducto,
            resumen.Operaciones,
            resumen.TotalEntradas,
            resumen.TotalSalidas,
            resumen.TotalAjustes,
            NetoMovimiento   = resumen.NetoMovimiento,
            Interpretacion   = resumen.NetoMovimiento >= 0
                ? $"Ganancia neta de {resumen.NetoMovimiento} unidades"
                : $"Pérdida neta de {Math.Abs(resumen.NetoMovimiento)} unidades"
        });
    }

    // GET api/historial/todos?tipo=Salida&page=2
    // Historial global con filtro por tipo — útil para auditoría
    [HttpGet("todos")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> HistorialGlobal(
        [FromQuery] string?   tipo    = null,
        [FromQuery] string?   usuario = null,
        [FromQuery] DateTime? desde   = null,
        [FromQuery] DateTime? hasta   = null,
        [FromQuery] int       page    = 1)
    {
        var historial = await HistorialMovimientos.CargarAsync(_repo);

        var filtro = new FiltroMovimiento
        {
            Tipo     = tipo,
            Usuario  = usuario,
            Desde    = desde,
            Hasta    = hasta,
            Page     = page,
            PageSize = 50
        };

        var iterador    = historial.CrearIteradorFiltrado(filtro);
        var resultados  = new List<object>();

        while (iterador.HasNext())
        {
            var m = iterador.Next();
            resultados.Add(new
            {
                m.Id, m.Tipo, m.Cantidad,
                m.IdProducto, m.NombreProducto,
                m.Usuario, m.Motivo,
                Fecha = m.Fecha.ToString("dd/MM/yyyy HH:mm")
            });
        }

        return Ok(new
        {
            TotalFiltrado = iterador.Total,
            Pagina        = page,
            Resultados    = resultados
        });
    }
}

// ── Registro en Program.cs ──
// No requiere registro adicional — HistorialMovimientos se instancia
// directamente con el método estático CargarAsync().

/* ─── 14 patrones activos ────────────────────────────────────────────────
 *  Singleton(1) · Factory Method(2) · Builder(3) · Observer(4)
 *  State(5) · Strategy(6) · Chain(7) · Command(8) · Decorator(9)
 *  Facade(10) · Proxy(11) · Template Method(12) · Composite(13)
 *  Iterator(14)
 * ──────────────────────────────────────────────────────────────────────── */