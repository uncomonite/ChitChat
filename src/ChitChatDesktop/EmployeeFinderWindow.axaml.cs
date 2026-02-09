using System;
using Avalonia.Controls;
using Avalonia.Input;
using ChitChat.ViewModels;

namespace ChitChat.Views;

public partial class EmployeeFinderWindow : Window
{
    public EmployeeFinderWindow()
    {
        InitializeComponent();
        EmployeesList.DoubleTapped += OnEmployeeDoubleTapped;
    }

    public EmployeeFinderWindow(EmployeeFinderViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private async void OnEmployeeDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is EmployeeFinderViewModel viewModel)
        {
            await viewModel.RequestSelectEmployeeAsync(viewModel.SelectedEmployee);
            Close();
        }
    }
}
