using System.IO;
using System.Threading.Tasks;
using ImageDescriptionApp;
//using ImageDescriptionApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ClarifaiClient _clarifaiClient;
        private readonly ComputerVisionService _computerVisionService;
        private readonly GroqService _groqService;

        // Inietta i servizi nel controller
        public ImageController(ComputerVisionService computerVisionService, GroqService groqService, ClarifaiClient clarifaiClient)
        {
            _computerVisionService = computerVisionService;
            _groqService = groqService;
            _clarifaiClient = clarifaiClient;
        }

        [HttpPost("describe-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> DescribeImage([FromForm] ImageUploadRequest request)
        {
            var image = request.Image;

            if (image == null || image.Length == 0)
                return BadRequest("Nessun file immagine caricato.");

            try
            {
                // Carica l'immagine come MemoryStream
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);

                // Ottenere la descrizione da Clarifai
                memoryStream.Position = 0; // Assicurati che il flusso sia riutilizzabile
                var clarifaiRawJson = await _clarifaiClient.GetClarifaiRawResponse(memoryStream);
                var tags = _clarifaiClient.GetTagsFromClarifaiJson(clarifaiRawJson);
                var groqEnhancedDescription = await _groqService.GenerateDescriptionFromTags(tags);


                // Riporta il puntatore all'inizio del MemoryStream per Clarifai
                // memoryStream.Position = 0;
                // Ottenere la descrizione base da Azure (Computer Vision)
                //  var basicDescription = await _computerVisionService.GetImageDescriptionAsync(memoryStream);

                // Migliorare la descrizione tramite Groq
                //  var improvedDescription = await _groqService.ImproveDescriptionAsync(basicDescription);

                // Restituire una risposta con le descrizioni da tutti i servizi
                return Ok(new
                {
                    //   AzureDescription = basicDescription,
                    //   ClarifaiDescription = clarifaiDescription,
                    //  GroqEnhancedDescription = improvedDescription
                    ClarifaiTags = tags,
                    GroqEnhancedDescription = groqEnhancedDescription
                });
            }
            catch (Exception ex)
            {
                // Gestione degli errori
                return StatusCode(500, $"Errore durante l'elaborazione: {ex.Message}");
            }
        }
    }
}
