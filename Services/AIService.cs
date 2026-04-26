using PuestoWeb.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PuestoWeb.Services;

public class AIService
{
    private readonly ILogger<AIService> _logger;

    public AIService(IConfiguration configuration, ILogger<AIService> logger)
    {
        _logger = logger;
    }

    public bool IsConfigured => true; 

    public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName)
    {
        await Task.CompletedTask;
        return "Pedido por audio detectado";
    }

    public async Task<string> AnalyzeImageAsync(string imageUrl)
    {
        await Task.CompletedTask;
        return "Pedido por imagen detectado";
    }

    public async Task<List<InterpretedItem>> InterpretOrderAsync(string text)
    {
        return await Task.Run(() => InterpretWithRules(text));
    }

    private List<InterpretedItem> InterpretWithRules(string text)
    {
        var items = new List<InterpretedItem>();
        if (string.IsNullOrEmpty(text)) return items;

        // Limpiar el texto y buscar patrones de cantidad (números)
        var cleanedText = text.ToLower();
        
        // Regex para encontrar números seguidos opcionalmente de kg/kilo y luego palabras
        // Ej: "2kg de papas", "1 kilo de manzanas", "3 bananas"
        var matches = Regex.Matches(cleanedText, @"(\d+(?:[\.,]\d+)?)\s*(?:kg|kilo|kilos|unidades|uds)?\s*(?:de)?\s*([a-záéíóúñ\s]+?)(?=\s+\d|\s*$|,) ");

        foreach (Match match in matches)
        {
            if (double.TryParse(match.Groups[1].Value.Replace(",", "."), out double qty))
            {
                items.Add(new InterpretedItem 
                { 
                    ArticuloSugerido = match.Groups[2].Value.Trim(), 
                    Cantidad = qty 
                });
            }
        }

        // Si no encontró nada con números, buscamos palabras clave que coincidan con productos comunes
        if (items.Count == 0)
        {
            // Fallback: si menciona un producto pero sin cantidad, asumimos 1
            var commonProducts = new[] { "manzana", "banana", "papa", "tomate", "lechuga", "naranja" };
            foreach (var prod in commonProducts)
            {
                if (cleanedText.Contains(prod))
                {
                    items.Add(new InterpretedItem { ArticuloSugerido = prod, Cantidad = 1 });
                }
            }
        }

        // Si sigue vacío, al menos devolvemos la primera palabra larga para intentar buscar algo
        if (items.Count == 0)
        {
            var words = cleanedText.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                   .Where(w => w.Length > 3).ToList();
            if (words.Any())
            {
                items.Add(new InterpretedItem { ArticuloSugerido = words[0], Cantidad = 1 });
            }
        }

        return items;
    }
}

public class InterpretedItem
{
    [System.Text.Json.Serialization.JsonPropertyName("articulo")]
    public string ArticuloSugerido { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("cantidad")]
    public double Cantidad { get; set; }
}
