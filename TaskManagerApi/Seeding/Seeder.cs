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
            new TaskItem { Title = "Write tests", State = TaskState.InProgress },
            new TaskItem { Title = "Document endpoints", State = TaskState.InProgress },
            new TaskItem { Title = "Update projects", State = TaskState.InProgress },
            new TaskItem { Title = "Delete unused files", State = TaskState.InProgress },
            new TaskItem { Title = "Clean up models", State = TaskState.InProgress }
        ];

        // Assign a random user to each task
        foreach (var task in _tasks)
        {
            var user = _users.OrderBy(_ => Guid.NewGuid()).First();
            task.AssignedUserId = user.Id;

            db.TaskTransferHistories.Add(new TaskTransferHistory
            {
                TaskId = task.Id,
                UserId = user.Id,
                TransferredAt = DateTime.UtcNow
            });
        }

        db.Tasks.AddRange(_tasks);
    }

    #endregion
}