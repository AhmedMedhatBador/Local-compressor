
using FileCompressor.Core.Interfaces;
using FFMpegCore;
using FFMpegCore.Pipes;

namespace FileCompressor.Infrastructure.Compression;

public class FfmpegVideoConverter : IVideoConverter
{



static FfmpegVideoConverter()
{
    var ffmpegFolder = Environment.GetEnvironmentVariable(
        "FILECOMPRESSOR_FFMPEG_PATH");

    if (string.IsNullOrWhiteSpace(ffmpegFolder))
    {
        ffmpegFolder = @"C:\msys64\ucrt64\bin";
    }

    GlobalFFOptions.Configure(options =>
    {
        options.BinaryFolder = ffmpegFolder;
    });
}

    public async Task ConvertVideoToGifAsync(
        string inputPath,
        string outputPath,
        int width = 480,
        int framesPerSecond = 12,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException(
                "The input video could not be found.",
                inputPath);
        }


var supportedExtensions = new[]
{
    ".mp4",
    ".webm",
    ".mov",
    ".avi",
    ".mkv"
};

var inputExtension = Path.GetExtension(inputPath);

if (!supportedExtensions.Contains(
        inputExtension,
        StringComparer.OrdinalIgnoreCase))
{
    throw new NotSupportedException(
        $"The video format '{inputExtension}' is not supported.");
}


        if (width < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                "Width must be greater than zero.");
        }

        if (framesPerSecond < 1 || framesPerSecond > 60)
        {
            throw new ArgumentOutOfRangeException(
                nameof(framesPerSecond),
                "Frames per second must be between 1 and 60.");
        }

        if (!string.Equals(
                Path.GetExtension(outputPath),
                ".gif",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The output file must have a .gif extension.",
                nameof(outputPath));
        }

        var outputDirectory = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var arguments = FFMpegArguments
            .FromFileInput(inputPath)
            .OutputToFile(
                outputPath,
                overwrite: true,
                options => options
                    .WithCustomArgument(
                        $"-vf \"fps={framesPerSecond},scale={width}:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse\"")
                    .WithCustomArgument("-loop 0"));

        
try
{
    await arguments
        .CancellableThrough(cancellationToken)
        .ProcessAsynchronously();
}
catch
{
    if (File.Exists(outputPath))
    {
        File.Delete(outputPath);
    }

    throw;
}

    }
}
