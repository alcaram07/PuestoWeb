using Microsoft.AspNetCore.Mvc;
using PuestoWeb.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;

namespace PuestoWeb.Controllers;

[ApiController]
[Route("api/whatsapp")]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
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

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("El webhook está vivo y accesible.");
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
        Console.WriteLine("[DEBUG-CONTROLLER] Recibida llamada POST a Receive()");
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            
            // LOG CRÍTICO: Ver todo lo que Meta envía
            _logger.LogInformation("--- WEBHOOK RECIBIDO ---");
            _logger.LogInformation("Cuerpo: {Body}", body);
            Console.WriteLine($"[DEBUG-BODY] {body}");

            using var jsonDoc = JsonDocument.Parse(body);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("entry", out var entryArray) && entryArray.GetArrayLength() > 0)
            {
                var value = entryArray[0].GetProperty("changes")[0].GetProperty("value");

                // Verificar si es un MENSAJE o solo un cambio de ESTADO (leído/entregado)
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
                        text = "Pedido por audio"; 
                    }

                    _logger.LogInformation("Extrayendo pedido de {From}: {Text}", from, text);
                    
                    if (!string.IsNullOrEmpty(text))
                    {
                        var pedido = await _orderProcessor.ProcessWhatsAppOrderAsync(from, text, type);
                        if (pedido != null)
                        {
                            _logger.LogInformation("PEDIDO CREADO EXITOSAMENTE: ID {Id}", pedido.Id);
                        }
                        else
                        {
                            _logger.LogWarning("No se pudo crear el pedido (posiblemente no se encontraron artículos)");
                        }
                    }
                }
                else if (value.TryGetProperty("statuses", out _))
                {
                    _logger.LogInformation("Notificación de estado recibida (Entregado/Leído). Ignorando.");
                }
            }

            return Ok(); // Siempre 200 OK para que Meta no se queje
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando el Webhook");
            return Ok(); 
        }
    }
}
