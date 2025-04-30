using TaskManagerApi.Services.Abstractions;

namespace TaskManagerApi.Background;

public class TaskReassignmentBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

            // implement logic in taskService

            await Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
        }
    }
}