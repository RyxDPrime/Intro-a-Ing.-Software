// OrdenMemento.cs — la instantánea inmutable del estado de OrdenCompra
// El Caretaker (HistorialOrdenes) guarda estos objetos pero no puede
// leer ni modificar su contenido — es opaco para él.
// Solo OrdenCompra (Originator) sabe cómo crear y restaurar un Memento.

namespace AguaMinami.Application.Purchasing.Memento;

// ── Memento — completamente inmutable con record sealed ──
public sealed record OrdenMemento
{
    // Metadatos del snapshot
    public Guid     Id           { get; }
    public DateTime GuardadoEn   { get; }
    public string   GuardadoPor  { get; }
    public string   Descripcion  { get; }

    // Estado capturado de OrdenCompra
    internal string   Estado               { get; }
    internal int      IdProducto           { get; }
    internal int?     IdProveedor          { get; }
    internal int      CantidadSolicitada   { get; }
    internal decimal  CostoTotal           { get; }
    internal DateTime? FechaEntregaEstimada { get; }
    internal string?  MotivoCancelacion    { get; }
    internal string?  UsuarioProceso       { get; }

    // Constructor interno — solo OrdenCompra puede crearlo
    internal OrdenMemento(
        string   estado,
        int      idProducto,
        int?     idProveedor,
        int      cantidadSolicitada,
        decimal  costoTotal,
        DateTime? fechaEntregaEstimada,
        string?  motivoCancelacion,
        string?  usuarioProceso,
        string   guardadoPor,
        string   descripcion)
    {
        Id                  = Guid.NewGuid();
        GuardadoEn          = DateTime.Now;
        GuardadoPor         = guardadoPor;
        Descripcion         = descripcion;
        Estado              = estado;
        IdProducto          = idProducto;
        IdProveedor         = idProveedor;
        CantidadSolicitada  = cantidadSolicitada;
        CostoTotal          = costoTotal;
        FechaEntregaEstimada = fechaEntregaEstimada;
        MotivoCancelacion   = motivoCancelacion;
        UsuarioProceso      = usuarioProceso;
    }

    // Descripción pública para mostrar en el historial de snapshots
    public override string ToString() =>
        $"[{GuardadoEn:dd/MM/yyyy HH:mm}] Estado: {Estado} | " +
        $"Guardado por: {GuardadoPor} — {Descripcion}";
}