using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace Camunda.API.Worker
{
    public class ExternalTaskData
    {
        public string Id { get; set; }
        public string TopicName { get; set; }
        public JObject Variables { get; set; }
    }
    public class ExternalTaskHandlerService
    {
        private readonly HttpClient _httpClient;
        private readonly string _camundaApiUrl; // Replace with your Camunda API URL
        private bool _isRunning = false;

        public ExternalTaskHandlerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _camundaApiUrl = "http://localhost:8080/engine-rest/"; // Replace with your Camunda API URL
        }
        public async Task StartHandlingExternalTasksAsync(CancellationToken cancellationToken)
        {
            if (_isRunning)
            {
                // Already started, do nothing.
                return;
            }

            _isRunning = true;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Fetch external tasks from Camunda
                    var externalTasks = await FetchExternalTasksAsync();
                    foreach (var task in externalTasks)
                    {
                        // Handle the task (in this case, we'll update the salary)
                        await HandleExternalTaskAsync(task);
                    }
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        public async Task FetchAndHandleExternalTasksAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Fetch external tasks from Camunda
                var externalTasks = await FetchExternalTasksAsync();
                foreach (var task in externalTasks)
                {
                    // Handle the task (in this case, we'll update the salary)
                    await HandleExternalTaskAsync(task);
                }

                // Wait for a specific interval before fetching tasks again
               // await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }

        private async Task<List<ExternalTaskData>> FetchExternalTasksAsync()
        {
            var response = await _httpClient.GetAsync($"{_camundaApiUrl}external-task?topicName=updateSalary");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ExternalTaskData>>(content);
        }
        private async Task HandleExternalTaskAsync(ExternalTaskData task)
        {
            // Call the .NET Core API endpoint to update the salary
            var response = await _httpClient.PatchAsync("https://localhost:7094/api/v1/incrementSalary", null);
            response.EnsureSuccessStatusCode();

            // Complete the external task in Camunda after processing
            await CompleteExternalTaskAsync(task.Id);
        }

        private async Task CompleteExternalTaskAsync(string taskId)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_camundaApiUrl}external-task/{taskId}/complete");
            await _httpClient.SendAsync(request);
        }
    }
}
