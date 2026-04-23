using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PuestoWeb.Data;
using PuestoWeb.Models;

namespace PuestoWeb.Pages;

public class CatalogoModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CatalogoModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Articulo> Articulos { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? TipoFiltro { get; set; }

    public async Task OnGetAsync(string? tipo)
    {
        TipoFiltro = tipo;
        var query = _context.Articulos.AsQueryable();

        if (!string.IsNullOrEmpty(tipo))
        {
            if (Enum.TryParse<TipoArticulo>(tipo, out var tipoEnum))
            {
                query = query.Where(a => a.Tipo == tipoEnum);
            }
        }

        Articulos = await query.ToListAsync();
    }

    public async Task<IActionResult> OnPostAddToCartAsync(int id)
    {
        var articulo = await _context.Articulos.FindAsync(id);
        if (articulo != null)
        {
            var cartService = HttpContext.RequestServices.GetRequiredService<Services.CartService>();
            cartService.AddToCart(articulo);
        }

        return RedirectToPage();
    }
}
