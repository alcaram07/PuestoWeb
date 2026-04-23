using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PuestoWeb.Data;
using PuestoWeb.Models;

namespace PuestoWeb.Pages.Admin.Pedidos;

[Authorize(Roles = "Admin")]
public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Pedido Pedido { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var pedido = await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Linias)
                .ThenInclude(l => l.Articulo)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (pedido == null) return NotFound();
        
        Pedido = pedido;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, EstadoPedido nuevoEstado)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null) return NotFound();

        pedido.Estado = nuevoEstado;
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
