
using FileCompressor.Core.Interfaces;
using FileCompressor.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace FileCompressor.Infrastructure.Compression;

public class ImageCompressor : IImageCompressor
{
    private readonly int _quality;

    public ImageCompressor(int quality)
    {
        if (quality < 1 || quality > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quality),
                "Quality must be between 1 and 100.");
        }

        _quality = quality;
    }

    public async Task<CompressionResult> CompressAsync(
        string inputPath,
        string outputPath)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException(
                "The input image could not be found.",
                inputPath);
        }

        var inputExtension = Path.GetExtension(inputPath);

        var outputExtension = Path.GetExtension(outputPath);

        if (!string.Equals(
                inputExtension,
                outputExtension,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The output extension must match the input format. " +
                "Use the conversion feature to change formats.",
                nameof(outputPath));
        }

        var originalSize = new FileInfo(inputPath).Length;

        using var image = await Image.LoadAsync(inputPath);

        switch (inputExtension.ToLowerInvariant())
        {
            case ".jpg":
            case ".jpeg":
                await image.SaveAsJpegAsync(
                    outputPath,
                    new JpegEncoder
                    {
                        Quality = _quality
                    });
                break;

            case ".png":
                await image.SaveAsPngAsync(
                    outputPath,
                    new PngEncoder());
                break;

            default:
                throw new NotSupportedException(
                    $"Compression for '{inputExtension}' " +
                    "is not supported yet. Currently supported: JPEG and PNG.");
        }

        var compressedSize = new FileInfo(outputPath).Length;

        return new CompressionResult
        {
            OriginalFilePath = inputPath,
            OutputFilePath = outputPath,
            OriginalSize = originalSize,
            CompressedSize = compressedSize
        };
    }
}
