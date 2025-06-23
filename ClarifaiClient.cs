using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ClarifaiClient
{
    private readonly string apiKey;
    private readonly HttpClient client;

    public ClarifaiClient(string apiKey)
    {
        this.apiKey = apiKey;
        this.client = new HttpClient(); // Crea un'istanza di HttpClient
    }

    // Metodo per inviare l'immagine a Clarifai e ottenere la descrizione
    public async Task<string> GetImageDescription(Stream imageStream)
    {
        try
        {
            // Converte lo Stream in un array di byte
            byte[] imageBytes = ReadStream(imageStream);
            string base64Image = Convert.ToBase64String(imageBytes);


            // URL corretto per il modello "General" di Clarifai
            var url = "https://api.clarifai.com/v2/users/greg/apps/MyImageRecognitionApp/workflows/image-description-workflow/results";

            // Crea la richiesta HTTP
            /*
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Headers =
                {
                    // Aggiungi l'header di autenticazione con la chiave API
                    Authorization = new AuthenticationHeaderValue("Bearer", apiKey)
                }
            };*/
            var requestBody = new
            {
                inputs = new[]
                {
                    new
                    {
                        data = new
                        {
                            image = new
                            {
                                base64 = base64Image
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

            // Crea il contenuto della richiesta con l'immagine

            /*
            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(imageBytes), "file", "image.jpg"); // Usa un nome di file adeguato

            request.Content = content;
            */
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Key", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // Invia la richiesta HTTP
            var response = await client.SendAsync(request);

            // Se la richiesta è riuscita, leggi la risposta
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return ParseDescriptionFromClarifaiJson(result); // Restituisce il risultato come stringa JSON
            }
            else
            {
                // Aggiungi il corpo della risposta per ottenere più dettagli sull'errore
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"Errore nella chiamata a Clarifai: {response.ReasonPhrase}. Dettagli: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            return $"Errore: {ex.Message}";
        }
    }

    // Funzione di utilità per leggere uno Stream in un array di byte
    private byte[] ReadStream(Stream stream)
    {
        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }

    private string ParseDescriptionFromClarifaiJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);

            var concepts = doc.RootElement
                .GetProperty("results")[0]
                .GetProperty("outputs")[0]
                .GetProperty("data")
                .GetProperty("concepts");

            var tags = concepts
                .EnumerateArray()
                .Select(c => c.GetProperty("name").GetString())
                .ToList();

            if (tags.Count == 0)
                return "Nessuna descrizione disponibile.";

            var description = $"L'immagine contiene: {string.Join(", ", tags)}.";
            return description;
        }
        catch
        {
            return "Errore nell'interpretazione della risposta Clarifai.";
        }
    }

    public List<string> GetTagsFromClarifaiJson(string json)
    {
        var tags = new List<string>();

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                return tags;

            var output = results[0].GetProperty("outputs")[0];
            var data = output.GetProperty("data");

            if (!data.TryGetProperty("concepts", out var concepts))
                return tags;

            tags = concepts
                .EnumerateArray()
                .Select(c => c.GetProperty("name").GetString())
                .ToList();
        }
        catch (Exception ex)
        {
            // logga o ignora
        }

        return tags;
    }



    public async Task<string> GetClarifaiRawResponse(Stream imageStream)
    {
        try
        {
            byte[] imageBytes = ReadStream(imageStream);
            string base64Image = Convert.ToBase64String(imageBytes);

            var url = "https://api.clarifai.com/v2/users/greg/apps/MyImageRecognitionApp/workflows/image-description-workflow/results";

            var requestBody = new
            {
                inputs = new[]
                {
                new
                {
                    data = new
                    {
                        image = new
                        {
                            base64 = base64Image
                        }
                    }
                }
            }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Key", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Errore Clarifai: {response.StatusCode}. {responseString}");

            return responseString;
        }
        catch (Exception ex)
        {
            return $"{{ \"errore\": \"{ex.Message}\" }}";
        }
    }


}
