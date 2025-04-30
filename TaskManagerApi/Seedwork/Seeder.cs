using TaskManagerApi.Data;
using TaskManagerApi.Models;

namespace TaskManagerApi.Seedwork;

public class Seeder(AppDbContext db)
{
    public void Seed()
    {
        if (!db.Users.Any())
        {
            db.Users.AddRange(
                new User { Name = "George" },
                new User { Name = "Mary" },
                new User { Name = "Alex" },
                new User { Name = "Anna" },
                new User { Name = "Bob" });
        }

        if (!db.Tasks.Any())
        {
            db.Tasks.AddRange(
                new TaskItem { Title = "Write tests" },
                new TaskItem { Title = "Document endpoints" },
                new TaskItem { Title = "Update projects" },
                new TaskItem { Title = "Delete unused files" },
                new TaskItem { Title = "Clean up models" });
        }

        db.SaveChanges();
    }
}