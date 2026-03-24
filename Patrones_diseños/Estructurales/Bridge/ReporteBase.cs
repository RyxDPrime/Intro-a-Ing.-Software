// ---- Abstracción base (lado izquierdo del Bridge) ----
public abstract class ReporteBase
{
    // El puente: la abstracción mantiene una referencia a la implementación
    protected readonly IRendizador _rendizador;
    protected readonly AppConfiguracion _config;   // Singleton (patrón 1)

    protected ReporteBase(IRendizador rendizador, AppConfiguracion config)
    {
        _rendizador = rendizador;
        _config     = config;
    }

    // Método sellado — define el orden; las subclases proveen los datos
    public async Task<ReporteResultado> GenerarAsync(FiltroReporte filtro)
    {
        ValidarFiltro(filtro);
        var datos   = await ObtenerDatosAsync(filtro);
        var titulo  = ObtenerTitulo(filtro);
        var salida  = await _rendizador.RenderizarAsync(datos, titulo);

        return new ReporteResultado
        {
            Contenido    = salida,
            ContentType  = _rendizador.ContentType,
            Extension    = _rendizador.Extension,
            NombreArchivo = $"{titulo}_{DateTime.Now:yyyyMMdd}{_rendizador.Extension}"
        };
    }

    // Las subclases implementan SOLO cómo obtener sus datos
    protected abstract Task<DatosReporte> ObtenerDatosAsync(FiltroReporte filtro);
    protected abstract string ObtenerTitulo(FiltroReporte filtro);

    protected virtual void ValidarFiltro(FiltroReporte filtro)
    {
        if (filtro.FechaInicio > filtro.FechaFin)
            throw new ArgumentException("FechaInicio no puede ser mayor que FechaFin.");
    }
}