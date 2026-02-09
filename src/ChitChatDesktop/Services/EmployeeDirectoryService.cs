using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChitChat.Dtos;

namespace ChitChat.Services;

public sealed class EmployeeDirectoryService : IEmployeeDirectoryService
{
    private readonly ApiClient _apiClient;

    public EmployeeDirectoryService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IReadOnlyList<Department>> GetDepartmentsAsync(CancellationToken cancellationToken = default)
        => await _apiClient.GetAsync<List<Department>>("api/departments", cancellationToken)
           ?? new List<Department>();

    public async Task<IReadOnlyList<Employee>> GetEmployeesAsync(CancellationToken cancellationToken = default)
        => await _apiClient.GetAsync<List<Employee>>("api/employees", cancellationToken)
           ?? new List<Employee>();
}
