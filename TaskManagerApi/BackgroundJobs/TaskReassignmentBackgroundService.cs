using TaskManagerApi.Services.Abstractions;

namespace TaskManagerApi.BackgroundJobs;

public class TaskReassignmentBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<TaskReassignmentBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

            logger.LogInformation("Start reassignment at {Time}", DateTime.UtcNow);
            await taskService.ReassignTasksAsync();
            logger.LogInformation("End reassignment at {Time}", DateTime.UtcNow);

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}