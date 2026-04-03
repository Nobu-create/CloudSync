using CloudSync.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CloudSync.Tests
{
    public class ToDoModelTests
    {
        // ===== ToDo Model Tests =====

        [Fact]
        public void ToDo_IsOverdue_WhenStatusOpenAndDueDatePast()
        {
            var task = new ToDo
            {
                StatusId = "open",
                DueDate = DateTime.Today.AddDays(-1)
            };
            Assert.True(task.Overdue);
        }

        [Fact]
        public void ToDo_IsNotOverdue_WhenStatusClosed()
        {
            var task = new ToDo
            {
                StatusId = "closed",
                DueDate = DateTime.Today.AddDays(-1)
            };
            Assert.False(task.Overdue);
        }

        [Fact]
        public void ToDo_IsNotOverdue_WhenDueDateIsFuture()
        {
            var task = new ToDo
            {
                StatusId = "open",
                DueDate = DateTime.Today.AddDays(5)
            };
            Assert.False(task.Overdue);
        }

        [Fact]
        public void ToDo_DefaultPriority_IsMedium()
        {
            var task = new ToDo();
            Assert.Equal("medium", task.PriorityId);
        }

        // ===== User Model Tests =====

        [Fact]
        public void User_FullName_CombinesFirstAndLastName()
        {
            var user = new User { FirstName = "Nobukhosi", LastName = "Dube" };
            Assert.Equal("Nobukhosi Dube", user.FullName);
        }

        [Fact]
        public void User_FullName_HandlesEmptyLastName()
        {
            var user = new User { FirstName = "Nobukhosi", LastName = "" };
            Assert.Equal("Nobukhosi ", user.FullName);
        }

        // ===== Project Model Tests =====

        [Fact]
        public void Project_ProgressPercent_IsZero_WhenNoTasks()
        {
            var project = new Project();
            Assert.Equal(0, project.ProgressPercent);
        }

        [Fact]
        public void Project_ProgressPercent_IsHundred_WhenAllTasksDone()
        {
            var project = new Project
            {
                Tasks = new List<ToDo>
                {
                    new ToDo { StatusId = "closed" },
                    new ToDo { StatusId = "closed" }
                }
            };
            Assert.Equal(100, project.ProgressPercent);
        }

        [Fact]
        public void Project_ProgressPercent_IsCorrect_WhenSomeTasksDone()
        {
            var project = new Project
            {
                Tasks = new List<ToDo>
                {
                    new ToDo { StatusId = "closed" },
                    new ToDo { StatusId = "closed" },
                    new ToDo { StatusId = "open" },
                    new ToDo { StatusId = "open" }
                }
            };
            Assert.Equal(50, project.ProgressPercent);
        }

        [Fact]
        public void Project_OverdueTasks_CountsCorrectly()
        {
            var project = new Project
            {
                Tasks = new List<ToDo>
                {
                    new ToDo { StatusId = "open", DueDate = DateTime.Today.AddDays(-1) },
                    new ToDo { StatusId = "open", DueDate = DateTime.Today.AddDays(5) },
                    new ToDo { StatusId = "closed", DueDate = DateTime.Today.AddDays(-1) }
                }
            };
            Assert.Equal(1, project.OverdueTasks);
        }

        // ===== Database Context Tests =====

        private ToDoContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ToDoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ToDoContext(options);
        }

        [Fact]
        public async Task Context_CanAddAndRetrieveTask()
        {
            using var context = GetInMemoryContext();
            var task = new ToDo
            {
                Description = "Test task",
                CategoryId = "work",
                StatusId = "open",
                PriorityId = "medium",
                UserId = "user1",
                DueDate = DateTime.Today.AddDays(1)
            };
            context.ToDos.Add(task);
            await context.SaveChangesAsync();

            var retrieved = await context.ToDos.FindAsync(task.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Test task", retrieved!.Description);
        }

        [Fact]
        public async Task Context_CanAddAndRetrieveProject()
        {
            using var context = GetInMemoryContext();
            var project = new Project
            {
                Name = "Test Project",
                Description = "A test project",
                UserId = "user1",
                StatusId = "active"
            };
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var retrieved = await context.Projects.FindAsync(project.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Test Project", retrieved!.Name);
        }

        [Fact]
        public async Task Context_CanAddNotification()
        {
            using var context = GetInMemoryContext();
            var notification = new Notification
            {
                UserId = "user1",
                Message = "You have a new task",
                Type = "info"
            };
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            var count = context.Notifications.Count();
            Assert.Equal(1, count);
        }

        // ===== Filters Tests =====

        [Fact]
        public void Filters_ParsesFilterString_Correctly()
        {
            var filters = new Filters("work-future-open");
            Assert.Equal("work", filters.CategoryId);
            Assert.Equal("future", filters.Due);
            Assert.Equal("open", filters.StatusId);
        }

        [Fact]
        public void Filters_HasCategory_IsFalse_WhenAll()
        {
            var filters = new Filters("all-all-all");
            Assert.False(filters.HasCategory);
        }

        [Fact]
        public void Filters_HasCategory_IsTrue_WhenSet()
        {
            var filters = new Filters("work-all-all");
            Assert.True(filters.HasCategory);
        }
    }
}
