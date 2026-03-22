// CategoriaCompuesta.cs — nodo contenedor del árbol
// Puede contener ProductoHoja o OTRAS CategoriaCompuesta.
// Las operaciones se propagan recursivamente a todos los hijos.
// Mapea a Categoria y Tipo_Producto de tu diagrama de clases.

namespace AguaMinami.Application.Catalog;

public class CategoriaCompuesta : IComponenteProducto
{
    private readonly List<IComponenteProducto> _hijos = [];

    public int    Id     { get; }
    public string Nombre { get; }
    public bool   EsHoja => false;
    public string Descripcion { get; }

    // Acceso de solo lectura a los hijos
    public IReadOnlyList<IComponenteProducto> Hijos => _hijos.AsReadOnly();

    public CategoriaCompuesta(int id, string nombre, string descripcion = "")
    {
        Id          = id;
        Nombre      = nombre;
        Descripcion = descripcion;
    }

    // ── Gestión de hijos ──
    public void Agregar(IComponenteProducto componente) =>
        _hijos.Add(componente);

    public void Quitar(IComponenteProducto componente) =>
        _hijos.Remove(componente);

    // ── Operaciones recursivas — el cliente llama igual que en una hoja ──

    // Suma el valor de TODOS los productos del árbol bajo esta categoría
    public decimal CalcularValorStock() =>
        _hijos.Sum(h => h.CalcularValorStock());

    // Cuenta todos los productos hoja en el subárbol
    public int ContarProductos() =>
        _hijos.Sum(h => h.ContarProductos());

    // Recopila todos los productos bajo el mínimo en el subárbol
    public List<ProductoHoja> ObtenerBajoMinimo() =>
        _hijos.SelectMany(h => h.ObtenerBajoMinimo()).ToList();

    // Muestra el árbol con indentación — llama recursivamente a los hijos
    public void Mostrar(int nivel = 0)
    {
        var prefijo = new string(' ', nivel * 2);
        Console.WriteLine(
            $"{prefijo}[+] {Nombre} " +
            $"({ContarProductos()} productos | " +
            $"RD${CalcularValorStock():F2} en stock)");

        foreach (var hijo in _hijos)
            hijo.Mostrar(nivel + 1);
    }

    // Búsqueda recursiva por nombre (insensible a mayúsculas)
    public IComponenteProducto? Buscar(string nombre)
    {
        if (Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase))
            return this;

        foreach (var hijo in _hijos)
        {
            var encontrado = hijo is CategoriaCompuesta cat
                ? cat.Buscar(nombre)
                : hijo.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase)
                    ? hijo : null;

            if (encontrado is not null) return encontrado;
        }
        return null;
    }

    // Devuelve todos los productos hoja del subárbol aplanados
    public List<ProductoHoja> ObtenerTodosLosProductos() =>
        _hijos.SelectMany(h => h is CategoriaCompuesta c
            ? c.ObtenerTodosLosProductos()
            : h is ProductoHoja p ? [p] : [])
        .ToList();
}