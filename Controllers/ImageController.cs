using System.Threading.Tasks;
using ImageDescriptionApp;
using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    /*
     [Route("api/[controller]")]//route base per il controller
     [ApiController]
     public class ImageController : ControllerBase
     {
         private readonly ComputerVisionService _computerVisionService;

         // Inietta il servizio ComputerVisionService nel controller
         public ImageController(ComputerVisionService computerVisionService)
         {
             _computerVisionService = computerVisionService;
         }

         // Endpoint per ottenere la descrizione di un'immagine
         [HttpPost("describe-image")]
         public async Task<IActionResult> DescribeImage([FromBody] string imageUrl)
         {
             if (string.IsNullOrEmpty(imageUrl))
             {
                 return BadRequest("Image URL is required.");
             }

             try
             {
                 // Ottieni la descrizione dell'immagine
                 var description = await _computerVisionService.GetImageDescriptionAsync(imageUrl);

                 // Restituisci la descrizione dell'immagine come risposta
                 return Ok(new { Description = description });
             }
             catch (Exception ex)
             {
                 // Gestisci gli errori e restituisci un errore HTTP
                 return StatusCode(500, $"Error: {ex.Message}");
             }
         }
     }*/


    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ImageDescriptionService _imageDescriptionService;

        // Iniezione del servizio
        public ImageController(ImageDescriptionService imageDescriptionService)
        {
            _imageDescriptionService = imageDescriptionService;
        }

        [HttpPost("describe-image")]
        public async Task<IActionResult> DescribeImage([FromBody] string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return BadRequest("Image URL is required.");
            }

            try
            {
                var detailedDescription = await _imageDescriptionService.GetDetailedDescriptionAsync(imageUrl);
                return Ok(new { DetailedDescription = detailedDescription });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }




}
