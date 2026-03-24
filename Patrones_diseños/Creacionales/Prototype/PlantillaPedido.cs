// ---- Prototipo concreto: pedido semanal de clientes frecuentes ----
public class PlantillaPedidoSemanal : IPlantillaPedido
{
    public int    Id        { get; private set; }
    public string Nombre    { get; private set; }
    public int    ClienteId { get; private set; }
    public string DiaSemana { get; private set; }   // "Lunes", "Miércoles"
    public List<LineaPlantilla> Lineas { get; private set; }
    public string? NotasEntrega { get; set; }

    public PlantillaPedidoSemanal(int id, string nombre, int clienteId,
                                   string diaSemana, List<LineaPlantilla> lineas,
                                   string? notas = null)
    {
        Id          = id;
        Nombre      = nombre;
        ClienteId   = clienteId;
        DiaSemana   = diaSemana;
        Lineas      = lineas;
        NotasEntrega = notas;
    }

    // Copia profunda — nuevo Id, mismas líneas pero clonadas independientemente
    public IPlantillaPedido Clonar()
    {
        return new PlantillaPedidoSemanal(
            id:        0,   // el clon aún no tiene Id de BD
            nombre:    $"{Nombre} — copia {DateTime.Now:dd/MM/yyyy}",
            clienteId: ClienteId,
            diaSemana: DiaSemana,
            lineas:    Lineas.Select(l => l.Clonar()).ToList(),   // deep copy
            notas:     NotasEntrega
        );
    }

    // Convierte la plantilla en un Pedido real usando los Flyweights (patrón 20)
    public Pedido ConvertirAPedido()
    {
        return new Pedido
        {
            ClienteId  = ClienteId,
            FechaPedido = DateTime.Now,
            // Las líneas se llenan con el Builder al confirmar (patrón 3)
            // — aquí solo guardamos la intención de qué productos y cantidades
            Lineas = new List<LineaPedido>()
        };
    }
}

// ---- Prototipo concreto: carga de ruta recurrente ----
public class PlantillaPedidoRuta : IPlantillaPedido
{
    public int    Id        { get; private set; }
    public string Nombre    { get; private set; }
    public int    ClienteId { get; private set; }
    public string CodigoRuta       { get; private set; }
    public string ChoferId         { get; private set; }
    public List<LineaPlantilla> Lineas { get; private set; }

    public PlantillaPedidoRuta(int id, string nombre, int clienteId,
                                string codigoRuta, string choferId,
                                List<LineaPlantilla> lineas)
    {
        Id          = id;
        Nombre      = nombre;
        ClienteId   = clienteId;
        CodigoRuta  = codigoRuta;
        ChoferId    = choferId;
        Lineas      = lineas;
    }

    public IPlantillaPedido Clonar()
    {
        return new PlantillaPedidoRuta(
            id:         0,
            nombre:     $"{Nombre} — {DateTime.Now:dd/MM/yyyy}",
            clienteId:  ClienteId,
            codigoRuta: CodigoRuta,
            choferId:   ChoferId,
            lineas:     Lineas.Select(l => l.Clonar()).ToList()
        );
    }

    public Pedido ConvertirAPedido() => new() { ClienteId = ClienteId };
}
