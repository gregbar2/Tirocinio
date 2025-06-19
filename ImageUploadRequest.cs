using Microsoft.AspNetCore.Http;


namespace ImageDescriptionApp
{
    public class ImageUploadRequest
    {
        public IFormFile Image { get; set; }
    }

}
