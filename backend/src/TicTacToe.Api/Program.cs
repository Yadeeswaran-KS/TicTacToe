using System.Text.Json.Serialization;
using TicTacToe.Api.Filters;
using TicTacToe.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)))
    .AddMvcOptions(options => options.Filters.Add<ApiExceptionFilter>());
builder.Services.AddSingleton<GameService>();
builder.Services.AddCors(options =>
    options.AddPolicy("AngularFrontend", policy =>
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AngularFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}
