// IProductoVisitor.cs — contrato del Visitor
// Un método por cada tipo concreto del Composite.
// Nuevas operaciones = nuevas clases Visitor. Las clases del árbol no cambian.
//
// Actualización al Composite (patrón 13):
// Solo se añade Aceptar(visitor) a IComponenteProducto.
// ProductoHoja y CategoriaCompuesta implementan ese único método.

namespace AguaMinami.Application.Catalog.Visitors;

public interface IProductoVisitor
{
    void VisitarHoja(ProductoHoja        hoja);
    void VisitarCategoria(CategoriaCompuesta categoria);
}

// ── Actualización mínima al Composite del patrón 13 ──
// Se agrega UN SOLO MÉTODO a IComponenteProducto
public interface IComponenteProducto
{
    int     Id          { get; }
    string  Nombre      { get; }
    bool    EsHoja      { get; }
    decimal CalcularValorStock();
    int     ContarProductos();
    List<ProductoHoja> ObtenerBajoMinimo();
    void    Mostrar(int nivel = 0);
    void    Agregar(IComponenteProducto c);
    void    Quitar(IComponenteProducto  c);

    // ── NUEVO: acepta un visitor ──
    void Aceptar(IProductoVisitor visitor);
}

// ── ProductoHoja actualizada con Aceptar() ──
// (solo se muestra el método nuevo — el resto es igual al patrón 13)
public class ProductoHoja : IComponenteProducto
{
    // ... todas las propiedades del patrón 13 ...

    // Llama al método del visitor para hojas
    public void Aceptar(IProductoVisitor visitor) =>
        visitor.VisitarHoja(this);
}

// ── CategoriaCompuesta actualizada con Aceptar() ──
public class CategoriaCompuesta : IComponenteProducto
{
    // ... todas las propiedades del patrón 13 ...

    // Visita la categoría y luego propaga a todos los hijos
    public void Aceptar(IProductoVisitor visitor)
    {
        visitor.VisitarCategoria(this);

        // El visitor recorre el árbol automáticamente
        foreach (var hijo in Hijos)
            hijo.Aceptar(visitor);
    }
}
