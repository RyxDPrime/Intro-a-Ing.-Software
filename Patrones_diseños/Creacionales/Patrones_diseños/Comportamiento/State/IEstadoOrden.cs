// IEstadoOrden.cs — contrato que todos los estados deben cumplir
// Cada transición recibe la OrdenCompra (contexto) para poder
// cambiar su estado interno desde dentro del propio estado.

namespace AguaMinami.Application.Purchasing.States;

public interface IEstadoOrden
{
    // Nombre legible del estado — se persiste en BD (Estado_Orden)
    string Nombre { get; }

    // Contable aprueba y pone la orden en proceso
    void Procesar(OrdenCompra orden, string usuario);

    // Administrador confirma que llegó la mercancía al almacén
    void Recibir(OrdenCompra orden, int cantidadRecibida, string usuario);

    // Cualquiera con permiso puede cancelar (con motivo obligatorio)
    void Cancelar(OrdenCompra orden, string motivo, string usuario);

    // Acciones permitidas desde este estado (para la UI)
    List<string> AccionesPermitidas();
}

// ── Evento que emite OrdenCompra en cada transición ──
// El Observer del patrón anterior puede escuchar estos eventos también
public record OrdenTransicionEvento(
    int      IdOrden,
    int      IdProducto,
    string   EstadoAnterior,
    string   EstadoNuevo,
    string   Usuario,
    string?  Motivo,
    DateTime Fecha
);