using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChitChat.Dtos;

namespace ChitChat.Services;

public interface IEmployeeDirectoryService
{
    Task<IReadOnlyList<Department>> GetDepartmentsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Employee>> GetEmployeesAsync(CancellationToken cancellationToken = default);
}
