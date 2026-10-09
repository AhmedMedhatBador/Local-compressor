namespace FileCompressor.Core.Models;

public class CompressionResult
{
    public string OriginalFilePath { get; set; } = string.Empty;
    public string OutputFilePath { get; set; } = string.Empty;
    public long OriginalSize { get; set; }
    public long CompressedSize { get; set; }
    public bool SavedSpace => CompressedSize < OriginalSize;
    public double CompressionPercentage =>
        OriginalSize == 0
            ? 0
            : (1 - (double)CompressedSize / OriginalSize) * 100;
}