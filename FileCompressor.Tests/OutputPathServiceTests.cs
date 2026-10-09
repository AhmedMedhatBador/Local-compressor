
using FileCompressor.Core.Services;
using Xunit;

namespace FileCompressor.Tests;

public class OutputPathServiceTests : IDisposable
{
    private readonly string _directory =
        Directory.CreateTempSubdirectory("OutputPathTests_").FullName;

    [Fact]
    public void GetUniqueOutputPath_PreservesRequestedExtension()
    {
        var inputPath = Path.Combine(_directory, "photo.png");
        var service = new OutputPathService();

        var output = service.GetUniqueOutputPath(inputPath, ".png");

        Assert.Equal("photo_compressed.png", Path.GetFileName(output));
    }

    [Fact]
    public void GetUniqueOutputPath_DoesNotOverwriteExistingOutput()
    {
        var inputPath = Path.Combine(_directory, "photo.png");
        var existingOutput = Path.Combine(
            _directory, "photo_compressed.png");

        File.WriteAllText(existingOutput, "existing file");

        var service = new OutputPathService();
        var output = service.GetUniqueOutputPath(inputPath, ".png");

        Assert.Equal("photo_compressed_2.png", Path.GetFileName(output));
        Assert.Equal("existing file", File.ReadAllText(existingOutput));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
