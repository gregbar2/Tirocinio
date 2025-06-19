using System.Threading.Tasks;
using ImageDescriptionApp;
using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ComputerVisionService _computerVisionService;
        private readonly GroqService _groqService;

        // Inietta entrambi i servizi nel controller
        public ImageController(ComputerVisionService computerVisionService, GroqService groqService)
        {
            _computerVisionService = computerVisionService;
            _groqService = groqService;
        }

        // Endpoint per ottenere la descrizione dell'immagine + versione migliorata da Groq
        [HttpPost("describe-image")]
        public async Task<IActionResult> DescribeImage([FromBody] string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return BadRequest("Image URL is required.");
            }

            try
            {
                // Step 1: Ottieni la descrizione base da Azure
                var basicDescription = await _computerVisionService.GetImageDescriptionAsync(imageUrl);

                // Step 2: Passa la descrizione base a Groq per migliorarla
                var improvedDescription = await _groqService.ImproveDescriptionAsync(basicDescription);

                // Step 3: Restituisci entrambe
                return Ok(new
                {
                    AzureDescription = basicDescription,
                    GroqEnhancedDescription = improvedDescription
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
