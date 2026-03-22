// SistemaMediator.cs — el núcleo del patrón
// Mantiene un registro de manejadores por tipo de evento.
// Cuando se publica un evento, notifica a todos los suscritos en paralelo.
// Los módulos no se conocen entre sí — solo hablan con el Mediator.

using Microsoft.Extensions.Logging;

namespace AguaMinami.Application.Mediator;

public class SistemaMediator : ISistemaMediator
{
    // Diccionario: tipo de evento → lista de manejadores
    private readonly Dictionary<Type, List<object>> _manejadores = new();
    private readonly ILogger<SistemaMediator>         _logger;

    public SistemaMediator(ILogger<SistemaMediator> logger) =>
        _logger = logger;

    // ── Registra un manejador para un tipo de evento ──
    public void Suscribir<TEvento>(IManejadorEvento<TEvento> manejador)
        where TEvento : SistemaEvento
    {
        var tipo = typeof(TEvento);

        if (!_manejadores.ContainsKey(tipo))
            _manejadores[tipo] = [];

        _manejadores[tipo].Add(manejador);

        _logger.LogInformation(
            "[Mediator] '{Mod}' suscrito a {Evento}",
            manejador.Nombre, tipo.Name);
    }

    // ── Publica un evento a todos los manejadores suscritos ──
    public async Task Publicar<TEvento>(TEvento evento)
        where TEvento : SistemaEvento
    {
        var tipo = typeof(TEvento);

        _logger.LogInformation(
            "[Mediator] Publicando '{Evento}' de '{Orig}'",
            tipo.Name, evento.Originador);

        if (!_manejadores.TryGetValue(tipo, out var lista))
        {
            _logger.LogWarning(
                "[Mediator] Ningún módulo suscrito a '{Evento}'", tipo.Name);
            return;
        }

        // Ejecuta todos los manejadores en paralelo
        var tareas = lista
            .Cast<IManejadorEvento<TEvento>>()
            .Select(async m =>
            {
                try
                {
                    _logger.LogDebug(
                        "[Mediator] '{Mod}' manejando '{Evento}'",
                        m.Nombre, tipo.Name);
                    await m.Manejar(evento);
                }
                catch (Exception ex)
                {
                    // Un manejador que falla no bloquea a los demás
                    _logger.LogError(ex,
                        "[Mediator] Error en '{Mod}' al manejar '{Evento}'",
                        m.Nombre, tipo.Name);
                }
            });

        await Task.WhenAll(tareas);

        _logger.LogInformation(
            "[Mediator] '{Evento}' procesado por {N} módulo(s)",
            tipo.Name, lista.Count);
    }
}