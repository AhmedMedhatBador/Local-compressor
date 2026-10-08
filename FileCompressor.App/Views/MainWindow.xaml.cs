using FileCompressor.App.ViewModels;
using System.Windows;

namespace FileCompressor.App.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel();
        DataContext = _viewModel;
    }
}