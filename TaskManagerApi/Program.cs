using Microsoft.EntityFrameworkCore;
using TaskManagerApi.BackgroundJobs;
using TaskManagerApi.Data;
using TaskManagerApi.Seeding;
using TaskManagerApi.Services;
using TaskManagerApi.Services.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TaskDb"));
builder.Services.AddScoped<Seeder>();
builder.Services.AddHostedService<TaskReassignmentBackgroundService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// DB seeding logic
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
    seeder.Seed();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();