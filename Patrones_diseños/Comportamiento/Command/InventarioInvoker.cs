// InventarioInvoker.cs — el INVOCADOR del patrón Command
// Mantiene una pila de comandos ejecutados para poder deshacerlos.
// También mantiene una cola de comandos pendientes (batch diferido).
// "Toda modificación debe quedar registrada" — tu documento.

namespace AguaMinami.Application.Inventory.Commands;

public class InventarioInvoker
{
    private readonly Stack<IInventarioCommand> _historial    = new();
    private readonly Queue<IInventarioCommand> _cola         = new();
    private readonly IAuditoriaRepository       _auditoria;

    public int TotalEjecutados  => _historial.Count;
    public int TotalEnCola      => _cola.Count;
    public bool PuedeDeshacerse =>
        _historial.TryPeek(out var ultimo) && ultimo.PuedeDeshacerse;

    public InventarioInvoker(IAuditoriaRepository auditoria) =>
        _auditoria = auditoria;

    // ── Ejecuta un comando inmediatamente ──
    public async Task<ResultadoCommand> Ejecutar(IInventarioCommand comando)
    {
        var resultado = await comando.Ejecutar();

        if (resultado.Exitoso)
        {
            _historial.Push(comando);

            // Persiste en auditoría — "con usuario, fecha y motivo" (tu documento)
            await _auditoria.GuardarAsync(new RegistroAuditoria
            {
                Tipo        = comando.Tipo,
                Descripcion = comando.Descripcion,
                Fecha       = comando.FechaCreacion,
                Datos       = $"StockAnterior:{resultado.StockAnterior} → StockNuevo:{resultado.StockNuevo}"
            });
        }

        return resultado;
    }

    // ── Deshace el último comando ejecutado ──
    public async Task<ResultadoCommand> Deshacer()
    {
        if (!_historial.TryPop(out var ultimo))
            throw new InvalidOperationException(
                "No hay comandos en el historial para deshacer.");

        if (!ultimo.PuedeDeshacerse)
            throw new InvalidOperationException(
                $"El comando '{ultimo.Descripcion}' no puede deshacerse.");

        var resultado = await ultimo.Deshacer();

        await _auditoria.GuardarAsync(new RegistroAuditoria
        {
            Tipo        = "DESHACER",
            Descripcion = $"[DESHACER] {ultimo.Descripcion}",
            Fecha       = DateTime.Now,
            Datos       = $"StockRestaurado:{resultado.StockNuevo}"
        });

        return resultado;
    }

    // ── Encola un comando para ejecución diferida (batch) ──
    public void Encolar(IInventarioCommand comando) =>
        _cola.Enqueue(comando);

    // ── Ejecuta todos los comandos encolados en orden ──
    // Útil para procesar múltiples movimientos de una sola salida en ruta
    public async Task<List<ResultadoCommand>> EjecutarCola()
    {
        var resultados = new List<ResultadoCommand>();

        while (_cola.TryDequeue(out var comando))
        {
            var resultado = await Ejecutar(comando);
            resultados.Add(resultado);

            // Si uno falla, detiene la ejecución del resto de la cola
            if (!resultado.Exitoso)
            {
                _cola.Clear();
                break;
            }
        }

        return resultados;
    }

    // Devuelve el historial para mostrar en pantalla
    public IReadOnlyCollection<string> ObtenerHistorial() =>
        _historial.Select(c => $"[{c.Tipo}] {c.Descripcion}").ToList();
}