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
        _logger.LogInformation("--- WEBHOOK RECIBIDO ---");
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            _logger.LogInformation("Cuerpo: {Body}", body);

            using var jsonDoc = JsonDocument.Parse(body);
            var root = jsonDoc.RootElement;

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
            _logger.LogError(ex, "Error en el Webhook");
            return Ok(); 
        }
    }
}
