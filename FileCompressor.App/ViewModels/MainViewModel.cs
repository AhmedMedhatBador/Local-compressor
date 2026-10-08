using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FileCompressor.App.Commands;
using FileCompressor.Infrastructure.Compression;

namespace FileCompressor.App.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private string _selectedFileName = "No image selected";
    private string? _selectedFilePath;
    private string _compressionResult = string.Empty;

    public RelayCommand SelectFileCommand { get; }
    public AsyncRelayCommand CompressCommand { get; }

    public MainViewModel()
    {
        SelectFileCommand = new RelayCommand(SelectFile);
        CompressCommand = new AsyncRelayCommand(CompressAsync);
    }

    private void SelectFile()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp;*.gif"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedFilePath = dialog.FileName;
            SelectedFileName = Path.GetFileName(dialog.FileName);
            CompressionResult = string.Empty;
        }
    }


    private string _selectedCompressionMode = "Balanced";

    public string SelectedCompressionMode
    {
        get => _selectedCompressionMode;
        set
        {
            if (_selectedCompressionMode == value)
                return;

            _selectedCompressionMode = value;
            OnPropertyChanged();
        }
    }

    public string? SelectedFilePath
    {
        get => _selectedFilePath;
        set
        {
            if (_selectedFilePath == value) return;
            _selectedFilePath = value;
            OnPropertyChanged();
        }
    }

    public string SelectedFileName
    {
        get => _selectedFileName;
        set
        {
            if (_selectedFileName == value) return;
            _selectedFileName = value;
            OnPropertyChanged();
        }
    }

    public string CompressionResult
    {
        get => _compressionResult;
        set
        {
            if (_compressionResult == value) return;
            _compressionResult = value;
            OnPropertyChanged();
        }
    }

    private async Task CompressAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath))
            return;

        var outputPath = Path.Combine(
            Path.GetDirectoryName(SelectedFilePath)!,
            Path.GetFileNameWithoutExtension(SelectedFilePath) + "_compressed.jpg"
        );

        try
        {
            var compressor = new ImageCompressor(75);

            var result = await compressor.CompressAsync(
                SelectedFilePath,
                outputPath
            );

            CompressionResult =
                $"Original: {result.OriginalSize / 1024.0:F2} KB\n" +
                $"Compressed: {result.CompressedSize / 1024.0:F2} KB\n" +
                $"Saved: {result.CompressionPercentage:F1}%";
        }
        catch (Exception ex)
        {
            CompressionResult = $"Compression failed: {ex.Message}";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName)
        );
    }
}