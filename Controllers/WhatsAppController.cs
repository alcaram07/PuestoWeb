using Microsoft.AspNetCore.Mvc;
using PuestoWeb.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization; // Añadir esto

namespace PuestoWeb.Controllers;

[ApiController]
[Route("api/whatsapp")]
[AllowAnonymous] // Permitir acceso público para que Meta pueda enviar mensajes
public class WhatsAppController : ControllerBase
{
    private readonly AIService _aiService;
    private readonly OrderProcessorService _orderProcessor;
    private readonly ILogger<WhatsAppController> _logger;
    private const string VERIFY_TOKEN = "puesto_token_2024";

    public WhatsAppController(AIService aiService, OrderProcessorService orderProcessor, ILogger<WhatsAppController> logger)
    {
        _aiService = aiService;
        _orderProcessor = orderProcessor;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Verify()
    {
        var hubMode = Request.Query["hub.mode"].ToString();
        var hubToken = Request.Query["hub.verify_token"].ToString();
        var hubChallenge = Request.Query["hub.challenge"].ToString();

        if (hubMode == "subscribe" && hubToken == VERIFY_TOKEN)
        {
            return Ok(hubChallenge);
        }

        return Forbid();
    }

    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            _logger.LogInformation("Webhook recibido: {Body}", body);

            using var jsonDoc = JsonDocument.Parse(body);
            var root = jsonDoc.RootElement;

            // Navegar por el JSON de Meta para encontrar el mensaje
            if (root.TryGetProperty("entry", out var entryArray) && entryArray.GetArrayLength() > 0)
            {
                var entry = entryArray[0];
                if (entry.TryGetProperty("changes", out var changesArray) && changesArray.GetArrayLength() > 0)
                {
                    var change = changesArray[0];
                    var value = change.GetProperty("value");

                    if (value.TryGetProperty("messages", out var messagesArray) && messagesArray.GetArrayLength() > 0)
                    {
                        var message = messagesArray[0];
                        string from = message.GetProperty("from").GetString() ?? "Desconocido";
                        string type = message.GetProperty("type").GetString() ?? "text";
                        string text = "";

                        if (type == "text")
                        {
                            text = message.GetProperty("text").GetProperty("body").GetString() ?? "";
                        }
                        else if (type == "audio")
                        {
                            // En una versión final, aquí descargaríamos el audio usando el media ID
                            // Por ahora, usamos la simulación que configuramos en AIService
                            text = "Audio de WhatsApp"; 
                        }

                        if (!string.IsNullOrEmpty(text))
                        {
                            _logger.LogInformation("Procesando pedido de {From}: {Text}", from, text);
                            await _orderProcessor.ProcessWhatsAppOrderAsync(from, text, type);
                        }
                    }
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando mensaje de WhatsApp");
            return Ok(); // Siempre devolvemos Ok a Meta para evitar que reintente infinitamente
        }
    }
}
