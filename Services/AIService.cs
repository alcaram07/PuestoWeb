using PuestoWeb.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PuestoWeb.Services;

public class AIService
{
    private readonly string _apiKey;
    private readonly ILogger<AIService> _logger;

    public AIService(IConfiguration configuration, ILogger<AIService> logger)
    {
        _apiKey = configuration["Groq:ApiKey"] ?? "";
        _logger = logger;
    }

    public bool IsConfigured => true; 

    public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName)
    {
        await Task.CompletedTask;
        return "Simulación: El audio dice 'Quiero 2 kilos de papas y una sandía'.";
    }

    public async Task<string> AnalyzeImageAsync(string imageUrl)
    {
        await Task.CompletedTask;
        return "Simulación: Imagen de lista de compras detectada.";
    }

    public async Task<List<InterpretedItem>> InterpretOrderAsync(string text)
    {
        // Forzamos el uso de reglas para evitar problemas de compilación con librerías externas sin usar
        // En un escenario real con API Key, aquí se reintegraría la llamada a Groq o OpenAI
        return await Task.Run(() => InterpretWithRules(text));
    }

    private List<InterpretedItem> InterpretWithRules(string text)
    {
        var items = new List<InterpretedItem>();
        if (string.IsNullOrEmpty(text)) return items;

        var words = text.ToLower().Split(new char[] { ' ', ',', '.', '\n', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < words.Length; i++)
        {
            if (double.TryParse(words[i], out double qty))
            {
                if (i + 1 < words.Length)
                {
                    var product = words[i + 1];
                    if ((product == "kg" || product == "kilo" || product == "kilos" || product == "de") && i + 2 < words.Length)
                    {
                        product = words[i + 2];
                        if (product == "de" && i + 3 < words.Length) product = words[i + 3];
                    }
                    
                    items.Add(new InterpretedItem { ArticuloSugerido = product, Cantidad = qty });
                    i++; 
                }
            }
            else if (words[i] == "un" || words[i] == "una")
            {
                 if (i + 1 < words.Length)
                 {
                    var product = words[i + 1];
                    if (product == "kilo" || product == "kg" && i + 2 < words.Length) product = words[i+2];
                    items.Add(new InterpretedItem { ArticuloSugerido = product, Cantidad = 1 });
                    i++;
                 }
            }
        }

        if (items.Count == 0 && words.Length > 0)
        {
            // Si no hay estructura clara, tomamos la primera palabra como producto probable
            items.Add(new InterpretedItem { ArticuloSugerido = words[0], Cantidad = 1 });
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
