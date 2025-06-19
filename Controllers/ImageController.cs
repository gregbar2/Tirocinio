using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using ImageDescriptionApp;
//using ImageDescriptionApp.Services;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;
using static Google.Rpc.Context.AttributeContext.Types;

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

        [HttpPost("describe-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> DescribeImage([FromForm] ImageUploadRequest request)
        {
            var image = request.Image;

            if (image == null || image.Length == 0)
                return BadRequest("Nessun file immagine caricato.");

            try
            {
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var basicDescription = await _computerVisionService.GetImageDescriptionAsync(memoryStream);
                var improvedDescription = await _groqService.ImproveDescriptionAsync(basicDescription);

                return Ok(new
                {
                    AzureDescription = basicDescription,
                    GroqEnhancedDescription = improvedDescription
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore: {ex.Message}");
            }
        }
    }
}




