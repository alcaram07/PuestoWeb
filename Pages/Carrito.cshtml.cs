using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PuestoWeb.Models;
using PuestoWeb.Services;

namespace PuestoWeb.Pages;

public class CarritoModel : PageModel
{
    private readonly CartService _cartService;

    public CarritoModel(CartService cartService)
    {
        _cartService = cartService;
    }

    public List<CartItem> CartItems { get; set; } = new();
    public decimal Total { get; set; }

    public void OnGet()
    {
        CartItems = _cartService.GetCart();
        Total = _cartService.GetTotal();
    }

    public IActionResult OnPostRemove(int id)
    {
        _cartService.RemoveFromCart(id);
        return RedirectToPage();
    }

    public IActionResult OnPostUpdateQuantity(int id, double delta)
    {
        var cart = _cartService.GetCart();
        var item = cart.FirstOrDefault(i => i.ArticuloId == id);
        
        if (item != null)
        {
            item.Cantidad += delta;
            if (item.Cantidad <= 0)
            {
                _cartService.RemoveFromCart(id);
            }
            else
            {
                _cartService.SaveCart(cart);
            }
        }
        
        return RedirectToPage();
    }
}
