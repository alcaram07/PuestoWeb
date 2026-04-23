using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PuestoWeb.Data;
using PuestoWeb.Models;

namespace PuestoWeb.Pages.Admin.Articulos;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
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

        var articulo =  await _context.Articulos.FirstOrDefaultAsync(m => m.Id == id);
        if (articulo == null)
        {
            return NotFound();
        }
        Articulo = articulo;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.Attach(Articulo).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ArticuloExists(Articulo.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Index");
    }

    private bool ArticuloExists(int id)
    {
        return _context.Articulos.Any(e => e.Id == id);
    }
}
