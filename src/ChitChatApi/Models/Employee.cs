namespace ChitChatApi.Models;

public sealed class Employee
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int? DepartmentId { get; init; }
    public string Username { get; init; } = string.Empty;
}
