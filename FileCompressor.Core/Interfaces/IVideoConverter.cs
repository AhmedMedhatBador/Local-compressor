
namespace FileCompressor.Core.Interfaces;

public interface IVideoConverter
{
    Task ConvertVideoToGifAsync(
        string inputPath,
        string outputPath,
        int width = 480,
        int framesPerSecond = 12,
        CancellationToken cancellationToken = default);
}
