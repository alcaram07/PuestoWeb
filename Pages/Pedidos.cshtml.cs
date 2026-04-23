using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PuestoWeb.Data;
using PuestoWeb.Models;

namespace PuestoWeb.Pages;

[Authorize]
public class PedidosModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public PedidosModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IList<Pedido> Pedidos { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == user.Id);
        
        if (cliente == null)
        {
            Pedidos = new List<Pedido>();
            return Page();
        }

        Pedidos = await _context.Pedidos
            .Where(p => p.ClienteId == cliente.Id)
            .Include(p => p.Linias)
                .ThenInclude(l => l.Articulo)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();

        return Page();
    }
}
