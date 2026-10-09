using FileCompressor.Infrastructure.Compression;
using Xunit;

namespace FileCompressor.Tests;

public class VideoConverterTests
{
    private readonly FfmpegVideoConverter _converter = new();

    

    
[Fact]
public async Task ConvertVideoToGifAsync_Throws_WhenInputFormatIsUnsupported()
{
    var inputPath = Path.Combine(
        Path.GetTempPath(),
        $"{Guid.NewGuid()}.png");

    await File.WriteAllTextAsync(inputPath, "test");

    try
    {
        await Assert.ThrowsAsync<NotSupportedException>(
            () => _converter.ConvertVideoToGifAsync(
                inputPath,
                "output.gif"));
    }
    finally
    {
        File.Delete(inputPath);
    }
}


    [Fact]
    public async Task ConvertVideoToGifAsync_Throws_WhenInputFileDoesNotExist()
    {
        var inputPath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.mp4");

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _converter.ConvertVideoToGifAsync(inputPath, "output.gif"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ConvertVideoToGifAsync_Throws_WhenWidthIsInvalid(int width)
    {
        var inputPath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.mp4");

        await File.WriteAllTextAsync(inputPath, "test");

        try
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _converter.ConvertVideoToGifAsync(
                    inputPath,
                    "output.gif",
                    width));
        }
        finally
        {
            File.Delete(inputPath);
        }
    }



[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(61)]
public async Task ConvertVideoToGifAsync_Throws_WhenFpsIsInvalid(
    int fps)
{
    var inputPath = Path.Combine(
        Path.GetTempPath(), $"{Guid.NewGuid()}.mp4");

    await File.WriteAllTextAsync(inputPath, "test");

    try
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _converter.ConvertVideoToGifAsync(
                inputPath, "output.gif", 320, fps));
    }
    finally
    {
        File.Delete(inputPath);
    }
}

[Fact]
public async Task ConvertVideoToGifAsync_Throws_WhenOutputIsNotGif()
{
    var inputPath = Path.Combine(
        Path.GetTempPath(), $"{Guid.NewGuid()}.mp4");

    await File.WriteAllTextAsync(inputPath, "test");

    try
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _converter.ConvertVideoToGifAsync(
                inputPath, "output.png"));
    }
    finally
    {
        File.Delete(inputPath);
    }
}

}
