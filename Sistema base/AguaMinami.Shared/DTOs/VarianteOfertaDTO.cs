namespace AguaMinami.Shared.DTOs;

public class VarianteOfertaDTO
{
    public short IdVariante { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
}

public class VarianteOfertaCreateDTO
{
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
}
