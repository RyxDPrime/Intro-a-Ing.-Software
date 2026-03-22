// OrdenCompra.cs — el CONTEXTO del patrón State
// Nunca valida el estado con if/switch.
// Delega toda la lógica de transición en _estadoActual.
// Mapea directamente a Orden_Compra y Estado_Orden de tu diagrama de BD.

using AguaMinami.Application.Purchasing.States;

namespace AguaMinami.Application.Purchasing;

public class OrdenCompra
{
    // ── Estado actual (el corazón del patrón) ──
    private IEstadoOrden _estadoActual;

    // ── Datos de la orden (Orden_Compra en tu BD) ──
    public int      Id                   { get; set; }
    public int      IdProducto            { get; set; }
    public int?     IdProveedor           { get; set; }
    public int      CantidadSolicitada    { get; set; }
    public int      CantidadRecibida      { get; set; }
    public decimal  CostoTotal            { get; set; }
    public bool     GeneradaAutomaticamente { get; set; }
    public bool     RecepcionParcial      { get; set; }

    // ── Fechas de ciclo de vida ──
    public DateTime  FechaOrden           { get; set; }
    public DateTime? FechaProcesado       { get; set; }
    public DateTime? FechaRecepcion       { get; set; }
    public DateTime? FechaEntregaEstimada { get; set; }

    // ── Usuarios responsables de cada transición ──
    public string? UsuarioCreacion  { get; set; }
    public string? UsuarioProceso   { get; set; }
    public string? UsuarioRecepcion { get; set; }
    public string? MotivoCancelacion { get; set; }

    // ── Historial de transiciones para auditoría ──
    public List<OrdenTransicionEvento> Historial { get; } = [];

    // ── Nombre del estado actual (lo que se guarda en la BD) ──
    public string EstadoNombre => _estadoActual.Nombre;

    // ── Acciones disponibles según estado actual (para la UI de React) ──
    public List<string> AccionesPermitidas => _estadoActual.AccionesPermitidas();

    // ── Constructor: toda orden nace en Pendiente ──
    public OrdenCompra()
    {
        _estadoActual = new EstadoPendiente();
        FechaOrden    = DateTime.Now;
    }

    // ── Restaurar estado desde BD (al cargar una orden existente) ──
    public OrdenCompra(string estadoGuardado) : this()
    {
        _estadoActual = estadoGuardado switch
        {
            "Pendiente"  => new EstadoPendiente(),
            "EnProceso"  => new EstadoEnProceso(),
            "Recibida"   => new EstadoRecibida(),
            "Cancelada"  => new EstadoCancelada(),
            _ => throw new ArgumentException($"Estado desconocido: {estadoGuardado}")
        };
    }

    // ── Métodos públicos: delegan en el estado actual ──
    public void Procesar(string usuario) =>
        _estadoActual.Procesar(this, usuario);

    public void Recibir(int cantidadRecibida, string usuario) =>
        _estadoActual.Recibir(this, cantidadRecibida, usuario);

    public void Cancelar(string motivo, string usuario) =>
        _estadoActual.Cancelar(this, motivo, usuario);

    // ── Llamado por los estados concretos al transicionar ──
    // Registra el evento en el historial para auditoría
    internal void CambiarEstado(
        IEstadoOrden nuevoEstado,
        string       usuario,
        string?      motivo)
    {
        var evento = new OrdenTransicionEvento(
            IdOrden:        Id,
            IdProducto:     IdProducto,
            EstadoAnterior: _estadoActual.Nombre,
            EstadoNuevo:    nuevoEstado.Nombre,
            Usuario:        usuario,
            Motivo:         motivo,
            Fecha:          DateTime.Now
        );

        Historial.Add(evento);
        _estadoActual = nuevoEstado;
    }
}