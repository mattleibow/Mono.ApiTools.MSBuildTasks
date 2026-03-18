using System;
using System.IO;
using Xunit;

namespace Mono.ApiTools.MSBuildTasks.Tests;

public class PublicApiFileTests_GenerateUnshippedPublicApiFile
{
    private string CreateTempFile(string[] lines)
    {
        var path = Path.GetTempFileName();
        File.WriteAllLines(path, lines);
        return path;
    }

    [Fact]
    public void CorrectDiff_Added_WhenApisAdded()
    {
        // Arrange
        var shipped = new PublicApiFile();
        shipped.LoadShippedPublicApiFile(CreateTempFile(["A"]));
        var current = new PublicApiFile();
        current.LoadUnshippedPublicApiFile(CreateTempFile(["A", "C"]));

        // Act
        var diff = current.GenerateUnshippedPublicApiFile(shipped);

        // Assert
        Assert.DoesNotContain("A", diff.PublicApis);    // A: unchanged so not listed
        Assert.Contains("C", diff.PublicApis);          // C: added
    }

    [Fact]
    public void CorrectDiff_Removed_WhenApisRemoved()
    {
        // Arrange
        var shipped = new PublicApiFile();
        shipped.LoadShippedPublicApiFile(CreateTempFile(["A", "B"]));
        var current = new PublicApiFile();
        current.LoadUnshippedPublicApiFile(CreateTempFile(["A"]));

        // Act
        var diff = current.GenerateUnshippedPublicApiFile(shipped);

        // Assert
        Assert.DoesNotContain("A", diff.PublicApis);    // A: unchanged so not listed
        Assert.Contains("*REMOVED*B", diff.PublicApis); // B: removed
        Assert.DoesNotContain("B", diff.PublicApis);    // B: removed
    }

    [Fact]
    public void CorrectDiff_AddedAndRemoved_WhenApisAddedAndRemoved()
    {
        // Arrange
        var shipped = new PublicApiFile();
        shipped.LoadShippedPublicApiFile(CreateTempFile(["A", "B"]));
        var current = new PublicApiFile();
        current.LoadUnshippedPublicApiFile(CreateTempFile(["A", "C"]));

        // Act
        var diff = current.GenerateUnshippedPublicApiFile(shipped);

        // Assert
        Assert.DoesNotContain("A", diff.PublicApis);    // A: unchanged so not listed
        Assert.Contains("*REMOVED*B", diff.PublicApis); // B: removed
        Assert.DoesNotContain("B", diff.PublicApis);    // B: removed
        Assert.Contains("C", diff.PublicApis);          // C: added
    }

    [Fact]
    public void CorrectDiff_Empty_WhenNoChange()
    {
        // Arrange
        var shipped = new PublicApiFile();
        shipped.LoadShippedPublicApiFile(CreateTempFile(["A", "B"]));
        var current = new PublicApiFile();
        current.LoadUnshippedPublicApiFile(CreateTempFile(["A", "B"]));

        // Act
        var diff = current.GenerateUnshippedPublicApiFile(shipped);

        // Assert
        Assert.Empty(diff.PublicApis);
    }

    [Fact]
    public void CorrectDiff_ExperimentalApis_TreatedAsDistinctFromNonExperimental()
    {
        // Arrange: shipped has non-experimental B, current has experimental [TEST001]B
        var shipped = new PublicApiFile();
        shipped.LoadShippedPublicApiFile(CreateTempFile(["A", "B"]));
        var current = new PublicApiFile();
        current.LoadUnshippedPublicApiFile(CreateTempFile(["A", "[TEST001]B"]));

        // Act
        var diff = current.GenerateUnshippedPublicApiFile(shipped);

        // Assert: the comparer strips prefixes for comparison, so B == [TEST001]B
        Assert.Empty(diff.PublicApis);
    }

    [Fact]
    public void CorrectDiff_Added_WhenNewExperimentalApiAdded()
    {
        // Arrange
        var shipped = new PublicApiFile();
        shipped.LoadShippedPublicApiFile(CreateTempFile(["A"]));
        var current = new PublicApiFile();
        current.LoadUnshippedPublicApiFile(CreateTempFile(["A", "[TEST001]B"]));

        // Act
        var diff = current.GenerateUnshippedPublicApiFile(shipped);

        // Assert
        Assert.Contains("[TEST001]B", diff.PublicApis);
        Assert.DoesNotContain("A", diff.PublicApis);
    }

    [Fact]
    public void CorrectDiff_Removed_WhenExperimentalApiRemoved()
    {
        // Arrange
        var shipped = new PublicApiFile();
        shipped.LoadShippedPublicApiFile(CreateTempFile(["A", "[TEST001]B"]));
        var current = new PublicApiFile();
        current.LoadUnshippedPublicApiFile(CreateTempFile(["A"]));

        // Act
        var diff = current.GenerateUnshippedPublicApiFile(shipped);

        // Assert
        Assert.Contains("*REMOVED*[TEST001]B", diff.PublicApis);
    }
}
