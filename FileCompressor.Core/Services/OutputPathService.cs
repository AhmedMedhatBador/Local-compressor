using System.IO;

namespace FileCompressor.Core.Services;

public class OutputPathService
{
    public string GetUniqueOutputPath(
        string inputPath,
        string outputExtension = ".jpg")
    {
        var directory = Path.GetDirectoryName(inputPath)
            ?? throw new ArgumentException(
                "The input path must include a directory.",
                nameof(inputPath));

        var fileName = Path.GetFileNameWithoutExtension(inputPath);
        var outputPath = Path.Combine(
            directory,
            $"{fileName}_compressed{outputExtension}");

        var counter = 2;

        while (File.Exists(outputPath))
        {
            outputPath = Path.Combine(
                directory,
                $"{fileName}_compressed_{counter}{outputExtension}");

            counter++;
        }

        return outputPath;
    }
}