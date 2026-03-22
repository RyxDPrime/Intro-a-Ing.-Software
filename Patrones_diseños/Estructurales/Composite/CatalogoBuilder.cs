// CatalogoBuilder.cs — construye el árbol real de productos de Agua Minami
// Usa los datos de Categoria, Tipo_Producto y Producto de tu BD.
// El árbol refleja exactamente la jerarquía de tu diagrama de clases:
//
//   Catálogo Agua Minami
//   ├── Productos Terminados
//   │   ├── Botellones
//   │   │   ├── Botellón 20L (nuevo)
//   │   │   └── Botellón 20L (reutilizado)
//   │   ├── Botellitas
//   │   │   └── Botellita 500ml
//   │   └── Funditas
//   │       └── Funda 250ml
//   └── Materia Prima
//       ├── Rollos plástico
//       ├── Tapas
//       ├── Sal
//       ├── Etiquetas
//       └── Gasoil

namespace AguaMinami.Application.Catalog;

public class CatalogoBuilder
{
    private readonly ICategoriaRepository   _categorias;
    private readonly IProductoRepository     _productos;
    private readonly IStockRepository        _stock;

    public CatalogoBuilder(
        ICategoriaRepository  categorias,
        IProductoRepository   productos,
        IStockRepository      stock)
    {
        _categorias = categorias;
        _productos  = productos;
        _stock      = stock;
    }

    // Construye el árbol completo desde la BD
    public async Task<CategoriaCompuesta> ConstruirAsync()
    {
        var raiz       = new CategoriaCompuesta(0, "Catálogo Agua Minami");
        var categorias = await _categorias.ObtenerTodasAsync();
        var productos  = await _productos.ObtenerTodosActivosAsync();
        var stocks     = await _stock.ObtenerStockPorProductoAsync();

        // Construye el mapa de categorías para O(1) lookup
        var mapaCategorias = categorias
            .ToDictionary(c => c.Id, c =>
                new CategoriaCompuesta(c.Id, c.Nombre, c.Descripcion));

        // Construye la jerarquía de categorías
        foreach (var cat in categorias)
        {
            var nodo = mapaCategorias[cat.Id];
            if (cat.IdCategoriaPadre.HasValue &&
                mapaCategorias.TryGetValue(cat.IdCategoriaPadre.Value, out var padre))
                padre.Agregar(nodo);
            else
                raiz.Agregar(nodo);
        }

        // Agrega los productos como hojas en su categoría correspondiente
        foreach (var prod in productos)
        {
            var stockActual = stocks.GetValueOrDefault(prod.Id, 0);

            var hoja = new ProductoHoja(
                id:             prod.Id,
                nombre:         prod.Nombre,
                precioUnitario: prod.PrecioUnitario,
                stockActual:    stockActual,
                stockMinimo:    prod.StockMinimo,
                unidad:         prod.Unidad,
                activo:         prod.Activo,
                imagen:         prod.Imagen);

            if (mapaCategorias.TryGetValue(prod.IdCategoria, out var catNodo))
                catNodo.Agregar(hoja);
            else
                raiz.Agregar(hoja);    // sin categoría → va a la raíz
        }

        return raiz;
    }

    // Versión estática para pruebas y demos — árbol hardcodeado de Agua Minami
    public static CategoriaCompuesta ConstruirDemo()
    {
        var raiz = new CategoriaCompuesta(0, "Catálogo Agua Minami");

        // ── Productos Terminados ──
        var terminados = new CategoriaCompuesta(1, "Productos Terminados");

        var botellones = new CategoriaCompuesta(2, "Botellones");
        botellones.Agregar(new ProductoHoja(1, "Botellón 20L (N)",  35m,  120, 20));
        botellones.Agregar(new ProductoHoja(2, "Botellón 20L (R)",  25m,  340, 50));

        var botellitas = new CategoriaCompuesta(3, "Botellitas");
        botellitas.Agregar(new ProductoHoja(3, "Botellita 500ml",   12m, 1200, 200));
        botellitas.Agregar(new ProductoHoja(4, "Fardo Botellitas x24", 280m, 45, 10));

        var funditas = new CategoriaCompuesta(4, "Funditas");
        funditas.Agregar(new ProductoHoja(5, "Fundita 250ml",   5m,    8, 100));  // bajo mínimo!
        funditas.Agregar(new ProductoHoja(6, "Fardo Funditas x48", 220m,  30,  10));

        terminados.Agregar(botellones);
        terminados.Agregar(botellitas);
        terminados.Agregar(funditas);

        // ── Materia Prima ──
        var materiaPrima = new CategoriaCompuesta(5, "Materia Prima");
        materiaPrima.Agregar(new ProductoHoja(7,  "Rollo plástico",  1500m, 12,  5, "Unidad"));
        materiaPrima.Agregar(new ProductoHoja(8,  "Tapas botellón",  0.8m, 500, 100));
        materiaPrima.Agregar(new ProductoHoja(9,  "Sal (kg)",          25m, 200,  50, "Kg"));
        materiaPrima.Agregar(new ProductoHoja(10, "Etiquetas",        0.5m, 90, 200));  // bajo mínimo!
        materiaPrima.Agregar(new ProductoHoja(11, "Gasoil (galón)",   280m,  15,   5, "Galón"));

        raiz.Agregar(terminados);
        raiz.Agregar(materiaPrima);
        return raiz;
    }
}