using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Mono.ApiTools.MSBuildTasks.Tests;

public class PublicApiFileTests
{
    private string CreateTempFile(IEnumerable<string> lines)
    {
        var path = Path.GetTempFileName();
        File.WriteAllLines(path, lines);
        return path;
    }


    [Fact]
    public void LoadShippedPublicApiFile_ParsesLinesCorrectly_WithNullableEnable()
    {
        // Arrange
        var path = CreateTempFile([
            "#nullable enable",
            "MyType",
            "MyType.MyMethod() -> void",
            "*REMOVED*OldType"
        ]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadShippedPublicApiFile(path);

        // Assert
        Assert.True(apiFile.HasNullableEnable);
        Assert.Contains("MyType", apiFile.PublicApis);
        Assert.Contains("MyType.MyMethod() -> void", apiFile.PublicApis);
        Assert.DoesNotContain("*REMOVED*OldType", apiFile.PublicApis);
        Assert.Equal(2, apiFile.Count);
    }

    [Fact]
    public void LoadShippedPublicApiFile_ParsesLinesCorrectly_WithoutNullableEnable()
    {
        // Arrange
        var path = CreateTempFile([
            "MyType",
            "MyType.MyMethod() -> void"
        ]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadShippedPublicApiFile(path);

        // Assert
        Assert.False(apiFile.HasNullableEnable);
        Assert.Contains("MyType", apiFile.PublicApis);
        Assert.Contains("MyType.MyMethod() -> void", apiFile.PublicApis);
        Assert.Equal(2, apiFile.Count);
    }

    [Fact]
    public void LoadShippedPublicApiFile_RemovesDuplicatesAndWhitespace()
    {
        // Arrange
        var path = CreateTempFile([
            "#nullable enable",
            "  MyType  ",
            "MyType",
            "",
            "\t",
            "*REMOVED*OldType"
        ]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadShippedPublicApiFile(path);

        // Assert
        Assert.Single(apiFile.PublicApis);
        Assert.Equal("MyType", apiFile.PublicApis[0]);
    }

    [Fact]
    public void LoadShippedPublicApiFile_EmptyFile_ResultsInNoApis()
    {
        // Arrange
        var path = CreateTempFile(Array.Empty<string>());
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadShippedPublicApiFile(path);

        // Assert
        Assert.Empty(apiFile.PublicApis);
    }

    [Fact]
    public void Save_WritesFileWithNullableEnable()
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
    public void Save_WritesFileWithoutNullableEnable()
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

    [Fact]
    public void GenerateUnshippedPublicApiFile_DetectsAddedAndRemoved()
    {
        // Arrange
        var shipped = new PublicApiFile();
        shipped.LoadShippedPublicApiFile(CreateTempFile(["A", "B"]));
        var current = new PublicApiFile();
        current.LoadShippedPublicApiFile(CreateTempFile(["A", "C"]));

        // Act
        var diff = current.GenerateUnshippedPublicApiFile(shipped);

        // Assert
        Assert.Contains("B", diff.PublicApis);
        Assert.Contains("*REMOVED*C", diff.PublicApis);
        Assert.DoesNotContain("A", diff.PublicApis);
    }

    [Fact]
    public void GenerateUnshippedPublicApiFile_EmptyDiffWhenNoChange()
    {
        // Arrange
        var shipped = new PublicApiFile();
        shipped.LoadShippedPublicApiFile(CreateTempFile(["A", "B"]));
        var current = new PublicApiFile();
        current.LoadShippedPublicApiFile(CreateTempFile(["A", "B"]));

        // Act
        var diff = current.GenerateUnshippedPublicApiFile(shipped);

        // Assert
        Assert.Empty(diff.PublicApis);
    }

    [Fact]
    public void Count_ReflectsNumberOfApis()
    {
        // Arrange
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadShippedPublicApiFile(CreateTempFile(["A", "B", "C"]));

        // Assert
        Assert.Equal(3, apiFile.Count);
    }

    [Fact]
    public void LoadShippedPublicApiFile_SortsEntriesAlphabetically()
    {
        // Arrange
        var path = CreateTempFile(["CType", "AType", "BType"]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadShippedPublicApiFile(path);

        // Assert
        Assert.Equal(["AType", "BType", "CType"], apiFile.PublicApis);
    }

    [Fact]
    public void LoadShippedPublicApiFile_SortsEntriesAlphabetically_WithNullableEnable()
    {
        // Arrange
        var path = CreateTempFile([
            "#nullable enable",
            "CType",
            "AType",
            "BType"
        ]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadShippedPublicApiFile(path);

        // Assert
        Assert.Equal(["AType", "BType", "CType"], apiFile.PublicApis);
        Assert.True(apiFile.HasNullableEnable);
    }

    [Fact]
    public void LoadShippedPublicApiFile_SortsEntries_IgnoresRemovedPrefixForSorting()
    {
        // Arrange
        var path = CreateTempFile([
            "BType",
            "*REMOVED*AType",
            "CType"
        ]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadShippedPublicApiFile(path);

        // Assert
        Assert.Equal(["BType", "CType"], apiFile.PublicApis);
    }
}
