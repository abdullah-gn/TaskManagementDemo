using TaskManagementDemo.Models;

namespace TaskManagementDemo.Services
{
    public interface ITaskService
    {
        Task<Models.TaskEntity> CreateTaskAsync(TaskDto taskDto, string userId);
        Task<IEnumerable<TaskEntity>> GetTasksAsync(string userId);
        Task<Models.TaskEntity> GetTaskByIdAsync(int id, string userId);
        Task<Models.TaskEntity> UpdateTaskAsync(int id, TaskDto taskDto, string userId);
        Task DeleteTaskAsync(int id, string userId);
    }
}
