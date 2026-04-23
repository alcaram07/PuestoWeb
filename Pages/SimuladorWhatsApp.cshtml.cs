using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PuestoWeb.Data;
using PuestoWeb.Models;
using PuestoWeb.Services;

namespace PuestoWeb.Pages;

public class SimuladorWhatsAppModel : PageModel
{
    private readonly OrderProcessorService _orderProcessor;
    private readonly ApplicationDbContext _context;

    public SimuladorWhatsAppModel(OrderProcessorService orderProcessor, ApplicationDbContext context)
    {
        _orderProcessor = orderProcessor;
        _context = context;
    }

    [BindProperty]
    public string Telefono { get; set; } = "+5491112345678";

    [BindProperty]
    public string Tipo { get; set; } = "text";

    [BindProperty]
    public string Contenido { get; set; } = "Hola, quiero 2 kilos de papas y una banana.";

    public Pedido? PedidoProcesado { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Contenido))
        {
            Contenido = Tipo == "text" ? "Pedido de prueba" : "http://dummy-link.com/media";
        }

        var pedido = await _orderProcessor.ProcessWhatsAppOrderAsync(Telefono, Contenido, Tipo);

        if (pedido != null)
        {
            // Volver a cargar con relaciones para la UI
            PedidoProcesado = await _context.Pedidos
                .Include(p => p.Linias)
                    .ThenInclude(l => l.Articulo)
                .FirstOrDefaultAsync(p => p.Id == pedido.Id);
        }

        return Page();
    }
}
