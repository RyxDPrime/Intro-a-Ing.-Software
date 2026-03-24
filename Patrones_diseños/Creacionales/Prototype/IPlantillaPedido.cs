// ---- Interfaz Prototype ----
public interface IPlantillaPedido
{
    int    Id          { get; }
    string Nombre      { get; }   // "Pedido semanal Colmado El Buen Precio"
    int    ClienteId   { get; }

    // Produce una copia profunda lista para editar y confirmar
    IPlantillaPedido Clonar();

    // Convierte la plantilla en un Pedido real (para pasarlo al Builder)
    Pedido ConvertirAPedido();
}

// ---- Línea de plantilla: misma estructura que LineaPedido pero sin precio
//      negociado fijo — se recalcula al clonar según el día ----
public class LineaPlantilla
{
    public int     ProductoId       { get; set; }
    public string  NombreProducto   { get; set; } = "";
    public decimal CantidadBase     { get; set; }
    public bool    AjustarConOferta { get; set; } = true;

    // Copia profunda de la línea — necesaria para que el clon sea independiente
    public LineaPlantilla Clonar() => new()
    {
        ProductoId       = ProductoId,
        NombreProducto   = NombreProducto,
        CantidadBase     = CantidadBase,
        AjustarConOferta = AjustarConOferta
    };
}