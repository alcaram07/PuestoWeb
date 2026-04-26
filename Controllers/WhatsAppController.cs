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

    // NUEVO: Link de Súper-Prueba
    [HttpGet("super-test")]
    public async Task<IActionResult> SuperTest()
    {
        _logger.LogInformation("Lanzando Súper-Test manual...");
        var pedido = await _orderProcessor.ProcessWhatsAppOrderAsync("59899097344", "2kg de Manzana Roja", "text");
        if (pedido != null)
        {
            return Ok($"¡ÉXITO! Se ha creado el Pedido ID {pedido.Id} con un total de ${pedido.Total}. Ve a /Admin/Pedidos para verlo.");
        }
        return BadRequest("No se pudo crear el pedido. ¿Estás seguro de que tienes un artículo llamado 'Manzana Roja' en el catálogo?");
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
            
            _logger.LogInformation("--- WEBHOOK RECIBIDO ---");
            _logger.LogInformation("Cuerpo: {Body}", body);

            using var jsonDoc = JsonDocument.Parse(body);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("entry", out var entryArray) && entryArray.GetArrayLength() > 0)
            {
                var value = entryArray[0].GetProperty("changes")[0].GetProperty("value");

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

                    _logger.LogInformation("Procesando pedido de {From}: {Text}", from, text);
                    
                    if (!string.IsNullOrEmpty(text))
                    {
                        var pedido = await _orderProcessor.ProcessWhatsAppOrderAsync(from, text, type);
                        if (pedido != null)
                        {
                            _logger.LogInformation("PEDIDO CREADO EXITOSAMENTE: ID {Id}", pedido.Id);
                        }
                    }
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando el Webhook");
            return Ok(); 
        }
    }
}
