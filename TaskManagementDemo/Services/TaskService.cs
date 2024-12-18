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
        public async Task<IEnumerable<TaskEntity>> GetTasksAsync(string userId, TaskQueryParameters parameters)
        {
            var query = _context.Tasks.Where(t => t.UserId == userId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(parameters.Status))
            {
                query = query.Where(t => t.Status.ToLower() == parameters.Status.ToLower());
            }

            if (parameters.DueDateFrom.HasValue)
            {
                query = query.Where(t => t.DueDate >= parameters.DueDateFrom.Value);
            }

            if (parameters.DueDateTo.HasValue)
            {
                query = query.Where(t => t.DueDate <= parameters.DueDateTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(t =>
                    t.Title.Contains(parameters.SearchTerm) ||
                    t.Description.Contains(parameters.SearchTerm));
            }

            // Apply sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "title" => parameters.SortDescending
                    ? query.OrderByDescending(t => t.Title)
                    : query.OrderBy(t => t.Title),
                "status" => parameters.SortDescending
                    ? query.OrderByDescending(t => t.Status)
                    : query.OrderBy(t => t.Status),
                "duedate" => parameters.SortDescending
                    ? query.OrderByDescending(t => t.DueDate)
                    : query.OrderBy(t => t.DueDate),
                _ => parameters.SortDescending
                    ? query.OrderByDescending(t => t.DueDate)
                    : query.OrderBy(t => t.DueDate)
            };

            return await query.ToListAsync();
        }
    }
}
