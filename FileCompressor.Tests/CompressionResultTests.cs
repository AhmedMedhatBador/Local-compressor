
using FileCompressor.Core.Models;
using Xunit;

namespace FileCompressor.Tests;

public class CompressionResultTests
{
    [Fact]
    public void SavedSpace_ReturnsTrue_WhenCompressedFileIsSmaller()
    {
        var result = new CompressionResult
        {
            OriginalSize = 1000,
            CompressedSize = 700
        };

        Assert.True(result.SavedSpace);
    }



[Fact]
public void CompressionPercentage_ReturnsZero_WhenFileSizesAreEqual()
{
    var result = new CompressionResult
    {
        OriginalSize = 1000,
        CompressedSize = 1000
    };

    Assert.Equal(0, result.CompressionPercentage);
}


    [Fact]
    public void SavedSpace_ReturnsFalse_WhenCompressedFileIsLarger()
    {
        var result = new CompressionResult
        {
            OriginalSize = 1000,
            CompressedSize = 1200
        };

        Assert.False(result.SavedSpace);
    }

    [Fact]
    public void SavedSpace_ReturnsFalse_WhenFileSizesAreEqual()
    {
        var result = new CompressionResult
        {
            OriginalSize = 1000,
            CompressedSize = 1000
        };

        Assert.False(result.SavedSpace);
    }

    [Fact]
    public void CompressionPercentage_ReturnsPositiveValue_WhenSpaceIsSaved()
    {
        var result = new CompressionResult
        {
            OriginalSize = 1000,
            CompressedSize = 700
        };

        Assert.Equal(30.0, result.CompressionPercentage, 5);
    }

    [Fact]
    public void CompressionPercentage_ReturnsNegativeValue_WhenFileGetsLarger()
    {
        var result = new CompressionResult
        {
            OriginalSize = 1000,
            CompressedSize = 1200
        };

        Assert.Equal(-20.0, result.CompressionPercentage, 5);
    }

    [Fact]
    public void CompressionPercentage_ReturnsZero_WhenOriginalSizeIsZero()
    {
        var result = new CompressionResult
        {
            OriginalSize = 0,
            CompressedSize = 0
        };

        Assert.Equal(0, result.CompressionPercentage);
    }
}
