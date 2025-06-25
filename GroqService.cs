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

    //attualmente questo metodo non è utilizzato, ma lo lascio per completezza
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

    // 
    //Puoi usare la descrizione di Azure solo come riferimento di supporto, senza copiarla né amplificarla.

    //metodo per generare una descrizione basata sui tag ottenuti da Clarifai
    public async Task<string> GenerateDescriptionFromTags(ImageAnalysisResult analysis, String basicDescription)
    {
        /*
        string prompt = $"Hai a disposizione due fonti per descrivere un'immagine: i tag di riconoscimento oggettivi: {string.Join(",", tags)}, e" +
            $"la descrizione:{basicDescription}." +
            $"Basati solo sui dati forniti per generare una descrizione oggettiva, evitando invenzioni o aggettivi inutili." +
            $"Se puoi dedurre il colore principale o le caratteristiche visive dominanti, inseriscile. Cerca di indivduare le caratteristiche che contraddistinguono quell'immagine." +
            $"La descrizione deve essere chiara, concisa e non superare le 3 righe." +
            $"Producimi solo la descrizione senza aggiungere nient'altro." +
            $"Se descrizione e tag sono discordanti basati solo sui tag.";
        */
        var requestBody = new
        {
            messages = new[]
            {
            new { role = "user", content = BuildPromptForGroq(analysis,basicDescription)}
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




    public string BuildPromptForGroq(ImageAnalysisResult analysis,String basicDescription)
    {
        var tags = string.Join(", ", analysis.Tags.Distinct());
        var colors = string.Join(", ", analysis.Colors.Distinct());

        var prompt = $@"
            Hai a disposizione una serie di tag visivi e colori rilevati da un sistema di image recognition.

            Tag rilevati: {tags}.
            Colori principali: {colors}.
            Inoltre hai a disposizione una descrizione di base dell'immagine: {basicDescription}.


            Descrivi l’immagine in modo oggettivo, senza aggiungere dettagli inventati o aggettivi inutili. Deduci se possibile l’oggetto principale, il colore dominante e le caratteristiche visive più evidenti. La descrizione deve essere chiara e sintetica, massimo 3 righe.
            ";

        return prompt.Trim();
    }
}
