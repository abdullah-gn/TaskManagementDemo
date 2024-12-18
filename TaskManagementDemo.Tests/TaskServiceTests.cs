using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManagementDemo.Data;
using TaskManagementDemo.Models;
using TaskManagementDemo.Services;

namespace TaskManagementDemo.Tests
{
    public class TaskServiceTests
    {
        private readonly TaskDbContext _context;
        private readonly TaskService _taskService;
        private readonly string _testUserId = "test-user-id";

        public TaskServiceTests()
        {
            var options = new DbContextOptionsBuilder<TaskDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TaskDbContext(options);
            _taskService = new TaskService(_context);
        }

        [Fact]
        public async Task CreateTask_ShouldCreateNewTask()
        {
            // Arrange
            var taskDto = new TaskDto
            {
                Title = "Test Task",
                Description = "Test Description",
                DueDate = DateTime.Now.AddDays(1)
            };

            // Act
            var result = await _taskService.CreateTaskAsync(taskDto, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskDto.Title, result.Title);
            Assert.Equal(taskDto.Description, result.Description);
            Assert.Equal(_testUserId, result.UserId);
            Assert.Equal("Pending", result.Status);
        }

        [Fact]
        public async Task GetTasks_ShouldReturnUserTasks()
        {
            // Arrange
            await SeedTasks();
            var queryParams = new TaskQueryParameters();

            // Act
            var tasks = await _taskService.GetTasksAsync(_testUserId, queryParams);

            // Assert
            Assert.NotNull(tasks);
            Assert.All(tasks, task => Assert.Equal(_testUserId, task.UserId));
        }

        [Fact]
        public async Task GetTasks_WithFiltering_ShouldReturnFilteredTasks()
        {
            // Arrange
            await SeedTasks();
            var queryParams = new TaskQueryParameters
            {
                Status = "Pending",
                SearchTerm = "Test"
            };

            // Act
            var tasks = await _taskService.GetTasksAsync(_testUserId, queryParams);

            // Assert
            Assert.All(tasks, task =>
            {
                Assert.Equal("Pending", task.Status);
                Assert.Contains("Test", task.Title);
            });
        }

        [Fact]
        public async Task UpdateTask_ShouldUpdateExistingTask()
        {
            // Arrange
            var task = await CreateTestTask();
            var updateDto = new TaskDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                DueDate = DateTime.Now.AddDays(2)
            };

            // Act
            var result = await _taskService.UpdateTaskAsync(task.Id, updateDto, _testUserId);

            // Assert
            Assert.Equal(updateDto.Title, result.Title);
            Assert.Equal(updateDto.Description, result.Description);
        }

        [Fact]
        public async Task DeleteTask_ShouldRemoveTask()
        {
            // Arrange
            var task = await CreateTestTask();

            // Act
            await _taskService.DeleteTaskAsync(task.Id, _testUserId);

            // Assert
            var deletedTask = await _context.Tasks.FindAsync(task.Id);
            Assert.Null(deletedTask);
        }

        [Fact]
        public async Task GetTaskById_WithWrongUser_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var task = await CreateTestTask();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _taskService.GetTaskByIdAsync(task.Id, "wrong-user-id"));
        }

        private async Task<TaskEntity> CreateTestTask()
        {
            var task = new TaskEntity
            {
                Title = "Test Task",
                Description = "Test Description",
                DueDate = DateTime.Now.AddDays(1),
                UserId = _testUserId,
                Status = "Pending"
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        private async Task SeedTasks()
        {
            var tasks = new[]
            {
                new TaskEntity
                {
                    Title = "Test Task 1",
                    Description = "Description 1",
                    DueDate = DateTime.Now.AddDays(1),
                    UserId = _testUserId,
                    Status = "Pending"
                },
                new TaskEntity
                {
                    Title = "Test Task 2",
                    Description = "Description 2",
                    DueDate = DateTime.Now.AddDays(2),
                    UserId = _testUserId,
                    Status = "Completed"
                }
            };

            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();
        }
    }

    public class AuthServiceTests
    {
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Create mock configuration
            var configValues = new Dictionary<string, string>
            {
                {"Jwt:Key", "your-test-secret-key-with-minimum-sixteen-characters"},
                {"Jwt:Issuer", "test-issuer"},
                {"Jwt:Audience", "test-audience"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            _authService = new AuthService(_configuration);
        }

        [Fact]
        public void GenerateJwtToken_ShouldReturnValidToken()
        {
            // Arrange
            var userId = "test-user-id";
            var email = "test@example.com";

            // Act
            var token = _authService.GenerateJwtToken(userId, email);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);

            // Validate token can be decoded
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            Assert.Equal(userId, jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal(email, jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        }

        [Fact]
        public void GenerateJwtToken_ShouldIncludeCorrectClaims()
        {
            // Arrange
            var userId = "test-user-id";
            var email = "test@example.com";

            // Act
            var token = _authService.GenerateJwtToken(userId, email);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId);
            Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Email && c.Value == email);
        }
    }
}
