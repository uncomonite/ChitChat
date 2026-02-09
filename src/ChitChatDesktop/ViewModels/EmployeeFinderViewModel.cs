using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ChitChat.Services;

namespace ChitChat.ViewModels;

public partial class EmployeeFinderViewModel : ViewModelBase
{
    private readonly IEmployeeDirectoryService? _directoryService;

    public EmployeeFinderViewModel()
    {
        Departments = new ObservableCollection<DepartmentFilter>
        {
            new(1, "Engineering", true),
            new(2, "HR", true),
            new(3, "Sales", true),
            new(4, "Support", true)
        };

        Employees = new ObservableCollection<EmployeeListItem>
        {
            new(1, "Martin Baumann", 1, "Engineering"),
            new(2, "Artan Shala", 2, "HR"),
            new(3, "Marry Hoegart", 3, "Sales"),
            new(4, "Olivia Stone", 1, "Engineering"),
            new(5, "Robert Miles", 4, "Support")
        };

        foreach (var department in Departments)
        {
            department.PropertyChanged += OnDepartmentChanged;
        }

        RefreshFilteredEmployees();
    }

    public EmployeeFinderViewModel(IEmployeeDirectoryService directoryService)
    {
        _directoryService = directoryService;
        Departments = new ObservableCollection<DepartmentFilter>();
        Employees = new ObservableCollection<EmployeeListItem>();
    }

    public ObservableCollection<DepartmentFilter> Departments { get; }

    public ObservableCollection<EmployeeListItem> Employees { get; }

    public ObservableCollection<EmployeeListItem> FilteredEmployees { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private EmployeeListItem? _selectedEmployee;

    public event Func<EmployeeListItem, Task>? EmployeeSelected;

    partial void OnSearchTextChanged(string value)
    {
        RefreshFilteredEmployees();
    }

    public async Task RequestSelectEmployeeAsync(EmployeeListItem? employee)
    {
        if (employee is null)
        {
            return;
        }

        var handlers = EmployeeSelected;
        if (handlers is null)
        {
            return;
        }

        foreach (Func<EmployeeListItem, Task> handler in handlers.GetInvocationList())
        {
            try
            {
                await handler(employee);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to handle employee selection: {ex}");
            }
        }
    }

    public async Task LoadAsync()
    {
        if (_directoryService is null)
        {
            return;
        }

        var departments = await _directoryService.GetDepartmentsAsync();
        var employees = await _directoryService.GetEmployeesAsync();

        Departments.Clear();
        foreach (var department in departments)
        {
            var filter = new DepartmentFilter(department.Id, department.Name, true);
            filter.PropertyChanged += OnDepartmentChanged;
            Departments.Add(filter);
        }

        var hasUnassigned = employees.Any(employee => employee.DepartmentId is null);
        if (hasUnassigned)
        {
            var unassignedFilter = new DepartmentFilter(0, "Unassigned", true);
            unassignedFilter.PropertyChanged += OnDepartmentChanged;
            Departments.Add(unassignedFilter);
        }

        Employees.Clear();
        var departmentLookup = departments.ToDictionary(d => d.Id, d => d.Name);
        foreach (var employee in employees)
        {
            var departmentName = employee.DepartmentId is { } id && departmentLookup.TryGetValue(id, out var name)
                ? name
                : "Unassigned";
            Employees.Add(new EmployeeListItem(employee.Id, employee.Name, employee.DepartmentId ?? 0, departmentName));
        }

        RefreshFilteredEmployees();
    }

    private void OnDepartmentChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DepartmentFilter.IsSelected))
        {
            RefreshFilteredEmployees();
        }
    }

    private void RefreshFilteredEmployees()
    {
        var search = SearchText.Trim();
        var activeDepartments = Departments
            .Where(d => d.IsSelected)
            .Select(d => d.Id)
            .ToHashSet();

        var matches = Employees.Where(employee =>
            activeDepartments.Contains(employee.DepartmentId)
            && (string.IsNullOrWhiteSpace(search)
                || employee.Name.Contains(search, StringComparison.OrdinalIgnoreCase)));

        FilteredEmployees.Clear();
        foreach (var employee in matches)
        {
            FilteredEmployees.Add(employee);
        }
    }
}

public sealed partial class DepartmentFilter : ObservableObject
{
    public DepartmentFilter(int id, string name, bool isSelected)
    {
        Id = id;
        Name = name;
        _isSelected = isSelected;
    }

    public int Id { get; }
    public string Name { get; }

    [ObservableProperty]
    private bool _isSelected;
}

public sealed record EmployeeListItem(int Id, string Name, int DepartmentId, string DepartmentName);
