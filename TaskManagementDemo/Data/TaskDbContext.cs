using Microsoft.EntityFrameworkCore;
using TaskManagementDemo.Models;

namespace TaskManagementDemo.Data
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options)
         : base(options)
        {
        }

        public DbSet<TaskEntity> Tasks { get; set; }
    }
}
