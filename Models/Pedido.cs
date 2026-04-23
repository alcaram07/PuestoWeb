using System.ComponentModel.DataAnnotations;

namespace PuestoWeb.Models;

public enum EstadoPedido
{
    Pendiente,
    Enviado,
    Entregado,
    Cancelado
}

public class Pedido
{
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    [Required]
    public DateTime FechaPedido { get; set; } = DateTime.Now;

    [Required]
    public decimal Total { get; set; }

    [Required]
    public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

    public bool EsWhatsApp { get; set; } = false;

    public ICollection<LiniaPedido> Linias { get; set; } = new List<LiniaPedido>();
}
