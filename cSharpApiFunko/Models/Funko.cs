using System.CodeDom.Compiler;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cSharpApiFunko.Models;

[Table("Funkos")]
public record Funko()
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; } = 0;
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } =  string.Empty;
    public Guid CategoriaId { get; set; }
    [ForeignKey(nameof(CategoriaId))]
    [Required]
    public Category Categoria { get; set; } = new Category("");
    [Required]
    public double Precio { get; set; } = 0.0;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}