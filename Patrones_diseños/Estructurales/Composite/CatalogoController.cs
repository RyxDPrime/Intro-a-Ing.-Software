// CatalogoController.cs — el cliente del Composite
// Llama a las operaciones recursivas sin saber si habla con una hoja
// o una categoría. El árbol se recorre solo internamente.

[ApiController]
[Route("api/catalogo")]
[Authorize]
public class CatalogoController : ControllerBase
{
    private readonly CatalogoBuilder _builder;

    public CatalogoController(CatalogoBuilder builder) => _builder = builder;

    // GET api/catalogo
    // Devuelve el árbol completo con valor total de stock
    [HttpGet]
    public async Task<IActionResult> ObtenerCatalogo()
    {
        var raiz = await _builder.ConstruirAsync();

        // Una sola llamada recursiva calcula TODO el inventario
        return Ok(new
        {
            TotalProductos  = raiz.ContarProductos(),
            ValorTotalStock = $"RD${raiz.CalcularValorStock():F2}",
            Arbol           = MapearArbol(raiz)
        });
    }

    // GET api/catalogo/bajo-minimo
    // Devuelve todos los productos bajo el mínimo — una llamada, todo el árbol
    [HttpGet("bajo-minimo")]
    public async Task<IActionResult> ProductosBajoMinimo()
    {
        var raiz   = await _builder.ConstruirAsync();
        var alertas = raiz.ObtenerBajoMinimo();   // recorre todo el árbol

        return Ok(new
        {
            Total   = alertas.Count,
            Alertas = alertas.Select(p => new
            {
                p.Id,
                p.Nombre,
                p.StockActual,
                p.StockMinimo,
                Faltante = p.StockMinimo - p.StockActual,
                ValorFaltante = $"RD${(p.StockMinimo - p.StockActual) * p.PrecioUnitario:F2}"
            })
        });
    }

    // GET api/catalogo/categoria/2
    // Calcula valor y conteo solo de la categoría indicada
    [HttpGet("categoria/{id}")]
    public async Task<IActionResult> ResumenCategoria(int id)
    {
        var raiz = await _builder.ConstruirAsync();

        // Busca la categoría en el árbol recursivamente
        var nodo = raiz.Buscar(id.ToString());
        if (nodo is null) return NotFound();

        return Ok(new
        {
            nodo.Nombre,
            nodo.EsHoja,
            TotalProductos  = nodo.ContarProductos(),
            ValorStock      = $"RD${nodo.CalcularValorStock():F2}",
            BajoMinimo      = nodo.ObtenerBajoMinimo().Count
        });
    }

    // Serializa el árbol para el frontend React
    private static object MapearArbol(IComponenteProducto nodo)
    {
        if (nodo is ProductoHoja hoja)
            return new
            {
                hoja.Id, hoja.Nombre, hoja.EsHoja,
                hoja.StockActual, hoja.StockMinimo,
                Precio    = $"RD${hoja.PrecioUnitario:F2}",
                Valor     = $"RD${hoja.CalcularValorStock():F2}",
                BajoStock = hoja.StockActual <= hoja.StockMinimo
            };

        var cat = (CategoriaCompuesta)nodo;
        return new
        {
            cat.Id, cat.Nombre, cat.EsHoja,
            TotalProductos  = cat.ContarProductos(),
            ValorTotal      = $"RD${cat.CalcularValorStock():F2}",
            Hijos           = cat.Hijos.Select(MapearArbol).ToList()
        };
    }
}

// ── Registro en Program.cs ──
// builder.Services.AddScoped<CatalogoBuilder>();
// builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
// builder.Services.AddScoped<IProductoRepository, ProductoRepository>();