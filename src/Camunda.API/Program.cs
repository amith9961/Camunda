using Camunda.API.Worker;
using Camunda.Domain.Service;
using Data.GraphQL.Repository;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using MediatR;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSingleton<SalaryRepository>();
builder.Services.AddSingleton <GraphQLHttpClient>(options =>
{
    var option = new GraphQLHttpClientOptions
    {
        EndPoint = new Uri("http://localhost:8090/v1/graphql"),
    };
    var client = new GraphQLHttpClient(option, new NewtonsoftJsonSerializer());
    return client;
});
builder.Services.AddHttpClient();

// Register the ExternalTaskHandlerService as a hosted service
builder.Services.AddSingleton<ExternalTaskHandlerService>();
builder.Services.AddHostedService<ExternalTaskHandlerHostedService>();
builder.Services.AddSingleton<GraphQLRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.Run();

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
        _camundaApiUrl = "http://localhost:8080/engine-rest/"; // Replace with your Camunda API URL
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Poll Camunda every 5 seconds to check if the timer event has occurred
        while (!stoppingToken.IsCancellationRequested)
        {
            var timerEventOccurred = await CheckTimerEventStatusAsync();

            if (timerEventOccurred)
            {
                // Call the ExternalTaskHandlerService to start handling external tasks
                await _externalTaskHandlerService.StartHandlingExternalTasksAsync(_cancellationTokenSource.Token);
                break; // Exit the loop since the timer event occurred
            }

            // Wait for 5 seconds before checking again
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // Stop the ExternalTaskHandlerService when this hosted service is stopped
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
