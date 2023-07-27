using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading;

namespace Camunda.API.Worker
{
    public class ExternalTaskData
    {
        public string Id { get; set; }
        public string TopicName { get; set; }
        public JObject Variables { get; set; }
        public string WorkerId { get; set; }    
    }
    public class ExternalTaskHandlerService
    {
        private readonly HttpClient _httpClient;
        private readonly string _camundaApiUrl; 
        private bool _isRunning = false;

        public ExternalTaskHandlerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _camundaApiUrl = "http://localhost:8080/engine-rest/"; 
        }
        public async Task StartHandlingExternalTasksAsync(CancellationToken cancellationToken)
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var externalTasks = await FetchAndLockExternalTasksAsync();
                    foreach (var task in externalTasks)
                    {
                        await HandleExternalTaskAsync(task);
                    }
                    break;
                }
            }
            finally
            {
                _isRunning = false;
            }
        }
        private async Task<List<ExternalTaskData>> FetchAndLockExternalTasksAsync()
        {
            var fetchAndLockRequest = new
            {
                workerId = Guid.NewGuid().ToString(), 
                maxTasks = 10, 
                topics = new[]
                {
            new
            {
                topicName = "updateSalary",
                lockDuration = 10000,
                variables = new string[] 
                {
                    "variable1",
                    "variable2"
                }
            }
        }
            };

            var jsonRequest = JsonConvert.SerializeObject(fetchAndLockRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_camundaApiUrl}external-task/fetchAndLock", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ExternalTaskData>>(jsonResponse);
        }

        private async Task HandleExternalTaskAsync(ExternalTaskData task)
        {
            var response = await _httpClient.PatchAsync("https://localhost:7094/api/v1/incrementSalary", null);
            response.EnsureSuccessStatusCode();
            await CompleteExternalTaskAsync(task.Id,task.WorkerId,true);
        }
        private async Task CompleteExternalTaskAsync(string taskId, string workerId, bool success, Dictionary<string, object> variables = null, string errorMessage = null)
        {
            var requestUrl = $"{_camundaApiUrl}external-task/{taskId}/complete";

            var completeTaskRequest = new
            {
                workerId,
                variables,
                localVariables = new { }, 
                errorMessage = success ? null : errorMessage
            };

            var jsonRequest = JsonConvert.SerializeObject(completeTaskRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(requestUrl, content);
            response.EnsureSuccessStatusCode();
        }
    }
}
