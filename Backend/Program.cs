using Backend.Data;
using Backend.Services;
using Backend.AppOptions;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Exceptions;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

var origins = builder.Configuration
    .GetSection("Cors:AlloedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("RoomReservationDb"));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services
    .AddOptions<ReservationOptions>()
    .Bind(builder.Configuration.GetSection(ReservationOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => 
        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));


var app = builder.Build();

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    DbSeeder.Seed(scope.ServiceProvider.GetRequiredService<AppDbContext>());
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseCors("Frontend");  

app.Run();

public partial class Program { }