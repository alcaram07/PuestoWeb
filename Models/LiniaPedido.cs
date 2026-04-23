using System.ComponentModel.DataAnnotations;

namespace PuestoWeb.Models;

public class LiniaPedido
{
    public int Id { get; set; }

    [Required]
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;

    [Required]
    public int ArticuloId { get; set; }
    public Articulo Articulo { get; set; } = null!;

    [Required]
    [Range(0.01, 1000)]
    public double Cantidad { get; set; }

    [Required]
    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal => (decimal)Cantidad * PrecioUnitario;
}
