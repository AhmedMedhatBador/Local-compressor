using FileCompressor.Core.Interfaces;
using FileCompressor.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;


namespace FileCompressor.Infrastructure.Compression;

public class ImageCompressor : IImageCompressor
{
    private readonly int _quality;

    private readonly PngCompressionMode _pngCompressionMode;

    
public ImageCompressor(
    int quality,
    PngCompressionMode pngCompressionMode = PngCompressionMode.Balanced)
{
    if (quality < 1 || quality > 100)
    {
        throw new ArgumentOutOfRangeException(
            nameof(quality),
            "Quality must be between 1 and 100.");
    }

    if (!Enum.IsDefined(pngCompressionMode))
    {
        throw new ArgumentOutOfRangeException(
            nameof(pngCompressionMode),
            "Invalid PNG compression mode.");
    }

    _quality = quality;
    _pngCompressionMode = pngCompressionMode;
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

        System.Diagnostics.Debug.WriteLine(
    $"Image dimensions: {image.Width} x {image.Height}");


        switch (inputExtension.ToLowerInvariant())
        {
            case ".jpg":
            case ".jpeg":
                await image.SaveAsJpegAsync(
                    outputPath,
                    new JpegEncoder
                    {
                        Quality = _quality,
                        Progressive = true
                    });
                break;
           

            
case ".png":
    var pngCompressionLevel = _pngCompressionMode switch
    {
        PngCompressionMode.Fast => PngCompressionLevel.BestSpeed,
        PngCompressionMode.Balanced => PngCompressionLevel.DefaultCompression,
        PngCompressionMode.Maximum => PngCompressionLevel.BestCompression,
        _ => PngCompressionLevel.DefaultCompression
    };

    await image.SaveAsPngAsync(
        outputPath,
        new PngEncoder
        {
            CompressionLevel = pngCompressionLevel
        });
    break;


            case ".webp":
                await image.SaveAsWebpAsync(
                    outputPath,
                    new WebpEncoder
                    {
                        Quality = _quality,
                        FileFormat = WebpFileFormatType.Lossy
                    });
                break;

            case ".gif":
                await image.SaveAsGifAsync(
                    outputPath,
                    new GifEncoder
                    {
                        ColorTableMode = FrameColorTableMode.Global
                    });
                break;
       

       

           

            default:
                throw new NotSupportedException(
                    $"Compression for '{inputExtension}' " +
                    "is not supported yet. Currently supported: JPEG, PNG, WebP and GIF.");
               
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
