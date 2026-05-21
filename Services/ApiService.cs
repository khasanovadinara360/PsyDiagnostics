using PsyDiagnostics.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PsyDiagnostics.Services
{
    public class ApiService
    {
        private readonly HttpClient _http = new();

        private const string ApiUrl = "http://127.0.0.1:8000/predict";

        public async Task<PredictionResponse> GetFullPrediction(PredictionRequest data)
        {
            var json = JsonSerializer.Serialize(data);

            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _http.PostAsync(ApiUrl, content);

            response.EnsureSuccessStatusCode();

            var resultJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<PredictionResponse>(resultJson)
                   ?? new PredictionResponse();
        }

        public async Task<int> GetPrediction(PredictionRequest data)
        {
            var result = await GetFullPrediction(data);

            return result.Prediction;
        }
    }
}