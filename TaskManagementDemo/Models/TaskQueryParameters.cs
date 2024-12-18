namespace TaskManagementDemo.Models
{
    public class TaskQueryParameters
    {
        public string? Status { get; set; }
        public string? SortBy { get; set; } = "DueDate";
        public bool SortDescending { get; set; } = false;
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public string? SearchTerm { get; set; }
    }
}
