using TaskManagerApi.Data;
using TaskManagerApi.Enums;
using TaskManagerApi.Models;

namespace TaskManagerApi.Seeding;

public class Seeder(AppDbContext db, ILogger<Seeder> logger)
{
    private List<User> _users = [];
    private List<TaskItem> _tasks = [];

    public void Seed()
    {
        AddUsers();
        AddTasks();

        logger.LogInformation("Seeding {UsersCount} users and {TasksCount} tasks.", _users.Count, _tasks.Count);

        db.SaveChanges();
    }

    #region Users

    private void AddUsers()
    {
        if (!db.Users.Any())
        {
            _users =
            [
                new User { Name = "George" },
                new User { Name = "Mary" },
                new User { Name = "Alex" },
                new User { Name = "Anna" },
                new User { Name = "Bob" }
            ];

            db.Users.AddRange(_users);
        }
        else
        {
            _users = db.Users.ToList();
        }
    }

    private void AddTasks()
    {
        if (db.Tasks.Any()) return;

        _tasks =
        [
            new TaskItem { Title = "Write tests", State = TaskState.Waiting },
            new TaskItem { Title = "Document endpoints", State = TaskState.Waiting },
            new TaskItem { Title = "Update projects", State = TaskState.Waiting },
            new TaskItem { Title = "Delete unused files", State = TaskState.Waiting },
            new TaskItem { Title = "Clean up models", State = TaskState.Waiting }
        ];

        // Try to assign to users with zero assigned tasks
        var userTaskCounts = new Dictionary<Guid, int>();
        foreach (var user in _users)
            userTaskCounts[user.Id] = 0;

        foreach (var task in _tasks)
        {
            var eligibleUsers = _users
                .Where(u => userTaskCounts[u.Id] == 0)
                .ToList();

            if (eligibleUsers.Count == 0)
                continue;

            var chosenUser = eligibleUsers.OrderBy(_ => Guid.NewGuid()).First();
            task.AssignedUserId = chosenUser.Id;
            task.State = TaskState.InProgress;
            userTaskCounts[chosenUser.Id]++;

            db.TaskTransferHistories.Add(new TaskTransferHistory
            {
                TaskId = task.Id,
                UserId = chosenUser.Id,
                TransferredAt = DateTime.UtcNow
            });
            // Else leave task in Waiting state
        }

        db.Tasks.AddRange(_tasks);
    }

    #endregion
}