using PsyDiagnostics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PsyDiagnostics.Services
{
    public class ApiService
    {
        private readonly HttpClient _http = new HttpClient();

        public async Task<int> GetPrediction(PredictionRequest data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("http://127.0.0.1:8000/predict", content);

            response.EnsureSuccessStatusCode();

            var resultJson = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<PredictionResponse>(resultJson);

            return result.prediction;
        }
    }
}