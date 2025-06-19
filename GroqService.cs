namespace ImageDescriptionApp;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
                    new { role = "system", content = "Sei un assistente che descrive immagini in modo molto dettagliato e accurato." },
                    new { role = "user", content = $"Puoi migliorare questa descrizione dell'immagine e renderla più dettagliata, mantenendo il significato: {basicDescription}" }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_endpoint, content);
            var result = await response.Content.ReadAsStringAsync();

            dynamic jsonResponse = JsonConvert.DeserializeObject(result);
            return jsonResponse.choices[0].message.content;
        }
    }
}
