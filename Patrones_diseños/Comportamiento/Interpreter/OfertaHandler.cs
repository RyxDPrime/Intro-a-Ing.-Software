// ---- Integración con el AplicacionOfertasHandler (Chain, patrón 7) ----
// Este handler ya existía — ahora en vez de lógica hardcodeada
// usa el Interpreter para evaluar reglas dinámicas de la BD.

public class AplicacionOfertasHandler : VentaHandlerBase
{
    private readonly IOfertaRepository   _ofertaRepo;
    private readonly OfertaParser        _parser;

    public AplicacionOfertasHandler(IOfertaRepository ofertaRepo,
                                     OfertaParser parser)
    {
        _ofertaRepo = ofertaRepo;
        _parser     = parser;
    }

    public override async Task<VentaContext> ManejarAsync(VentaContext ctx)
    {
        // Obtener todas las ofertas activas de la BD
        var ofertas = await _ofertaRepo.ObtenerActivasAsync();

        foreach (var oferta in ofertas)
        {
            try
            {
                // Interpreter: parsea la regla de texto y construye el árbol
                var expresion = await _parser.ParsearAsync(oferta.ReglaTexto);

                var contextoOferta = new ContextoOferta { Pedido = ctx.Pedido };

                // Evalúa el árbol contra el pedido actual
                if (expresion.Evaluar(contextoOferta) &&
                    contextoOferta.LineaGratisGenerada is not null)
                {
                    // Agrega la línea gratis al pedido (integración con Flyweight)
                    ctx.Pedido.Lineas.Add(contextoOferta.LineaGratisGenerada);
                    ctx.OfertasAplicadas.Add(new OfertaAplicada
                    {
                        NombreOferta  = oferta.Nombre,
                        Descripcion   = contextoOferta.Descripcion,
                        AhorroRD      = contextoOferta.LineaGratisGenerada.Producto
                                            .PrecioBase * contextoOferta.CantidadGratis
                    });
                }
            }
            catch (Exception ex)
            {
                // Una oferta mal escrita no detiene la venta
                ctx.Advertencias.Add($"Oferta '{oferta.Nombre}' no pudo evaluarse: {ex.Message}");
            }
        }

        return await SiguienteAsync(ctx);
    }
}

// ---- Ejemplos de reglas que el administrador puede escribir en BD ----
/*
  Oferta 1: "COMPRA 5 BOTELLÓN LLEVA 1 BOTELLÓN GRATIS"
  Oferta 2: "COMPRA 10 BOTELLITA LLEVA 2 BOTELLITA GRATIS"
  Oferta 3: "VIGENCIA 2025-12-01 2025-12-31 COMPRA 3 BOTELLÓN LLEVA 1 FUNDITA GRATIS"
  Oferta 4: "COMPRA 20 BOTELLÓN LLEVA 2 BOTELLÓN GRATIS"

  El administrador las escribe en la pantalla de Ofertas del sistema.
  No necesita pedirle nada al programador — el Interpreter las evalúa en vivo.
*/
