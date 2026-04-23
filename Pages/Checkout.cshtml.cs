using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PuestoWeb.Data;
using PuestoWeb.Models;
using PuestoWeb.Services;

namespace PuestoWeb.Pages;

public class CheckoutModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly CartService _cartService;
    private readonly UserManager<IdentityUser> _userManager;

    public CheckoutModel(ApplicationDbContext context, CartService cartService, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _cartService = cartService;
        _userManager = userManager;
    }

    [BindProperty]
    public CheckoutInput Input { get; set; } = new();

    public List<CartItem> CartItems { get; set; } = new();
    public decimal Total { get; set; }

    public class CheckoutInput
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Formato de teléfono no válido")]
        public string Telefono { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        CartItems = _cartService.GetCart();
        Total = _cartService.GetTotal();

        if (CartItems.Count == 0)
        {
            return RedirectToPage("/Catalogo");
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (cliente != null)
                {
                    Input.Nombre = cliente.Nombre;
                    Input.Apellido = cliente.Apellido;
                    Input.Direccion = cliente.Direccion;
                    Input.Telefono = cliente.Telefono;
                }
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        CartItems = _cartService.GetCart();
        Total = _cartService.GetTotal();

        if (CartItems.Count == 0)
        {
            return RedirectToPage("/Catalogo");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // 1. Obtener o crear el cliente
        Cliente? cliente = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == user.Id);
            }
        }

        if (cliente == null)
        {
            // Intentar buscar por teléfono si no está logueado
            cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Telefono == Input.Telefono);
            
            if (cliente == null)
            {
                cliente = new Cliente
                {
                    Nombre = Input.Nombre,
                    Apellido = Input.Apellido,
                    Direccion = Input.Direccion,
                    Telefono = Input.Telefono,
                    UserId = User.Identity?.IsAuthenticated == true ? (await _userManager.GetUserAsync(User))?.Id : null
                };
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
            }
        }

        // 2. Crear el Pedido
        var pedido = new Pedido
        {
            ClienteId = cliente.Id,
            FechaPedido = DateTime.Now,
            Total = Total,
            Estado = EstadoPedido.Pendiente,
            EsWhatsApp = false
        };

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        // 3. Crear las Líneas de Pedido
        foreach (var item in CartItems)
        {
            var linia = new LiniaPedido
            {
                PedidoId = pedido.Id,
                ArticuloId = item.ArticuloId,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.Precio
            };
            _context.LiniasPedidos.Add(linia);
        }

        await _context.SaveChangesAsync();

        // 4. Limpiar carrito
        _cartService.ClearCart();

        return RedirectToPage("/PedidoExitoso", new { id = pedido.Id });
    }
}
