namespace cSharpApiFunko.Models;

public record Category(string Nombre)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = Nombre;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } =  DateTime.Now;
}