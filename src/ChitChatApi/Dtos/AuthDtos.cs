namespace ChitChatApi.Dtos;

public sealed record LoginRequest(string Username, string Password);

public sealed record EmployeeDto(int Id, string Name, int? DepartmentId, string Username);
