using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TaskManagementDemo.Models;
using TaskManagementDemo.Services;

namespace TaskManagementDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost]
        public async Task<ActionResult<TaskEntity>> CreateTask(TaskDto taskDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var task = await _taskService.CreateTaskAsync(taskDto, userId);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskEntity>>> GetTasks()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tasks = await _taskService.GetTasksAsync(userId);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskEntity>> GetTask(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var task = await _taskService.GetTaskByIdAsync(id, userId);
                return Ok(task);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TaskEntity>> UpdateTask(int id, TaskDto taskDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var task = await _taskService.UpdateTaskAsync(id, taskDto, userId);
                return Ok(task);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _taskService.DeleteTaskAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
