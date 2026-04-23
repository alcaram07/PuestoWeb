using Microsoft.AspNetCore.Mvc;
using PuestoWeb.Services;
using System.Text.Json;

namespace PuestoWeb.Controllers;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppController : ControllerBase
{
    private readonly AIService _aiService;
    private readonly OrderProcessorService _orderProcessor;
    private readonly ILogger<WhatsAppController> _logger;
    private const string VERIFY_TOKEN = "puesto_token_2024"; // Este es el token que pondrás en Meta

    public WhatsAppController(AIService aiService, OrderProcessorService orderProcessor, ILogger<WhatsAppController> logger)
    {
        _aiService = aiService;
        _orderProcessor = orderProcessor;
        _logger = logger;
    }

    // Verificación del Webhook (requerido por Meta al configurar)
    [HttpGet]
    public IActionResult Verify()
    {
        var hubMode = Request.Query["hub.mode"].ToString();
        var hubToken = Request.Query["hub.verify_token"].ToString();
        var hubChallenge = Request.Query["hub.challenge"].ToString();

        if (hubMode == "subscribe" && hubToken == VERIFY_TOKEN)
        {
            _logger.LogInformation("Webhook de WhatsApp verificado con éxito.");
            return Ok(hubChallenge);
        }

        return Forbid();
    }

    // Recepción de mensajes
    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        _logger.LogInformation("Mensaje de WhatsApp recibido: {Body}", body);

        try
        {
            // Nota: Aquí se debería deserializar el JSON de Meta (WhatsApp Business API)
            // Para esta implementación, asumimos una estructura simplificada que puedes adaptar
            using var jsonDoc = JsonDocument.Parse(body);
            var root = jsonDoc.RootElement;
            
            // Lógica para extraer el texto o el ID del audio del JSON de Meta
            // Esto varía según si usas Twilio o la API Directa de Meta.
            
            string clienteTelefono = "Desconocido";
            string contenidoMensaje = "";
            bool esAudio = false;

            // Intento básico de extraer datos (esto es un esquema general)
            if (body.Contains("text")) 
            {
                // Es un mensaje de texto
                contenidoMensaje = "Extracción de texto del JSON de Meta"; 
            }
            else if (body.Contains("audio"))
            {
                esAudio = true;
                // En el caso de audio, Meta envía un ID. 
                // Deberíamos descargar el archivo y pasarlo al AIService.
                contenidoMensaje = await _aiService.TranscribeAudioAsync(Stream.Null, "whatsapp_audio.ogg");
            }

            if (!string.IsNullOrEmpty(contenidoMensaje))
            {
                var items = await _aiService.InterpretOrderAsync(contenidoMensaje);
                // Aquí podrías enviar una respuesta automática al cliente confirmando el pedido
                _logger.LogInformation("Pedido interpretado vía WhatsApp: {Count} artículos", items.Count);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando Webhook de WhatsApp");
            return BadRequest();
        }
    }
}
