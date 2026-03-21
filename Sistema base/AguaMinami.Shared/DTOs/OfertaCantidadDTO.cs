namespace AguaMinami.Shared.DTOs;

public class OfertaCantidadDTO
{
    public int IdOferta { get; set; }
    public short? IdVariante { get; set; }
    public int? CantRequerida { get; set; }
    public int? CantGratis { get; set; }
    public bool? EsAcumulable { get; set; }

    // Navegación (solo lectura)
    public string? NombreOferta { get; set; }
    public string? NombreVariante { get; set; }
}

public class OfertaCantidadCreateDTO
{
    public int IdOferta { get; set; }
    public short? IdVariante { get; set; }
    public int? CantRequerida { get; set; }
    public int? CantGratis { get; set; }
    public bool? EsAcumulable { get; set; }
}
