namespace AguaMinami.Shared.DTOs;

public class OfertaAsignacionDTO
{
    public int IdAsignacion { get; set; }
    public int? IdOferta { get; set; }
    public string? IdEntidad { get; set; }
    public int? IdVariante { get; set; }

    // Navegación
    public string? NombreOferta { get; set; }
}

public class OfertaAsignacionCreateDTO
{
    public int? IdOferta { get; set; }
    public string? IdEntidad { get; set; }
    public int? IdVariante { get; set; }
}
