// PrinterConfig.cs
// Modelo de configuración de impresora (usado dentro del Singleton)

namespace AguaMinami.Infrastructure.Config;

public class PrinterConfig
{
    public string Nombre          { get; set; } = "";
    public string Tipo            { get; set; } = "";  // "Laser" | "Inyeccion" | "Termica"
    public bool   Predeterminada  { get; set; }
    public string Tinta           { get; set; } = "Color";
    public int    PaginasPorHoja  { get; set; } = 1;

    public override string ToString() =>
        $"{Nombre} ({Tipo}) - Predeterminada: {Predeterminada}";
}