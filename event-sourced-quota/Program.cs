using System.Text.Json;
using System.Text.Json.Serialization;
using Application;
using Data;
using Domain.Quota;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddApplicationServices(configuration);
builder.Services.AddRepositories(configuration, [new TargetJsonConverter()]);

builder.Services.AddControllers()
       .AddJsonOptions(
           jsonOptions =>
           {   
               jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
               jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
               jsonOptions.JsonSerializerOptions.Converters.Add(new TargetJsonConverter());
           });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

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
