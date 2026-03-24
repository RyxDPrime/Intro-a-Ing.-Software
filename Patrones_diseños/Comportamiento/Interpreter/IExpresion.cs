// ---- Interfaz AbstractExpression ----
public interface IExpresion
{
    bool Evaluar(ContextoOferta contexto);
}

// ---- Contexto: lo que la expresión necesita para evaluarse ----
public class ContextoOferta
{
    // Entrada: el pedido que se está validando
    public Pedido             Pedido          { get; set; } = null!;
    public ProductoFlyweight? ProductoOferta  { get; set; }  // producto que activa la regla
    public decimal            CantidadLinea   { get; set; }  // cantidad en el pedido

    // Salida: el resultado de evaluar la regla
    public bool               ReglaCumplida   { get; set; }
    public LineaPedido?       LineaGratisGenerada { get; set; }
    public string             Descripcion     { get; set; } = "";

    // Cuántas veces aplica la oferta (compra 10 botellones → 2 gratis si min=5)
    public int VecesAplica => ProductoOferta is null || CantidadMinima == 0
        ? 0
        : (int)Math.Floor(CantidadLinea / CantidadMinima);

    public decimal CantidadMinima { get; set; }   // lo llena CompraMinExpression
    public decimal CantidadGratis { get; set; }   // lo llena GratisExpression
}