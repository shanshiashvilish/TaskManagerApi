using TaskManagerApi.Services.Abstractions;

namespace TaskManagerApi.BackgroundJobs;

public class TaskReassignmentBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

            await taskService.ReassignTasksAsync();

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}