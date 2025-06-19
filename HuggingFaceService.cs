using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class HuggingFaceService
{
    private readonly string _apiKey;

    public HuggingFaceService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<string> ExpandDescriptionAsync(string baseDescription)
    {
        using (var client = new HttpClient())
        {
            // Aggiungi la chiave API come header
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var url = "https://huggingface.co/Salesforce/blip-image-captioning-base";  // Usa il modello di Image Captioning di Hugging Face

            var body = new
            {
                inputs = baseDescription  // Passa la descrizione base ottenuta da Azure
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(result);
                return jsonResponse[0].generated_text.ToString();  // Risultato della descrizione dettagliata
            }
            else
            {
                var errorResult = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error response: {errorResult}");
                throw new Exception($"Error: {response.StatusCode}, {response.ReasonPhrase}, Response: {errorResult}");
            }
        }
    }
}
