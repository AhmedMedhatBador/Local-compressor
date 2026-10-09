using FileCompressor.Infrastructure.Compression;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using FileCompressor.Core.Services;

namespace FileCompressor.Tests;

public class ImageCompressorTests
{
    [Fact]
    public void ImageCompressor_CanBeCreated()
    {
        var compressor = new ImageCompressor(75);

        Assert.NotNull(compressor);
    }




    [Fact]
    public void GetUniqueOutputPath_WhenOutputExists_GeneratesNewFilename()
    {
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var inputPath = Path.Combine(tempDirectory, "photo.png");
        var existingOutputPath = Path.Combine(
            tempDirectory,
            "photo_compressed.png");

        try
        {
            File.WriteAllText(inputPath, "input");
            File.WriteAllText(existingOutputPath, "existing output");

            var service = new OutputPathService();

            var outputPath = service.GetUniqueOutputPath(
                inputPath,
                ".png");

            Assert.Equal(
                Path.Combine(tempDirectory, "photo_compressed_2.png"),
                outputPath);

            Assert.Equal(
                "existing output",
                File.ReadAllText(existingOutputPath));

            Assert.False(File.Exists(outputPath));
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }




    [Fact]
    public async Task CompressAsync_MismatchedOutputExtension_ThrowsArgumentException()
    {
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var inputPath = Path.Combine(tempDirectory, "input.jpg");
        var outputPath = Path.Combine(tempDirectory, "output.png");

        try
        {
            using (var image = new Image<Rgb24>(10, 10))
            {
                await image.SaveAsJpegAsync(inputPath);
            }

            var compressor = new ImageCompressor(75);

            await Assert.ThrowsAsync<ArgumentException>(
                () => compressor.CompressAsync(inputPath, outputPath));

            Assert.False(File.Exists(outputPath));
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }



    [Fact]
    public async Task CompressAsync_MissingInput_ThrowsFileNotFoundException()
    {
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var inputPath = Path.Combine(tempDirectory, "missing.jpg");
        var outputPath = Path.Combine(tempDirectory, "output.jpg");

        try
        {
            var compressor = new ImageCompressor(75);

            await Assert.ThrowsAsync<FileNotFoundException>(
                () => compressor.CompressAsync(inputPath, outputPath));
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task CompressAsync_UnsupportedFormat_ThrowsNotSupportedException()
    {
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var inputPath = Path.Combine(tempDirectory, "input.bmp");
        var outputPath = Path.Combine(tempDirectory, "output.bmp");

        try
        {
            using (var image = new Image<Rgb24>(10, 10))
            {
                await image.SaveAsBmpAsync(inputPath);
            }

            var compressor = new ImageCompressor(75);

            await Assert.ThrowsAsync<NotSupportedException>(
                () => compressor.CompressAsync(inputPath, outputPath));
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }


    [Fact]
    public async Task CompressAsync_Jpeg_CreatesValidOutput()
    {
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var inputPath = Path.Combine(tempDirectory, "input.jpg");
        var outputPath = Path.Combine(tempDirectory, "output.jpg");

        try
        {
            using (var image = new Image<Rgb24>(100, 100))
            {
                await image.SaveAsJpegAsync(inputPath);
            }

            var compressor = new ImageCompressor(75);

            var result = await compressor.CompressAsync(
                inputPath,
                outputPath);

            Assert.True(File.Exists(outputPath));
            Assert.True(new FileInfo(outputPath).Length > 0);
            Assert.Equal(inputPath, result.OriginalFilePath);
            Assert.Equal(outputPath, result.OutputFilePath);
            Assert.Equal(
                new FileInfo(inputPath).Length,
                result.OriginalSize);
            Assert.Equal(
                new FileInfo(outputPath).Length,
                result.CompressedSize);

            using var compressedImage = await Image.LoadAsync(outputPath);

            Assert.Equal(100, compressedImage.Width);
            Assert.Equal(100, compressedImage.Height);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }


    [Fact]
    public async Task CompressAsync_Png_CreatesValidOutput()
    {
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var inputPath = Path.Combine(tempDirectory, "input.png");
        var outputPath = Path.Combine(tempDirectory, "output.png");

        try
        {
            using (var image = new Image<Rgba32>(100, 100))
            {
                await image.SaveAsPngAsync(inputPath);
            }

            var compressor = new ImageCompressor(75);

            var result = await compressor.CompressAsync(
                inputPath,
                outputPath);

            Assert.True(File.Exists(outputPath));
            Assert.True(result.CompressedSize > 0);

            using var compressedImage = await Image.LoadAsync<Rgba32>(
                outputPath);

            Assert.Equal(100, compressedImage.Width);
            Assert.Equal(100, compressedImage.Height);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task CompressAsync_Png_PreservesTransparency()
    {
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var inputPath = Path.Combine(tempDirectory, "transparent.png");
        var outputPath = Path.Combine(tempDirectory, "transparent_compressed.png");

        try
        {
            using (var image = new Image<Rgba32>(2, 2))
            {
                image[0, 0] = new Rgba32(255, 0, 0, 255);
                image[1, 0] = new Rgba32(0, 255, 0, 0);
                image[0, 1] = new Rgba32(0, 0, 255, 128);
                image[1, 1] = new Rgba32(255, 255, 0, 255);

                await image.SaveAsPngAsync(inputPath);
            }

            var compressor = new ImageCompressor(75);

            await compressor.CompressAsync(inputPath, outputPath);

            using var compressedImage = await Image.LoadAsync<Rgba32>(
                outputPath);

            Assert.Equal((byte)255, compressedImage[0, 0].A);
            Assert.Equal((byte)0, compressedImage[1, 0].A);
            Assert.Equal((byte)128, compressedImage[0, 1].A);
            Assert.Equal((byte)255, compressedImage[1, 1].A);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

}
