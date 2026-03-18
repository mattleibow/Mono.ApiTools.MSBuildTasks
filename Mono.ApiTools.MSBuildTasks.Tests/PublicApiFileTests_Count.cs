using System.IO;
using Xunit;

namespace Mono.ApiTools.MSBuildTasks.Tests;

public class PublicApiFileTests_Count
{
    private string CreateTempFile(string[] lines)
    {
        var path = Path.GetTempFileName();
        File.WriteAllLines(path, lines);
        return path;
    }

    [Fact]
    public void ReflectsNumberOfApis()
    {
        // Arrange
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadShippedPublicApiFile(CreateTempFile(["A", "B", "C"]));

        // Assert
        Assert.Equal(3, apiFile.Count);
    }
}
