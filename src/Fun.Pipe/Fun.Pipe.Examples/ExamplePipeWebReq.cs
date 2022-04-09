using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
namespace Fun.Pipe.Examples;

internal static class ExamplePipeWebReq
{
    static readonly HttpClient client = new();
    static readonly (double flip, string wizardGuid)[] guidsAndFlips = new[]
    {
        (0.2, "Morgana-77"),            // willResultInNotFound
        (0.4, "Shadow Shaman-34"),      // will make it through
        (0.8, "Merlin-42"),             // will make it through
        (0.8, "badresponsecontent"),    // willResultInDeserializationError
        (0.5, "Morgana-notanumber"),    // willResultInDeserializationError
        (0.8, "Gandalf-54"),            // will make it through
    };
    
    
    // Variants
    static async Task<bool> Imperative(double flip, string wizardGuid)
    {
        static async Task<Wizard> GetWizardImperative(double flip, string wizardGuid)
        {
            // below lines aim to simulate the failure possilibity of the request, which hopefully is smaller than 0.25 in real life
            bool willResultInNotFound = flip < 0.25;
            string url = willResultInNotFound ? "https://httpbin.org/status/404" : $"https://httpbin.org/anything?data={wizardGuid}";

            // simulate a request to get the object
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(url);
            }
            catch (HttpRequestException e)
            {
                Log($"wizard request failed: {e.Message}");
                return null;    // don't know what to return!
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log($"wizard request failed, status code: {response.StatusCode}");
                return null;    // not really a good idea! maybe `response.EnsureSuccessStatusCode()` can be called to throw instead; but is this meathod allowed to throw here?
                                // actually, it is the caller who must decide what to do with the result.
            }

            // lets simulate deserialization with a simple dash-separated parser
            string content = await response.Content.ReadAsStringAsync();
            var jObj = (JObject)JToken.Parse(content);
            string data = (string)jObj["args"]["data"];
            try
            {
                var wizard = FakeWizardDeserializer(data);
                return wizard;
            }
            catch (Exception e)
            {
                Log($"wizard deserialization failed: {e.Message}");
                return null;    // again, don't know what to return!
            }
        }
        static async Task<bool> UpdateWizardImperative(double flip, string wizardGuid, Wizard updatedWizard)
        {
            // below lines aim to simulate the failure possilibity of the request
            bool willResultInForbidden = flip < 0.25;
            string url = willResultInForbidden ? "https://httpbin.org/status/403" : "https://httpbin.org/status/200";

            // post the updated wizard
            var content = new StringContent($"{wizardGuid}-{updatedWizard.NbSpells}");
            try
            {
                var response = await client.PostAsync(url, content);
                return response.StatusCode == HttpStatusCode.OK; // misuse bool as the status
            }
            catch (Exception e)
            {
                Log($"wizard could not be updated: {e.Message}");
                return false;
            }
        }
        // Run
        var wizard = await GetWizardImperative(flip, wizardGuid);
        if (wizard == null) // misuse of null
            return false;   // misuse of bool as status
        var updatedWizard = DuelBalrogDemon(wizard);
        bool pushed = await UpdateWizardImperative(flip, wizardGuid, wizard);
        return pushed;
    }
    static async Task<Res> Pipe(double flip, string wizardGuid)
    {
        static async Task<Res<Wizard>> GetWizard(double flip, string wizardGuid)
        {
            // below lines aim to simulate the failure possilibity of the request, which hopefully is smaller than 0.25 in real life
            bool willResultInNotFound = flip < 0.25;
            string url = willResultInNotFound ? "https://httpbin.org/status/404" : $"https://httpbin.org/anything?data={wizardGuid}";

            // simulate a request to get the object
            var response = await TryMapAsync(() => client.GetAsync(url)); // HttpRequestException will be caught if there is a connection error
            var okResponse = response.ResFromStatus($"wizard request failed"); // ResFromStatus has special overloads for HttpResponseMessage accepting only 200-OK as Ok, and any other code as Err
            var content = await okResponse.TryMapAsync(response => response.Content.ReadAsStringAsync());

            // lets simulate deserialization with a simple dash-separated parser
            return content.TryMap(c => // the lambda is executed only if `content.IsOk`
            {
                var jObj = (JObject)JToken.Parse(c);
                string data = (string)jObj["args"]["data"];
                return FakeWizardDeserializer(data);
            });
        }

        static async Task<Res<HttpResponseMessage>> UpdateWizard(double flip, string wizardGuid, Wizard updatedWizard)
        {
            // below lines aim to simulate the failure possilibity of the request
            bool willResultInForbidden = flip < 0.25;
            string url = willResultInForbidden ? "https://httpbin.org/status/403" : "https://httpbin.org/status/200";

            // post the updated wizard
            var content = new StringContent($"{wizardGuid}-{updatedWizard.NbSpells}");
            var response = await TryMapAsync(() => client.PostAsync(url, content));
            return response.Map(x => x.ResFromStatus("wizard could not be updated"));
        }


        // Run
        var wizard = await GetWizard(flip, wizardGuid);
        var pushed = await wizard.Map(w => DuelBalrogDemon(w)).MapAsync(w => UpdateWizard(flip, wizardGuid, w));
        return pushed.AsRes();
    }
    
    
    // Helpers
    static Wizard FakeWizardDeserializer(string str)
    {
        var parts = str.Split('-');
        return new(Name: parts[0], NbSpells: int.Parse(parts[1]));
    }
    static Wizard DuelBalrogDemon(Wizard wizard)
    {
        double winProb = (double)wizard.NbSpells / 100.0;
        bool wins = (new Random()).NextDouble() < winProb;
        return wins ? (wizard with { NbSpells = wizard.NbSpells + 10 }) : (wizard with { NbSpells = 0 });
    }
    
    
    // Run
    internal static void Run()
    {
        Log($"\n\n\n--- {nameof(ExamplePipeWebReq)} ---");
        var rand = new Random();
        int wizardIndex = rand.Next(0, guidsAndFlips.Length);
        (double flip, string wizardGuid) = guidsAndFlips[wizardIndex];
        Log($"(flip, wizard) = ({flip}, {wizardGuid})");

        RunExample("Imperative", () => Log($"{Imperative(flip, wizardGuid).GetAwaiter().GetResult()}"));
        RunExample("Pipe", () => Log($"{Pipe(flip, wizardGuid).GetAwaiter().GetResult()}"));
    }
}
