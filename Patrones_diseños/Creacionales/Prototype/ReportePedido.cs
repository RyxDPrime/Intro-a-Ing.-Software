// ---- Registro de plantillas: almacén de prototipos disponibles ----
public class RegistroPlantillas
{
    // Clave: Id de la plantilla en BD
    private readonly Dictionary<int, IPlantillaPedido> _plantillas = new();
    private readonly IPlantillaRepository _repo;

    public RegistroPlantillas(IPlantillaRepository repo) => _repo = repo;

    // Carga todas las plantillas del cliente al abrir la pantalla de ventas
    public async Task CargarPorClienteAsync(int clienteId)
    {
        var plantillas = await _repo.ObtenerPorClienteAsync(clienteId);
        foreach (var p in plantillas)
            _plantillas[p.Id] = p;
    }

    // Guarda una plantilla nueva (por ejemplo, al marcar "guardar este pedido")
    public async Task GuardarAsync(IPlantillaPedido plantilla)
    {
        var id = await _repo.InsertarAsync(plantilla);
        // El registro guarda el original — no el clon
        _plantillas[id] = plantilla;
    }

    // Devuelve un CLON — el original queda intacto en el registro
    public IPlantillaPedido? ObtenerClon(int plantillaId)
    {
        return _plantillas.TryGetValue(plantillaId, out var plantilla)
            ? plantilla.Clonar()
            : null;
    }

    public IEnumerable<IPlantillaPedido> ListarTodas() => _plantillas.Values;

    // Elimina del registro y de la BD
    public async Task EliminarAsync(int plantillaId)
    {
        _plantillas.Remove(plantillaId);
        await _repo.EliminarAsync(plantillaId);
    }
}
