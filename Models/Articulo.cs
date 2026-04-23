using System.ComponentModel.DataAnnotations;

namespace PuestoWeb.Models;

public enum TipoArticulo
{
    Fruta,
    Verdura
}

public class Articulo
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [Required]
    public TipoArticulo Tipo { get; set; }

    [Required]
    [Range(0.01, 1000000)]
    public decimal Precio { get; set; }

    public string? ImagenUrl { get; set; }
}
