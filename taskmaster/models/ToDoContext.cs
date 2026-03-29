using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CloudSync.Models
{
    public class ToDoContext : IdentityDbContext<User>
    {
        public ToDoContext(DbContextOptions<ToDoContext> options) : base(options) { }

        public DbSet<ToDo> ToDos { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ToDo>()
                .HasOne(t => t.User).WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ToDo>()
                .HasOne(t => t.AssignedTo).WithMany()
                .HasForeignKey(t => t.AssignedToUserId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ToDo>()
                .HasOne(t => t.Category).WithMany()
                .HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ToDo>()
                .HasOne(t => t.Status).WithMany()
                .HasForeignKey(t => t.StatusId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ToDo>()
                .HasOne(t => t.Priority).WithMany()
                .HasForeignKey(t => t.PriorityId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ToDo>()
                .HasOne(t => t.Project).WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner).WithMany()
                .HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project).WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.User).WithMany()
                .HasForeignKey(pm => pm.UserId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User).WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);

            // Seed data
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = "work", Name = "Work" },
                new Category { CategoryId = "home", Name = "Home" },
                new Category { CategoryId = "personal", Name = "Personal" },
                new Category { CategoryId = "shopping", Name = "Shopping" }
            );

            modelBuilder.Entity<Status>().HasData(
                new Status { StatusId = "open", Name = "Open" },
                new Status { StatusId = "inprogress", Name = "In Progress" },
                new Status { StatusId = "closed", Name = "Closed" }
            );

            modelBuilder.Entity<Priority>().HasData(
                new Priority { PriorityId = "low", Name = "Low", BadgeColor = "success" },
                new Priority { PriorityId = "medium", Name = "Medium", BadgeColor = "warning" },
                new Priority { PriorityId = "high", Name = "High", BadgeColor = "danger" }
            );
        }
    }
}
