using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PuestoWeb.Data;
using PuestoWeb.Models;

namespace PuestoWeb.Pages.Admin.Articulos;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Articulo> Articulos { get;set; } = default!;

    public async Task OnGetAsync()
    {
        Articulos = await _context.Articulos.ToListAsync();
    }
}
