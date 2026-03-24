
// ══════════════════════════════════════════════════
//  Visitor 3: Exporta el inventario completo a CSV
//  Compatible con Excel para los jefes de Agua Minami
// ══════════════════════════════════════════════════
public class ExportarCsvVisitor : IProductoVisitor
{
    private readonly StringBuilder _csv = new();
    private int _nivelActual = 0;

    public ExportarCsvVisitor()
    {
        // Encabezado del CSV
        _csv.AppendLine("Tipo,Categoría,Producto,Stock,StockMínimo,PrecioUnit,ValorTotal,Estado");
    }

    public void VisitarHoja(ProductoHoja hoja)
    {
        var estado   = hoja.StockActual == 0                ? "SIN STOCK"
                     : hoja.StockActual <= hoja.StockMinimo ? "BAJO MÍNIMO"
                     : "OK";

        var valor    = hoja.StockActual * hoja.PrecioUnitario;
        var sangria  = new string(' ', _nivelActual * 2);

        _csv.AppendLine(
            $"Producto,"                +
            $"{sangria},"               +    // columna categoría vacía para hojas
            $"{hoja.Nombre},"           +
            $"{hoja.StockActual},"      +
            $"{hoja.StockMinimo},"      +
            $"RD${hoja.PrecioUnitario:F2}," +
            $"RD${valor:F2},"           +
            estado);
    }

    public void VisitarCategoria(CategoriaCompuesta cat)
    {
        _csv.AppendLine(
            $"Categoría,"                                       +
            $"{cat.Nombre},"                                    +
            ","                                                 +    // producto vacío
            $"{cat.ContarProductos()} productos,"               +
            ","                                                 +
            ","                                                 +
            $"RD${cat.CalcularValorStock():F2},"                +
            "");
        _nivelActual++;
    }

    public string ObtenerCsv()
    {
        return _csv.ToString();
    }

    public byte[] ObtenerBytes() =>
        Encoding.UTF8.GetBytes(_csv.ToString());
}

