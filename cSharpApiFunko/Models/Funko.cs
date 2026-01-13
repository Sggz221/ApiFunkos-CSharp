namespace cSharpApiFunko.Models;

public record Funko()
{
    public long Id { get; set; } = 0;
    public string Nombre { get; set; } =  string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public double Precio { get; set; } = 0.0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}