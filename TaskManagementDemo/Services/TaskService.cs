using Microsoft.EntityFrameworkCore;
using TaskManagementDemo.Data;
using TaskManagementDemo.Models;

namespace TaskManagementDemo.Services
{
    public class TaskService : ITaskService
    {
        private readonly TaskDbContext _context;

        public TaskService(TaskDbContext context)
        {
            _context = context;
        }

        public async Task<TaskEntity> CreateTaskAsync(TaskDto taskDto, string userId)
        {
            var task = new TaskEntity
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                UserId = userId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<IEnumerable<TaskEntity>> GetTasksAsync(string userId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task<TaskEntity> GetTaskByIdAsync(int id, string userId)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null || task.UserId != userId)
                throw new KeyNotFoundException("Task not found");
            return task;
        }

        public async Task<TaskEntity> UpdateTaskAsync(int id, TaskDto taskDto, string userId)
        {
            var task = await GetTaskByIdAsync(id, userId);

            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.DueDate = taskDto.DueDate;

            await _context.SaveChangesAsync();
            return task;
        }

        public async Task DeleteTaskAsync(int id, string userId)
        {
            var task = await GetTaskByIdAsync(id, userId);
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}
