using FileCompressor.Core.Interfaces;
using FileCompressor.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace FileCompressor.Infrastructure.Compression;

public class ImageCompressor : IImageCompressor
{
    private readonly int _quality;

    public ImageCompressor(int quality)
    {
        _quality = quality;
    }
    public async Task<CompressionResult> CompressAsync(
        string inputPath,
        string outputPath)
    {
        var originalSize = new FileInfo(inputPath).Length;

        using var image = await Image.LoadAsync(inputPath);

        await image.SaveAsJpegAsync(
            outputPath,
            new JpegEncoder
            {
                Quality = _quality
            });

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