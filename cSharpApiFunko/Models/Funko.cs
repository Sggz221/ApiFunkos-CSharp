using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cSharpApiFunko.Models;

[Table("Funkos")]
public record Funko()
{
    public const string IMAGE_DEFAULT = "default.png";
        
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } =  string.Empty;
    public Guid CategoriaId { get; set; }
    [ForeignKey(nameof(CategoriaId))]
    [Required]
    public Category? Categoria { get; set; }
    [Required]
    public double Precio { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string Image { get; set; } = IMAGE_DEFAULT;
}