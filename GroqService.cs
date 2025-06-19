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
                    new { role = "system", content = "Sei un assistente che riformula descrizioni di immagini in italiano in modo elegante, mantenendo fedelmente tutte le informazioni fornite. Le immagini raffigurano opere d'arte." },
                    new { role = "user", content = $"Riscrivi questa descrizione in italiano in modo più scorrevole e naturale, ma senza aggiungere dettagli non presenti: {basicDescription}.Dai uno stile alla descrizione come se volessi descrivere un'opera d'arte." }

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
