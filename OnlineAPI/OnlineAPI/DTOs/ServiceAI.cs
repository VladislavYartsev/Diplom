using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace OnlineAPI.DTOs
{
    public class ServiceAI

    {
        private readonly HttpClient _httpClient;

        public ServiceAI(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> PredictPriorityAsync(string title, string description)
        {
            var requestObj = new
            {
                title = title,
                description = description
            };

            var json = JsonSerializer.Serialize(requestObj);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://localhost:8000/predict", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var taskId = doc.RootElement.GetProperty("task_id").GetString();

            string result = null;
            while (true)
            {
                var statusResponse = await _httpClient.GetAsync($"http://localhost:8000/predict/{taskId}");
                statusResponse.EnsureSuccessStatusCode();

                var statusJson = await statusResponse.Content.ReadAsStringAsync();
                using var statusDoc = JsonDocument.Parse(statusJson);

                if (statusDoc.RootElement.TryGetProperty("priority", out var priorityElement))
                {
                    result = priorityElement.GetString();
                    break;
                }

                await Task.Delay(1000);
            }

            return result;
        }
    }
}

