public class ImageDescriptionService
{
    private readonly ComputerVisionService _computerVisionService;
    private readonly HuggingFaceService _huggingFaceService;

    public ImageDescriptionService(ComputerVisionService computerVisionService, HuggingFaceService huggingFaceService)
    {
        _computerVisionService = computerVisionService;
        _huggingFaceService = huggingFaceService;
    }

    public async Task<string> GetDetailedDescriptionAsync(string imageUrl)
    {
        // Step 1: Ottieni la descrizione base da Azure Computer Vision
        var baseDescription = await _computerVisionService.GetImageDescriptionAsync(imageUrl);

        // Step 2: Espandi la descrizione con Hugging Face
        var detailedDescription = await _huggingFaceService.ExpandDescriptionAsync(baseDescription);

        return detailedDescription;
    }
}
