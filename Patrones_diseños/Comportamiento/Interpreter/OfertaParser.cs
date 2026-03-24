// ---- Parser: convierte texto de BD en árbol de expresiones ----
public class OfertaParser
{
    private readonly ProductoFlyweightFactory _flyFactory;
    private readonly IProductoRepository      _productoRepo;

    public OfertaParser(ProductoFlyweightFactory flyFactory,
                        IProductoRepository productoRepo)
    {
        _flyFactory   = flyFactory;
        _productoRepo = productoRepo;
    }

    // Gramática soportada:
    //   COMPRA {cant} {producto} LLEVA {cant} {producto} GRATIS
    //   COMPRA {cant} {producto} DESCUENTO {pct}%
    //   VIGENCIA {desde} {hasta} COMPRA {cant} {producto} LLEVA {cant} {producto} GRATIS
    public async Task<IExpresion> ParsearAsync(string reglaTexto)
    {
        var tokens = reglaTexto.Trim().ToUpper().Split(' ',
                         StringSplitOptions.RemoveEmptyEntries);
        int pos = 0;

        // Si empieza con VIGENCIA, envuelve la regla interna
        if (tokens[pos] == "VIGENCIA")
        {
            pos++;
            var desde = DateTime.Parse(tokens[pos++]);
            var hasta = DateTime.Parse(tokens[pos++]);
            var reglaInterna = await ParsearReglaBaseAsync(tokens, pos);
            return new VigenciaExpression(desde, hasta, reglaInterna);
        }

        return await ParsearReglaBaseAsync(tokens, pos);
    }

    private async Task<IExpresion> ParsearReglaBaseAsync(string[] tokens, int pos)
    {
        // COMPRA {cant} {nombre_producto} LLEVA {cant} {nombre_producto} GRATIS
        if (tokens[pos] == "COMPRA")
        {
            pos++;
            var cantCompra   = decimal.Parse(tokens[pos++]);
            var nomCompra    = tokens[pos++];
            var idCompra     = await ResolverProductoIdAsync(nomCompra);

            if (tokens[pos] == "LLEVA")
            {
                pos++;
                var cantGratis = decimal.Parse(tokens[pos++]);
                var nomGratis  = tokens[pos++];
                var idGratis   = await ResolverProductoIdAsync(nomGratis);
                // tokens[pos] == "GRATIS"

                return new AndExpression(
                    new CompraMinExpression(idCompra, cantCompra),
                    new GratisExpression(idGratis, cantGratis, _flyFactory)
                );
            }

            if (tokens[pos] == "DESCUENTO")
            {
                pos++;
                var pct = decimal.Parse(tokens[pos].TrimEnd('%'));
                // Retorna expresión de descuento (extensión futura)
                return new CompraMinExpression(idCompra, cantCompra);
            }
        }

        throw new FormatException($"Regla no reconocida: {string.Join(' ', tokens)}");
    }

    private async Task<int> ResolverProductoIdAsync(string nombre)
    {
        var producto = await _productoRepo.BuscarPorNombreAsync(nombre)
            ?? throw new KeyNotFoundException($"Producto '{nombre}' no encontrado.");
        return producto.Id;
    }
}