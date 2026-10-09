
using FileCompressor.Core.Models;
using FileCompressor.Infrastructure.Compression;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace FileCompressor.Tests;

public class ImageCompressorTests : IDisposable
{
    private readonly string _tempDirectory =
        Directory.CreateTempSubdirectory("FileCompressorTests_").FullName;

    [Theory]
    [InlineData(".jpg")]
    [InlineData(".png")]
    [InlineData(".webp")]
    [InlineData(".gif")]
    public async Task CompressAsync_CreatesValidOutput_ForSupportedFormats(
        string extension)
    {
        var inputPath = Path.Combine(_tempDirectory, $"input{extension}");
        var outputPath = Path.Combine(_tempDirectory, $"output{extension}");

        await CreateImageAsync(inputPath);

        var compressor = new ImageCompressor(
            CompressionPreset.SmallerFileSimilarLook);

        var result = await compressor.CompressAsync(inputPath, outputPath);

        Assert.True(File.Exists(outputPath));
        Assert.True(result.OriginalSize > 0);
        Assert.True(result.CompressedSize > 0);

        using var output = await Image.LoadAsync<Rgba32>(outputPath);

        Assert.Equal(16, output.Width);
        Assert.Equal(16, output.Height);
    }

    [Theory]
    [InlineData(CompressionPreset.VisuallyLossless)]
    [InlineData(CompressionPreset.SmallerFileSimilarLook)]
    [InlineData(CompressionPreset.SmallestFile)]
    public async Task CompressAsync_WorksWithEachPreset(
        CompressionPreset preset)
    {
        var inputPath = Path.Combine(_tempDirectory, "preset.webp");
        var outputPath = Path.Combine(
            _tempDirectory, $"{preset}.webp");

        await CreateImageAsync(inputPath);

        var compressor = new ImageCompressor(preset);

        await compressor.CompressAsync(inputPath, outputPath);

        Assert.True(File.Exists(outputPath));

        using var output = await Image.LoadAsync<Rgba32>(outputPath);
        Assert.Equal(16, output.Width);
        Assert.Equal(16, output.Height);
    }

    [Fact]
    public async Task CompressAsync_PreservesPngTransparency()
    {
        var inputPath = Path.Combine(_tempDirectory, "transparent.png");
        var outputPath = Path.Combine(_tempDirectory, "transparent_out.png");

        using (var image = new Image<Rgba32>(16, 16))
        {
            image[0, 0] = new Rgba32(255, 0, 0, 0);
            image[1, 1] = new Rgba32(0, 255, 0, 255);
            await image.SaveAsPngAsync(inputPath);
        }

        var compressor = new ImageCompressor(
            CompressionPreset.SmallerFileSimilarLook);

        await compressor.CompressAsync(inputPath, outputPath);

        using var output = await Image.LoadAsync<Rgba32>(outputPath);

        Assert.Equal((byte)0, output[0, 0].A);
        Assert.Equal((byte)255, output[1, 1].A);
    }

    [Fact]
    public async Task CompressAsync_Throws_WhenInputDoesNotExist()
    {
        var inputPath = Path.Combine(_tempDirectory, "missing.png");
        var outputPath = Path.Combine(_tempDirectory, "output.png");

        var compressor = new ImageCompressor(75);

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => compressor.CompressAsync(inputPath, outputPath));
    }

    [Fact]
    public async Task CompressAsync_Throws_WhenExtensionsDoNotMatch()
    {
        var inputPath = Path.Combine(_tempDirectory, "input.png");
        var outputPath = Path.Combine(_tempDirectory, "output.jpg");

        await CreateImageAsync(inputPath);

        var compressor = new ImageCompressor(75);

        await Assert.ThrowsAsync<ArgumentException>(
            () => compressor.CompressAsync(inputPath, outputPath));

        Assert.False(File.Exists(outputPath));
    }

    [Fact]
    public void Constructor_Throws_WhenQualityIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ImageCompressor(0));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ImageCompressor(101));
    }

    private static async Task CreateImageAsync(string path)
    {
        using var image = new Image<Rgba32>(16, 16);

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                image[x, y] = new Rgba32(
                    (byte)(x * 16),
                    (byte)(y * 16),
                    120,
                    255);
            }
        }

        var extension = Path.GetExtension(path).ToLowerInvariant();

        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                await image.SaveAsJpegAsync(path);
                break;
            case ".png":
                await image.SaveAsPngAsync(path);
                break;
            case ".webp":
                await image.SaveAsWebpAsync(path);
                break;
            case ".gif":
                await image.SaveAsGifAsync(path);
                break;
            default:
                throw new NotSupportedException(extension);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
