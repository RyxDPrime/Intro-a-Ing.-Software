// ---- Estado extrínseco: varía por pedido, no se comparte ----
public class LineaPedido
{
    // Referencia al flyweight compartido — NO una copia
    public ProductoFlyweight Producto { get; private set; }

    // Estado extrínseco: único por cada línea de cada pedido
    public decimal Cantidad          { get; set; }
    public decimal PrecioNegociado   { get; set; }  // 0 = usar precio base
    public decimal DescuentoPct      { get; set; }
    public bool    EsLineaGratis     { get; set; }  // Generada por oferta

    public LineaPedido(ProductoFlyweight producto, decimal cantidad,
                       decimal precioNegociado = 0, decimal descuentoPct = 0)
    {
        Producto         = producto;
        Cantidad         = cantidad;
        PrecioNegociado  = precioNegociado;
        DescuentoPct     = descuentoPct;
    }

    // Delega el cálculo al flyweight pasándole el estado extrínseco
    public decimal Total =>
        EsLineaGratis
            ? 0   // Líneas de oferta "compra X lleva Y" no tienen costo
            : Producto.CalcularPrecioFinal(Cantidad, DescuentoPct, PrecioNegociado);

    public string Descripcion =>
        $"{Producto.Nombre} x{Cantidad} {Producto.Unidad} = RD${Total:N2}";
}

// ---- Pedido con múltiples líneas — todas apuntan al mismo flyweight ----
public class Pedido
{
    public int               Id       { get; set; }
    public List<LineaPedido> Lineas   { get; set; } = new();
    public decimal           Total    => Lineas.Sum(l => l.Total);
    public int               TipoVenta { get; set; }
}