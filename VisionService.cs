using Google.Cloud.Vision.V1;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageDescriptionApp.Services
{
    public class VisionService
    {
        public async Task<string> DescribeImageAsync(Stream imageStream)
        {
            // Carica immagine da stream
            Image image = Image.FromStream(imageStream);

            // Crea client Vision
            var client = await ImageAnnotatorClient.CreateAsync();

            // Rileva etichette (label detection)
            IReadOnlyList<EntityAnnotation> labels = await client.DetectLabelsAsync(image);

            if (labels.Count == 0)
                return "Nessuna descrizione trovata.";

            // Costruisci frase descrittiva
            return "L'immagine contiene: " + string.Join(", ", labels.Select(l => l.Description));
        }
    }
}
