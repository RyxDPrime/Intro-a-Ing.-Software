// IComponenteProducto.cs — interfaz compartida por hojas y contenedores
// El cliente trata a ProductoHoja y CategoriaCompuesta exactamente igual.
// Mapea a Categoria, Tipo_Producto y Producto de tu diagrama de clases.

namespace AguaMinami.Application.Catalog;

public interface IComponenteProducto
{
    int    Id          { get; }
    string Nombre      { get; }
    bool   EsHoja      { get; }   // true = Producto, false = Categoría

    // Calcula el valor total del stock bajo este nodo
    decimal CalcularValorStock();

    // Cuenta todos los productos hoja descendientes
    int ContarProductos();

    // Devuelve todos los productos bajo el mínimo en este nodo
    List<ProductoHoja> ObtenerBajoMinimo();

    // Muestra el árbol con indentación
    void Mostrar(int nivel = 0);

    // Agregar/quitar hijos (solo CategoriaCompuesta los implementa realmente)
    void Agregar(IComponenteProducto componente);
    void Quitar(IComponenteProducto  componente);
}

// ── ProductoHoja — nodo terminal del árbol ──
// Representa un producto concreto: Botellón, Botellita, Fundita, Sal, etc.
// Mapea directamente a la entidad Producto de tu diagrama de BD.
public class ProductoHoja : IComponenteProducto
{
    public int     Id            { get; }
    public string  Nombre        { get; }
    public bool    EsHoja        => true;
    public string  Unidad        { get; }    // "Unidad" | "Fardo" | "Kg"
    public decimal PrecioUnitario { get; }
    public int     StockActual   { get; set; }
    public int     StockMinimo   { get; }
    public bool    Activo        { get; }
    public string  Imagen        { get; }    // ruta de la imagen en el sistema

    public ProductoHoja(
        int     id,
        string  nombre,
        decimal precioUnitario,
        int     stockActual,
        int     stockMinimo,
        string  unidad  = "Unidad",
        bool    activo  = true,
        string  imagen  = "")
    {
        Id             = id;
        Nombre         = nombre;
        PrecioUnitario = precioUnitario;
        StockActual    = stockActual;
        StockMinimo    = stockMinimo;
        Unidad         = unidad;
        Activo         = activo;
        Imagen         = imagen;
    }

    // Un producto solo vale su propio stock × precio
    public decimal CalcularValorStock() =>
        StockActual * PrecioUnitario;

    public int ContarProductos() => 1;

    public List<ProductoHoja> ObtenerBajoMinimo() =>
        StockActual <= StockMinimo ? [this] : [];

    public void Mostrar(int nivel = 0)
    {
        var prefijo    = new string(' ', nivel * 2);
        var alertaStock = StockActual <= StockMinimo ? " ⚠" : "";
        Console.WriteLine(
            $"{prefijo}  [{Id}] {Nombre} | Stock: {StockActual} | " +
            $"RD${PrecioUnitario:F2}/u{alertaStock}");
    }

    // Las hojas no tienen hijos — operaciones no soportadas
    public void Agregar(IComponenteProducto c) =>
        throw new InvalidOperationException(
            "Un producto no puede contener otros componentes.");

    public void Quitar(IComponenteProducto c) =>
        throw new InvalidOperationException(
            "Un producto no puede contener otros componentes.");
}