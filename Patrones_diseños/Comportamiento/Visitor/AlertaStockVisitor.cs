
// ══════════════════════════════════════════════════
//  Visitor 2: Detecta productos bajo stock mínimo
//  y calcula el costo de reposición
// ══════════════════════════════════════════════════
public class AlertasStockVisitor : IProductoVisitor
{
    public List<AlertaStock> Alertas        { get; } = [];
    public decimal            CostoReposicion { get; private set; }

    public void VisitarHoja(ProductoHoja hoja)
    {
        if (hoja.StockActual > hoja.StockMinimo) return;

        var unidadesFaltantes = hoja.StockMinimo - hoja.StockActual;
        var costoEstimado     = unidadesFaltantes * hoja.PrecioUnitario;

        CostoReposicion += costoEstimado;

        Alertas.Add(new AlertaStock(
            IdProducto:        hoja.Id,
            NombreProducto:    hoja.Nombre,
            StockActual:       hoja.StockActual,
            StockMinimo:       hoja.StockMinimo,
            UnidadesFaltantes: unidadesFaltantes,
            CostoEstimado:     costoEstimado,
            Prioridad: hoja.StockActual == 0 ? "CRITICA"
                     : hoja.StockActual <= hoja.StockMinimo / 2 ? "ALTA"
                     : "MEDIA"
        ));
    }

    public void VisitarCategoria(CategoriaCompuesta cat)
    {
        // Las categorías no tienen stock propio — solo navega hacia hojas
    }

    public int TotalCriticas => Alertas.Count(a => a.Prioridad == "CRITICA");
    public int TotalAltas    => Alertas.Count(a => a.Prioridad == "ALTA");
}

public record AlertaStock(
    int     IdProducto,
    string  NombreProducto,
    int     StockActual,
    int     StockMinimo,
    int     UnidadesFaltantes,
    decimal CostoEstimado,
    string  Prioridad
);