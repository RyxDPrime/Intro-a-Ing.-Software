
// ══════════════════════════════════════════════════
//  Visitor 4: Auditoría — registra acceso al catálogo
//  Útil para saber qué productos consultan más los usuarios
// ══════════════════════════════════════════════════
public class AuditoriaVisitor : IProductoVisitor
{
    private readonly List<RegistroAuditoria> _registros = [];
    private readonly string                  _usuario;
    private readonly string                  _operacion;

    public AuditoriaVisitor(string usuario, string operacion)
    {
        _usuario   = usuario;
        _operacion = operacion;
    }

    public void VisitarHoja(ProductoHoja hoja)
    {
        _registros.Add(new RegistroAuditoria
        {
            Tipo        = "CATALOGO_ACCESO",
            Descripcion = $"{_operacion} | Producto: {hoja.Nombre} (Id: {hoja.Id})",
            Usuario     = _usuario,
            Fecha       = DateTime.Now,
            Datos       = $"Stock:{hoja.StockActual}"
        });
    }

    public void VisitarCategoria(CategoriaCompuesta cat)
    {
        _registros.Add(new RegistroAuditoria
        {
            Tipo        = "CATALOGO_ACCESO",
            Descripcion = $"{_operacion} | Categoría: {cat.Nombre}",
            Usuario     = _usuario,
            Fecha       = DateTime.Now
        });
    }

    public List<RegistroAuditoria> ObtenerRegistros() => _registros;
}