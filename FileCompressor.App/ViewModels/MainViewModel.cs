using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FileCompressor.App.Commands;
using FileCompressor.Infrastructure.Compression;
using FileCompressor.Core.Services;
using FileCompressor.Core.Models;

namespace FileCompressor.App.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private string _selectedFileName = "No image selected";
    private string? _selectedFilePath;
    private string _compressionResult = string.Empty;
    public RelayCommand SelectFileCommand { get; }
    public AsyncRelayCommand CompressCommand { get; }
    private readonly OutputPathService _outputPathService = new();

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
            

        var outputExtension = Path.GetExtension(SelectedFilePath);
        var outputPath = _outputPathService.GetUniqueOutputPath(
            SelectedFilePath,
            outputExtension);

        try
        {
            var quality = SelectedCompressionMode switch
            {
                "Maximum Quality" => 95,
                "Balanced" => 75,
                "Maximum Compression" => 50,
                _ => 75
            };

            var pngCompressionMode = SelectedCompressionMode switch
{
    "Maximum Quality" => PngCompressionMode.Fast,
    "Balanced" => PngCompressionMode.Balanced,
    "Maximum Compression" => PngCompressionMode.Maximum,
    _ => PngCompressionMode.Balanced
};

var compressor = new ImageCompressor(quality, pngCompressionMode);

            var inputPath = SelectedFilePath;

var result = await Task.Run(async () =>
    await compressor.CompressAsync(inputPath, outputPath)
);


            var sizeChange = result.CompressionPercentage;

var resultLabel = result.SavedSpace
    ? $"Saved: {sizeChange:F1}%"
    : result.CompressedSize == result.OriginalSize
        ? "File size unchanged (0.0% saved)"
        : $"File size increased by {Math.Abs(sizeChange):F1}%";

CompressionResult =
    $"Original: {result.OriginalSize / 1024.0:F2} KB\n" +
    $"Compressed: {result.CompressedSize / 1024.0:F2} KB\n" +
    resultLabel;
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