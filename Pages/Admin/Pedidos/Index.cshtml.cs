using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PuestoWeb.Data;
using PuestoWeb.Models;

namespace PuestoWeb.Pages.Admin.Pedidos;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Pedido> Pedidos { get;set; } = default!;

    public async Task OnGetAsync()
    {
        Pedidos = await _context.Pedidos
            .Include(p => p.Cliente)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();
    }
}
