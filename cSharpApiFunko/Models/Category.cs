namespace cSharpApiFunko.Models;

public record Category()
{
    private Guid Id { get; set; }
    private string Nombre { get; set; } = string.Empty;
    private DateTime CreatedAt { get; set; }
    private DateTime UpdatedAt { get; set; }
}