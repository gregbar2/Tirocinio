using Microsoft.AspNetCore.Components.Sections;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class ComputerVisionService
{
    private readonly string _apiKey;
    private readonly string _endpoint;

    public ComputerVisionService(string apiKey, string endpoint)
    {
        _apiKey = apiKey;
        _endpoint = endpoint;
    }

    public async Task<string> GetImageDescriptionAsync(string imageUrl)
    {
        using (var client = new HttpClient())
        {
            // Imposta l'header di autorizzazione
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

            // Endpoint per descrivere l'immagine
            var url = $"{_endpoint}vision/v3.1/describe?maxCandidates=5";

            // Corpo della richiesta (passa l'URL dell'immagine)
            var body = new
            {
                url = imageUrl
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            // Esegui la richiesta
            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode){
                
                var result = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(result);
                /*return jsonResponse.description.captions[0].text;*/
                

                // Analizza oggetti e scene per creare una descrizione più lunga
                var description = new StringBuilder();
                foreach (var caption in jsonResponse.description.captions)
                {
                    description.AppendLine(caption.text.ToString());
                    description.Replace("\n" , " "); // Rimuovi le nuove linee per una descrizione più compatta
                    description.Replace("\r", " "); // Rimuovi i ritorni a capo per una descrizione più compatta
                }


                // Aggiungi ulteriori dettagli su oggetti, scene e altre etichette
                if (jsonResponse.objects != null)
                {
                    description.AppendLine("\nOggetti rilevati nell'immagine:");
                    foreach (var obj in jsonResponse.objects)
                    {
                        description.AppendLine($"- {obj.objectProperty} (probabilità: {obj.confidence})");
                    }
                }

                //aggiungiamo tag per mgiliorare la descrizione delll'immagine
                if (jsonResponse.tags != null)
                {
                    description.AppendLine("\nTag dell'immagine:");
                    foreach (var tag in jsonResponse.tags)
                    {
                        description.AppendLine($"- {tag}");
                    }
                }

                return description.ToString();
            


           }else{

                throw new Exception($"Error: {response.StatusCode}, {response.ReasonPhrase}");
            }
        }
    }
}
