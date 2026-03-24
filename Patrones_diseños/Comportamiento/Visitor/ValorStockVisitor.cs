namespace AguaMinami.Application.Catalog.Visitors;

// ══════════════════════════════════════════════════
//  Visitor 1: Calcula el valor monetario del stock
//  Desglosado por categoría y producto
// ══════════════════════════════════════════════════
public class ValorStockVisitor : IProductoVisitor
{
    public decimal ValorTotal { get; private set; }
    public List<LineaValor> Detalle { get; } = [];

    public void VisitarHoja(ProductoHoja hoja)
    {
        var valor = hoja.StockActual * hoja.PrecioUnitario;
        ValorTotal += valor;

        Detalle.Add(new LineaValor(
            Tipo:        "Producto",
            Nombre:      hoja.Nombre,
            Stock:       hoja.StockActual,
            PrecioUnit:  hoja.PrecioUnitario,
            ValorTotal:  valor,
            BajoMinimo:  hoja.StockActual <= hoja.StockMinimo
        ));
    }

    public void VisitarCategoria(CategoriaCompuesta cat)
    {
        // La categoría no tiene precio propio — solo registra su nombre
        // El valor se acumula cuando visita las hojas hijas
        Detalle.Add(new LineaValor(
            Tipo:       "Categoría",
            Nombre:     $"[+] {cat.Nombre}",
            Stock:      0,
            PrecioUnit: 0m,
            ValorTotal: 0m,
            BajoMinimo: false
        ));
    }

    public string Resumen() =>
        $"Valor total inventario: RD${ValorTotal:N2} | " +
        $"{Detalle.Count(d => d.Tipo == "Producto")} productos";
}

public record LineaValor(
    string  Tipo,
    string  Nombre,
    int     Stock,
    decimal PrecioUnit,
    decimal ValorTotal,
    bool    BajoMinimo
);
