using Newtonsoft.Json;

namespace Camunda.API.Worker
{
    public class ExternalTaskHandlerHostedService : BackgroundService
    {
        private readonly ExternalTaskHandlerService _externalTaskHandlerService;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly HttpClient _httpClient;
        private readonly string _camundaApiUrl;

        public ExternalTaskHandlerHostedService(ExternalTaskHandlerService externalTaskHandlerService, IHttpClientFactory httpClientFactory)
        {
            _externalTaskHandlerService = externalTaskHandlerService;
            _cancellationTokenSource = new CancellationTokenSource();
            _httpClient = httpClientFactory.CreateClient();
            _camundaApiUrl = "http://localhost:8080/engine-rest/";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var timerEventOccurred = await CheckTimerEventStatusAsync();

                if (timerEventOccurred)
                {
                    await _externalTaskHandlerService.StartHandlingExternalTasksAsync(_cancellationTokenSource.Token);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            await base.StopAsync(cancellationToken);
        }

        private async Task<bool> CheckTimerEventStatusAsync()
        {
            var response = await _httpClient.GetAsync($"{_camundaApiUrl}history/activity-instance?activityId=Event_0ajqwj4&finished=true");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var activities = JsonConvert.DeserializeObject<List<ActivityInstance>>(content);
            return activities.Any();
        }
    }

    public class ActivityInstance
    {
        public string Id { get; set; }
        public string ActivityId { get; set; }
    }

}
