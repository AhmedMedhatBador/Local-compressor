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
public async Task CompressAsync_Gif_PreservesAnimationFramesAndTiming()
{
    var tempDirectory = Path.Combine(
        Path.GetTempPath(),
        Guid.NewGuid().ToString());

    Directory.CreateDirectory(tempDirectory);

    var inputPath = Path.Combine(tempDirectory, "input.gif");
    var outputPath = Path.Combine(tempDirectory, "output.gif");

    try
    {
        using (var image = new Image<Rgba32>(100, 100))
        {
            image.Metadata.GetGifMetadata().RepeatCount = 0;

            image.Frames.RootFrame.Metadata
                .GetGifMetadata().FrameDelay = 10;

            using var secondFrame = new Image<Rgba32>(100, 100);

            secondFrame.Frames.RootFrame.Metadata
                .GetGifMetadata().FrameDelay = 20;

            image.Frames.AddFrame(secondFrame.Frames.RootFrame);

            await image.SaveAsGifAsync(inputPath);
        }

        var compressor = new ImageCompressor(75);

        await compressor.CompressAsync(inputPath, outputPath);

        using var compressedImage = await Image.LoadAsync(outputPath);

        Assert.Equal(2, compressedImage.Frames.Count);
        Assert.Equal(
            10,
            compressedImage.Frames.RootFrame.Metadata
                .GetGifMetadata().FrameDelay);
        Assert.Equal(
            20,
            compressedImage.Frames[1].Metadata
                .GetGifMetadata().FrameDelay);
        Assert.Equal(
            0,
            compressedImage.Metadata.GetGifMetadata().RepeatCount);
    }
    finally
    {
        Directory.Delete(tempDirectory, recursive: true);
    }
}


[Fact]
public async Task CompressAsync_Gif_PreservesTransparency()
{
    var tempDirectory = Path.Combine(
        Path.GetTempPath(),
        Guid.NewGuid().ToString());

    Directory.CreateDirectory(tempDirectory);

    var inputPath = Path.Combine(tempDirectory, "input.gif");
    var outputPath = Path.Combine(tempDirectory, "output.gif");

    try
    {
        using (var image = new Image<Rgba32>(10, 10))
        {
            image[0, 0] = new Rgba32(255, 0, 0, 255);
            image[1, 0] = new Rgba32(0, 0, 0, 0);

            await image.SaveAsGifAsync(inputPath);
        }

        var compressor = new ImageCompressor(75);

        await compressor.CompressAsync(inputPath, outputPath);

        using var compressedImage =
            await Image.LoadAsync<Rgba32>(outputPath);

        Assert.Equal((byte)255, compressedImage[0, 0].A);
        Assert.Equal((byte)0, compressedImage[1, 0].A);
    }
    finally
    {
        Directory.Delete(tempDirectory, recursive: true);
    }
}



[Fact]
public async Task CompressPng_PreservesAllPixelValues()
{
    var inputPath = Path.Combine(
        Path.GetTempPath(),
        $"{Guid.NewGuid()}_original.png");

    var outputPath = Path.Combine(
        Path.GetTempPath(),
        $"{Guid.NewGuid()}_compressed.png");

    try
    {
        using (var original = new Image<Rgba32>(3, 2))
        {
            original[0, 0] = new Rgba32(255, 0, 0, 255);
            original[1, 0] = new Rgba32(0, 255, 0, 128);
            original[2, 0] = new Rgba32(0, 0, 255, 0);
            original[0, 1] = new Rgba32(10, 20, 30, 255);
            original[1, 1] = new Rgba32(100, 110, 120, 200);
            original[2, 1] = new Rgba32(255, 255, 255, 255);

            await original.SaveAsPngAsync(inputPath);
        }

        var compressor = new ImageCompressor(75);

        await compressor.CompressAsync(inputPath, outputPath);

        using var expected = await Image.LoadAsync<Rgba32>(inputPath);
        using var actual = await Image.LoadAsync<Rgba32>(outputPath);

        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);

        for (var y = 0; y < expected.Height; y++)
        {
            for (var x = 0; x < expected.Width; x++)
            {
                Assert.Equal(expected[x, y], actual[x, y]);
            }
        }
    }
    finally
    {
        if (File.Exists(inputPath))
            File.Delete(inputPath);

        if (File.Exists(outputPath))
            File.Delete(outputPath);
    }
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
public async Task CompressAsync_Webp_CreatesValidOutputAndPreservesDimensions()
{
    var tempDirectory = Path.Combine(
        Path.GetTempPath(),
        Guid.NewGuid().ToString());

    Directory.CreateDirectory(tempDirectory);

    var inputPath = Path.Combine(tempDirectory, "input.webp");
    var outputPath = Path.Combine(tempDirectory, "output.webp");

    try
    {
        using (var image = new Image<Rgba32>(100, 100))
        {
            image[0, 0] = new Rgba32(255, 0, 0, 255);
            image[1, 0] = new Rgba32(0, 255, 0, 128);
            image[2, 0] = new Rgba32(0, 0, 255, 0);

            await image.SaveAsWebpAsync(inputPath);
        }

        var compressor = new ImageCompressor(75);

        var result = await compressor.CompressAsync(
            inputPath,
            outputPath);

        Assert.True(File.Exists(outputPath));
        Assert.True(new FileInfo(outputPath).Length > 0);
        Assert.Equal(inputPath, result.OriginalFilePath);
        Assert.Equal(outputPath, result.OutputFilePath);

        using var compressedImage = await Image.LoadAsync<Rgba32>(outputPath);

        Assert.Equal(100, compressedImage.Width);
        Assert.Equal(100, compressedImage.Height);
        Assert.Equal((byte)255, compressedImage[0, 0].A);
        Assert.Equal((byte)128, compressedImage[1, 0].A);
        Assert.Equal((byte)0, compressedImage[2, 0].A);
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
            Assert.True(compressedImage.Metadata.GetJpegMetadata().Progressive);
       
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
