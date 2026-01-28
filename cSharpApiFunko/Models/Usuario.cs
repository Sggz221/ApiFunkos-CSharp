using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cSharpApiFunko.Models;

public class Usuario
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required]
    [StringLength(50)]
    public string UserName { get; set; } =  string.Empty;
    [Required]
    [StringLength(100)]
    public string PasswordHash { get; set; } =  string.Empty;
    [Required]
    [StringLength(50)]
    public string Email { get; set; } =  string.Empty;
    [Required]
    public string Role { get; set; } =  UserRoles.USER;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class UserRoles
{
    public const string ADMIN = "ADMIN";

    public const string USER = "USER";
}