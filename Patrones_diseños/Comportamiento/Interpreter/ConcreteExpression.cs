// ---- Expresiones terminales (hojas del árbol) ----

// Evalúa si la cantidad del producto en el pedido cumple el mínimo
public class CompraMinExpression : IExpresion
{
    private readonly int     _productoId;
    private readonly decimal _cantidadMinima;

    public CompraMinExpression(int productoId, decimal cantidadMinima)
    {
        _productoId     = productoId;
        _cantidadMinima = cantidadMinima;
    }

    public bool Evaluar(ContextoOferta ctx)
    {
        var linea = ctx.Pedido.Lineas
            .FirstOrDefault(l => l.Producto.Id == _productoId);

        if (linea is null) return false;

        ctx.ProductoOferta = linea.Producto;
        ctx.CantidadLinea  = linea.Cantidad;
        ctx.CantidadMinima = _cantidadMinima;

        return linea.Cantidad >= _cantidadMinima;
    }
}

// Agrega la línea gratis al pedido cuando la condición se cumple
public class GratisExpression : IExpresion
{
    private readonly int     _productoGratisId;
    private readonly decimal _cantidadGratis;
    private readonly ProductoFlyweightFactory _flyFactory;  // Flyweight (patrón 20)

    public GratisExpression(int productoGratisId, decimal cantidadGratis,
                             ProductoFlyweightFactory flyFactory)
    {
        _productoGratisId = productoGratisId;
        _cantidadGratis   = cantidadGratis;
        _flyFactory       = flyFactory;
    }

    public bool Evaluar(ContextoOferta ctx)
    {
        // Esta expresión siempre evalúa true — su trabajo es el efecto secundario
        ctx.CantidadGratis = _cantidadGratis * ctx.VecesAplica;

        Task.Run(async () =>
        {
            var productoGratis = await _flyFactory.ObtenerAsync(_productoGratisId);
            ctx.LineaGratisGenerada = new LineaPedido(
                productoGratis, ctx.CantidadGratis)
            {
                EsLineaGratis = true   // Flyweight + integración con patrón 14 (Command)
            };
            ctx.Descripcion = $"{ctx.CantidadGratis}x {productoGratis.Nombre} GRATIS";
        }).GetAwaiter().GetResult();

        return true;
    }
}

// ---- Expresiones no terminales (nodos del árbol) ----

// Ambas subexpresiones deben ser true (condición Y resultado)
public class AndExpression : IExpresion
{
    private readonly IExpresion _izquierda;
    private readonly IExpresion _derecha;

    public AndExpression(IExpresion izquierda, IExpresion derecha)
    {
        _izquierda = izquierda;
        _derecha   = derecha;
    }

    public bool Evaluar(ContextoOferta ctx)
        => _izquierda.Evaluar(ctx) && _derecha.Evaluar(ctx);
}

// Al menos una subexpresión debe ser true (ofertas alternativas)
public class OrExpression : IExpresion
{
    private readonly IExpresion _izquierda;
    private readonly IExpresion _derecha;

    public OrExpression(IExpresion izquierda, IExpresion derecha)
        => (_izquierda, _derecha) = (izquierda, derecha);

    public bool Evaluar(ContextoOferta ctx)
        => _izquierda.Evaluar(ctx) || _derecha.Evaluar(ctx);
}

// Oferta válida solo en un rango de fechas
public class VigenciaExpression : IExpresion
{
    private readonly DateTime _desde;
    private readonly DateTime _hasta;
    private readonly IExpresion _reglaInterna;

    public VigenciaExpression(DateTime desde, DateTime hasta, IExpresion regla)
        => (_desde, _hasta, _reglaInterna) = (desde, hasta, regla);

    public bool Evaluar(ContextoOferta ctx)
    {
        var hoy = DateTime.Today;
        if (hoy < _desde || hoy > _hasta) return false;
        return _reglaInterna.Evaluar(ctx);
    }
}
