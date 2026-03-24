// ---- SDK externo de la DGII (Adaptee) ----
// Esta clase viene de un paquete NuGet externo — no podemos modificarla.
// Su interfaz es completamente distinta a la nuestra.
namespace DgiiEcfSdk
{
    public class EcfClient
    {
        public EcfClient(string rnc, string certificadoPath, string ambiente) { }

        // Firma diferente: recibe un objeto EcfRequest, devuelve EcfResponse
        public Task<EcfResponse> GenerarECF(EcfRequest request) =>
            Task.FromResult(new EcfResponse());

        public Task<AnulacionResponse> AnularNCF(string ncf, int codigoMotivo) =>
            Task.FromResult(new AnulacionResponse());

        public Task<ConsultaResponse> ConsultarNCF(string rnc, string ncf) =>
            Task.FromResult(new ConsultaResponse());
    }

    // Modelos del SDK — nombres y estructura completamente distintos a los nuestros
    public class EcfRequest
    {
        public string  TipoECF      { get; set; } = "";  // "31", "32", etc.
        public string  RncEmisor    { get; set; } = "";
        public string  RncComprador { get; set; } = "";
        public string  NombreComprador { get; set; } = "";
        public DateTime FechaEmision { get; set; }
        public List<LineaEcf> Items { get; set; } = new();
        public decimal MontoTotal   { get; set; }
        public decimal Itbis        { get; set; }
        public string  Ambiente     { get; set; } = "TesteCF"; // o "eCF"
    }

    public class LineaEcf
    {
        public string  CodigoItem   { get; set; } = "";
        public string  DescripcionItem { get; set; } = "";
        public decimal CantidadItem { get; set; }
        public decimal PrecioUnitarioItem { get; set; }
        public decimal MontoItem    { get; set; }
    }

    public class EcfResponse
    {
        public bool   Success       { get; set; }
        public string CodigoNCF     { get; set; } = "";
        public string CodigoSeg     { get; set; } = "";
        public string XmlResponse   { get; set; } = "";
        public int    CodigoError   { get; set; }
        public string MensajeError  { get; set; } = "";
    }

    public class AnulacionResponse { public bool Success { get; set; } }
    public class ConsultaResponse  { public string Estado { get; set; } = ""; }
}
