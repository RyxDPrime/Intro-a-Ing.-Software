// Los cuatro estados concretos de OrdenCompra
// Cada uno implementa IEstadoOrden y lanza excepción
// si se intenta una transición inválida desde ese estado.
// ────────────────────────────────────────────────────

namespace AguaMinami.Application.Purchasing.States;

// ══════════════════════════════════════════
//  1. PENDIENTE — estado inicial
//     Acciones: Procesar, Cancelar
//     Generado automáticamente por AlertaComprasObserver
// ══════════════════════════════════════════
public class EstadoPendiente : IEstadoOrden
{
    public string Nombre => "Pendiente";

    public void Procesar(OrdenCompra orden, string usuario)
    {
        // Transición válida: Pendiente → EnProceso
        orden.FechaProcesado = DateTime.Now;
        orden.UsuarioProceso  = usuario;
        orden.CambiarEstado(new EstadoEnProceso(), usuario, null);
    }

    public void Recibir(OrdenCompra orden, int cantidadRecibida, string usuario) =>
        throw new InvalidOperationException(
            "No se puede recibir una orden que aún está Pendiente. " +
            "Debe procesarse primero por el contable.");

    public void Cancelar(OrdenCompra orden, string motivo, string usuario)
    {
        orden.MotivoCancelacion = motivo;
        orden.CambiarEstado(new EstadoCancelada(), usuario, motivo);
    }

    public List<string> AccionesPermitidas() =>
        ["Procesar", "Cancelar"];
}


// ══════════════════════════════════════════
//  2. EN PROCESO — contable aprobó y pagó
//     Acciones: Recibir, Cancelar
// ══════════════════════════════════════════
public class EstadoEnProceso : IEstadoOrden
{
    public string Nombre => "EnProceso";

    public void Procesar(OrdenCompra orden, string usuario) =>
        throw new InvalidOperationException(
            "La orden ya está en proceso. Solo puede Recibirse o Cancelarse.");

    public void Recibir(OrdenCompra orden, int cantidadRecibida, string usuario)
    {
        if (cantidadRecibida <= 0)
            throw new ArgumentException(
                "La cantidad recibida debe ser mayor a cero.");

        orden.CantidadRecibida = cantidadRecibida;
        orden.FechaRecepcion   = DateTime.Now;
        orden.UsuarioRecepcion = usuario;

        // Si recibió menos de lo solicitado, lo registra como recepción parcial
        orden.RecepcionParcial = cantidadRecibida < orden.CantidadSolicitada;

        orden.CambiarEstado(new EstadoRecibida(), usuario, null);
    }

    public void Cancelar(OrdenCompra orden, string motivo, string usuario)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException(
                "Cancelar una orden en proceso requiere un motivo.");

        orden.MotivoCancelacion = motivo;
        orden.CambiarEstado(new EstadoCancelada(), usuario, motivo);
    }

    public List<string> AccionesPermitidas() =>
        ["Recibir", "Cancelar"];
}


// ══════════════════════════════════════════
//  3. RECIBIDA — estado terminal positivo
//     La mercancía llegó al almacén
//     Ninguna transición posible desde aquí
// ══════════════════════════════════════════
public class EstadoRecibida : IEstadoOrden
{
    public string Nombre => "Recibida";

    public void Procesar(OrdenCompra orden, string usuario) =>
        throw new InvalidOperationException(
            "Orden ya recibida. No se puede modificar su estado.");

    public void Recibir(OrdenCompra orden, int cantidadRecibida, string usuario) =>
        throw new InvalidOperationException(
            "Orden ya recibida. No se puede volver a recibir.");

    public void Cancelar(OrdenCompra orden, string motivo, string usuario) =>
        throw new InvalidOperationException(
            "No se puede cancelar una orden ya recibida.");

    public List<string> AccionesPermitidas() => [];
}


// ══════════════════════════════════════════
//  4. CANCELADA — estado terminal negativo
//     Ninguna transición posible desde aquí
// ══════════════════════════════════════════
public class EstadoCancelada : IEstadoOrden
{
    public string Nombre => "Cancelada";

    public void Procesar(OrdenCompra orden, string usuario) =>
        throw new InvalidOperationException(
            "Orden cancelada. Crea una nueva orden si necesitas reponer el producto.");

    public void Recibir(OrdenCompra orden, int cantidadRecibida, string usuario) =>
        throw new InvalidOperationException(
            "Orden cancelada. No se puede recibir.");

    public void Cancelar(OrdenCompra orden, string motivo, string usuario) =>
        throw new InvalidOperationException(
            "La orden ya está cancelada.");

    public List<string> AccionesPermitidas() => [];
}