// Eventos y contratos del Mediator para Agua Minami.
// Cada evento representa algo que ocurrió en el sistema.
// Los módulos reaccionan a los eventos que les interesan — sin acoplamiento.

namespace AguaMinami.Application.Mediator;

// ── Interfaz base de todos los eventos ──
public abstract record SistemaEvento
{
    public DateTime OcurrioEn  { get; } = DateTime.Now;
    public string   Originador { get; init; } = "";
}

// ── Eventos concretos del sistema ──

// Disparado por VentaFacade (patrón 10) al completar una venta
public record VentaRegistradaEvento(
    int     IdVenta,
    int     IdCliente,
    string  NombreCliente,
    decimal Total,
    string  TipoComprobante,
    string  NCF,
    List<LineaVentaDto> Lineas
) : SistemaEvento;

// Disparado por AlertaComprasObserver (patrón 4)
public record OrdenCompraCreada(
    int    IdOrden,
    int    IdProducto,
    string NombreProducto,
    int    CantidadSolicitada,
    bool   GeneradaAutomaticamente
) : SistemaEvento;

// Disparado por OrdenCompraService (patrón 5) al recibir mercancía
public record MercanciaRecibidaEvento(
    int    IdOrden,
    int    IdProducto,
    string NombreProducto,
    int    CantidadRecibida,
    int    StockNuevo
) : SistemaEvento;

// Disparado por NominaContext (patrón 6) al calcular un sueldo
public record NominaCalculadaEvento(
    int     IdEmpleado,
    string  NombreEmpleado,
    string  TipoCalculo,
    decimal SueldoNeto,
    string  Periodo
) : SistemaEvento;

// ── Interfaz del Mediator ──
public interface ISistemaMediator
{
    // Publica un evento — todos los manejadores suscritos reaccionan
    Task Publicar<TEvento>(TEvento evento)
        where TEvento : SistemaEvento;

    // Suscribe un módulo a un tipo de evento
    void Suscribir<TEvento>(IManejadorEvento<TEvento> manejador)
        where TEvento : SistemaEvento;
}

// ── Interfaz de manejador de evento ──
public interface IManejadorEvento<in TEvento>
    where TEvento : SistemaEvento
{
    string Nombre { get; }
    Task Manejar(TEvento evento);
}