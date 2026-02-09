using System;
using ChitChat.Dtos;

namespace ChitChat.Services;

public sealed class CurrentUserStore
{
    public Employee? CurrentEmployee { get; private set; }

    public event Action<Employee?>? CurrentEmployeeChanged;

    public void Set(Employee? employee)
    {
        CurrentEmployee = employee;
        CurrentEmployeeChanged?.Invoke(employee);
    }
}
