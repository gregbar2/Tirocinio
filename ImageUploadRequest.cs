using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace ImageDescriptionApp
{
    public class ImageUploadRequest
    {
        [FromForm(Name = "image")]
        public IFormFile Image { get; set; }
    }

}
