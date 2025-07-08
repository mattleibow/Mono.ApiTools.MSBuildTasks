using System.IO;
using Xunit;

namespace Mono.ApiTools.MSBuildTasks.Tests;

public class PublicApiFileTests_Save
{
    private static string CreateTempFile(string[] lines)
    {
        var path = Path.GetTempFileName();
        File.WriteAllLines(path, lines);
        return path;
    }

    [Fact]
    public void WritesFileWithNullableEnable()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        var apiFile = new PublicApiFile();
        apiFile.LoadShippedPublicApiFile(CreateTempFile(["#nullable enable", "A", "B"]));

        // Act
        apiFile.Save(tempPath);
        var lines = File.ReadAllLines(tempPath);

        // Assert
        Assert.Equal("#nullable enable", lines[0]);
        Assert.Contains("A", lines);
        Assert.Contains("B", lines);
    }

    [Fact]
    public void WritesFileWithoutNullableEnable()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        var apiFile = new PublicApiFile();
        apiFile.LoadShippedPublicApiFile(CreateTempFile(["A", "B"]));

        // Act
        apiFile.Save(tempPath);
        var lines = File.ReadAllLines(tempPath);

        // Assert
        Assert.DoesNotContain("#nullable enable", lines);
        Assert.Contains("A", lines);
        Assert.Contains("B", lines);
    }
}
