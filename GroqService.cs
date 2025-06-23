namespace ImageDescriptionApp;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

public class GroqService
{
    private readonly string _apiKey;
    private readonly string _endpoint = "https://api.groq.com/openai/v1/chat/completions";

    public GroqService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<string> ImproveDescriptionAsync(string basicDescription)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var requestBody = new
            {
                model = "llama3-70b-8192",
                messages = new[]
                {
                    new { role = "system", content = "Sei un assistente che riformula descrizioni di immagini in italiano in modo elegante, mantenendo fedelmente tutte le informazioni fornite. Le immagini raffigurano opere d'arte." },
                    new { role = "user", content = $"Riscrivi questa descrizione in italiano in modo più scorrevole e naturale, ma senza aggiungere dettagli non presenti: {basicDescription}.Dai uno stile alla descrizione come se volessi descrivere un'opera d'arte, in particolare un dipinto.Prova a dedurre il nome dell'opera d'arte descritta, l'autore, il genere, l'anno di realizzazione, i materiali, il movimento artistico e le dimensioni." }

                     }
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_endpoint, content);
            var result = await response.Content.ReadAsStringAsync();

            dynamic jsonResponse = JsonConvert.DeserializeObject(result);
            return jsonResponse.choices[0].message.content;
        }
    }

    public async Task<string> GenerateDescriptionFromTags(List<string> tags)
    {
        string prompt = $"Genera una descrizione di un'immagine che contiene: {string.Join(", ", tags)}.Non aggiungere elementi non presenti o dettagli non veri.La descrizione deve essere di 3 righe al massimo.Cerca di estrapolare dall'immagine le informazioni sul colore dell'oggetto, le sue dimensioni.";

        var requestBody = new
        {
            messages = new[]
            {
            new { role = "user", content = prompt }
        },
            model = "meta-llama/llama-4-scout-17b-16e-instruct" // o il modello Groq che stai usando
        };

        var requestJson = System.Text.Json.JsonSerializer.Serialize(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "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");
        request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await new HttpClient().SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseContent);
        return doc.RootElement
                  .GetProperty("choices")[0]
                  .GetProperty("message")
                  .GetProperty("content")
                  .GetString();
    }
}
