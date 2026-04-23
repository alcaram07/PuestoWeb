namespace PuestoWeb.Models;

public class CartItem
{
    public int ArticuloId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public double Cantidad { get; set; }
    public string? ImagenUrl { get; set; }

    public decimal Subtotal => (decimal)Cantidad * Precio;
}
