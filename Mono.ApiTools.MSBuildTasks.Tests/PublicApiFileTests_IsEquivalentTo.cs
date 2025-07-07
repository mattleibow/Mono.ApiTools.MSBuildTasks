using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.ApiTools.MSBuildTasks;
using Xunit;

namespace Mono.ApiTools.MSBuildTasks.Tests;

public class PublicApiFileTests_IsEquivalentTo
{
    private PublicApiFile CreateFromLines(string[] lines)
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllLines(tempFile, lines);

        var apiFile = new PublicApiFile();
        apiFile.LoadPublicApiFile(tempFile, preserveRemovedItems: true);

        File.Delete(tempFile);

        return apiFile;
    }

    [Fact]
    public void IsEquivalentTo_IdenticalFiles_ReturnsTrue()
    {
        // Arrange
        var file1 = CreateFromLines(["#nullable enable", "A", "B", "C"]);
        var file2 = CreateFromLines(["#nullable enable", "A", "B", "C"]);

        // Act & Assert
        Assert.True(file1.IsEquivalentTo(file2));
    }

    [Fact]
    public void IsEquivalentTo_DifferentOrder_ReturnsTrue()
    {
        // Arrange
        var file1 = CreateFromLines(["#nullable enable", "A", "B", "C"]);
        var file2 = CreateFromLines(["#nullable enable", "C", "B", "A"]);

        // Act & Assert
        Assert.True(file1.IsEquivalentTo(file2));
    }

    [Fact]
    public void IsEquivalentTo_ExtraLine_ReturnsFalse()
    {
        // Arrange
        var file1 = CreateFromLines(["#nullable enable", "A", "B", "C"]);
        var file2 = CreateFromLines(["#nullable enable", "A", "B", "C", "D"]);

        // Act & Assert
        Assert.False(file1.IsEquivalentTo(file2));
    }

    [Fact]
    public void IsEquivalentTo_RemovedPrefix_RespectedInComparison()
    {
        // Arrange
        var file1 = CreateFromLines(["#nullable enable", "A", "B", "C"]);
        var file2 = CreateFromLines(["#nullable enable", "A", "*REMOVED*B", "C"]);

        // Act & Assert
        Assert.False(file1.IsEquivalentTo(file2));
    }

    [Fact]
    public void IsEquivalentTo_ObliviousPrefix_IgnoredInComparison()
    {
        // Arrange
        var file1 = CreateFromLines(["#nullable enable", "A", "B", "C"]);
        var file2 = CreateFromLines(["#nullable enable", "A", "~B", "C"]);

        // Act & Assert
        Assert.True(file1.IsEquivalentTo(file2));
    }

    [Fact]
    public void IsEquivalentTo_WhitespaceAndDuplicates_Ignored()
    {
        // Arrange
        var file1 = CreateFromLines(["#nullable enable", "A", "B", "C", "B", " ", ""]);
        var file2 = CreateFromLines(["#nullable enable", "C", "B", "A"]);

        // Act & Assert
        Assert.True(file1.IsEquivalentTo(file2));
    }

    [Fact]
    public void IsEquivalentTo_DifferentContent_ReturnsFalse()
    {
        // Arrange
        var file1 = CreateFromLines(["#nullable enable", "A", "B", "C"]);
        var file2 = CreateFromLines(["#nullable enable", "A", "B", "D"]);

        // Act & Assert
        Assert.False(file1.IsEquivalentTo(file2));
    }
}
