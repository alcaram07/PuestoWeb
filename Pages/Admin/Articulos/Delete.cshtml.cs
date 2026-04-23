using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PuestoWeb.Data;
using PuestoWeb.Models;

namespace PuestoWeb.Pages.Admin.Articulos;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Articulo Articulo { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var articulo = await _context.Articulos.FirstOrDefaultAsync(m => m.Id == id);

        if (articulo == null)
        {
            return NotFound();
        }
        else
        {
            Articulo = articulo;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var articulo = await _context.Articulos.FindAsync(id);
        if (articulo != null)
        {
            Articulo = articulo;
            _context.Articulos.Remove(Articulo);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
