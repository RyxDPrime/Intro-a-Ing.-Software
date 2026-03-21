namespace AguaMinami.Shared.DTOs;

public class OfertaDTO
{
    public int IdOferta { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Descripcion { get; set; }
}

public class OfertaCreateDTO
{
    public string Nombre { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Descripcion { get; set; }
}
