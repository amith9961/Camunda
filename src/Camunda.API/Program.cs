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
