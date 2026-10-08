using FileCompressor.Core.Models;

namespace FileCompressor.Core.Interfaces;

public interface IImageCompressor
{
    Task<CompressionResult> CompressAsync(
        string inputPath,
        string outputPath);
}