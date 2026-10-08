using FileCompressor.Infrastructure.Compression;

namespace FileCompressor.Tests;

public class ImageCompressorTests
{
    [Fact]
    public void ImageCompressor_CanBeCreated()
    {
        var compressor = new ImageCompressor(75);

        Assert.NotNull(compressor);
    }
}