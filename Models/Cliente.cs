using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PuestoWeb.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Direccion { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Telefono { get; set; } = string.Empty;

    public string? UserId { get; set; }
    public IdentityUser? User { get; set; }
}
