namespace AguaMinami.Shared.DTOs;

public class OfertaDescuentoDTO
{
    public int IdOferta { get; set; }
    public double? PorcentajeDesc { get; set; }
    public double? MontoFijo { get; set; }
    public double? TopeDescuento { get; set; }
    public bool? EsAcumulable { get; set; }

    // Navegación
    public string? NombreOferta { get; set; }
}

public class OfertaDescuentoCreateDTO
{
    public int IdOferta { get; set; }
    public double? PorcentajeDesc { get; set; }
    public double? MontoFijo { get; set; }
    public double? TopeDescuento { get; set; }
    public bool? EsAcumulable { get; set; }
}
