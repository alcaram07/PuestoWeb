using System.Text.Json;
using PuestoWeb.Models;

namespace PuestoWeb.Services;

public class CartService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CartSessionKey = "Cart";

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public List<CartItem> GetCart()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null) return new List<CartItem>();

        var cartJson = session.GetString(CartSessionKey);
        return cartJson == null ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
    }

    public void AddToCart(Articulo articulo, double cantidad = 1)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(i => i.ArticuloId == articulo.Id);

        if (item == null)
        {
            cart.Add(new CartItem
            {
                ArticuloId = articulo.Id,
                Nombre = articulo.Nombre,
                Precio = articulo.Precio,
                Cantidad = cantidad,
                ImagenUrl = articulo.ImagenUrl
            });
        }
        else
        {
            item.Cantidad += cantidad;
        }

        SaveCart(cart);
    }

    public void RemoveFromCart(int articuloId)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(i => i.ArticuloId == articuloId);
        if (item != null)
        {
            cart.Remove(item);
            SaveCart(cart);
        }
    }

    public void ClearCart()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Remove(CartSessionKey);
    }

    public void SaveCart(List<CartItem> cart)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session != null)
        {
            session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }
    }

    public decimal GetTotal()
    {
        return GetCart().Sum(i => i.Subtotal);
    }

    public int GetItemCount()
    {
        return GetCart().Count;
    }
}
