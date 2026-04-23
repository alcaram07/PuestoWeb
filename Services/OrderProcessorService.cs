using Microsoft.EntityFrameworkCore;
using PuestoWeb.Data;
using PuestoWeb.Models;

namespace PuestoWeb.Services;

public class OrderProcessorService
{
    private readonly ApplicationDbContext _context;
    private readonly AIService _aiService;

    public OrderProcessorService(ApplicationDbContext context, AIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<Pedido?> ProcessWhatsAppOrderAsync(string phoneNumber, string rawInput, string type = "text")
    {
        string textToProcess = rawInput;

        // 1. Si es audio o imagen, pasar por la IA primero
        if (type == "audio")
        {
            // Aquí se descargaría el stream real, por ahora usamos el texto simulado
            textToProcess = await _aiService.TranscribeAudioAsync(Stream.Null, "audio.ogg");
        }
        else if (type == "image")
        {
            textToProcess = await _aiService.AnalyzeImageAsync(rawInput);
        }

        // 2. Interpretar el texto para sacar lista de productos
        var itemsSugeridos = await _aiService.InterpretOrderAsync(textToProcess);

        if (!itemsSugeridos.Any()) return null;

        // 3. Buscar o crear el cliente
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Telefono == phoneNumber);
        if (cliente == null)
        {
            cliente = new Cliente
            {
                Nombre = "Cliente WhatsApp",
                Apellido = phoneNumber,
                Direccion = "Pendiente de confirmar",
                Telefono = phoneNumber
            };
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }

        // 4. Crear el Pedido
        var pedido = new Pedido
        {
            ClienteId = cliente.Id,
            FechaPedido = DateTime.Now,
            Estado = EstadoPedido.Pendiente,
            EsWhatsApp = true,
            Total = 0 // Se calcula abajo
        };

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        decimal total = 0;

        // 5. Mapear artículos sugeridos con artículos reales
        foreach (var sugerido in itemsSugeridos)
        {
            // Búsqueda simple (se puede mejorar con Fuzzy Search)
            var articuloReal = await _context.Articulos
                .FirstOrDefaultAsync(a => a.Nombre.ToLower().Contains(sugerido.ArticuloSugerido.ToLower()) 
                                       || sugerido.ArticuloSugerido.ToLower().Contains(a.Nombre.ToLower()));

            if (articuloReal != null)
            {
                var linia = new LiniaPedido
                {
                    PedidoId = pedido.Id,
                    ArticuloId = articuloReal.Id,
                    Cantidad = sugerido.Cantidad,
                    PrecioUnitario = articuloReal.Precio
                };
                _context.LiniasPedidos.Add(linia);
                total += linia.Subtotal;
            }
        }

        pedido.Total = total;
        await _context.SaveChangesAsync();

        return pedido;
    }
}
