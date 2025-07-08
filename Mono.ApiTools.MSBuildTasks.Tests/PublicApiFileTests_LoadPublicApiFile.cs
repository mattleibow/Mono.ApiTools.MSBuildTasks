using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Mono.ApiTools.MSBuildTasks.Tests;

public class PublicApiFileTests_LoadPublicApiFile
{
    private string CreateTempFile(IEnumerable<string> lines)
    {
        var path = Path.GetTempFileName();
        File.WriteAllLines(path, lines);
        return path;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ParsesLinesCorrectly_WithNullableEnable(bool preserveRemovedItems)
    {
        // Arrange
        var path = CreateTempFile(
        [
            "#nullable enable",
            "MyType",
            "MyType.MyMethod() -> void",
        ]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadPublicApiFile(path, preserveRemovedItems: preserveRemovedItems);

        // Assert
        Assert.True(apiFile.HasNullableEnable);
        Assert.Contains("MyType", apiFile.PublicApis);
        Assert.Contains("MyType.MyMethod() -> void", apiFile.PublicApis);
        Assert.Equal(2, apiFile.Count);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ParsesLinesCorrectly_WithoutNullableEnable(bool preserveRemovedItems)
    {
        // Arrange
        var path = CreateTempFile(
        [
            "MyType",
            "MyType.MyMethod() -> void"
        ]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadPublicApiFile(path, preserveRemovedItems: preserveRemovedItems);

        // Assert
        Assert.False(apiFile.HasNullableEnable);
        Assert.Contains("MyType", apiFile.PublicApis);
        Assert.Contains("MyType.MyMethod() -> void", apiFile.PublicApis);
        Assert.Equal(2, apiFile.Count);
    }

    [Fact]
    public void ParsesLinesCorrectly_WithPreserveRemovedItems()
    {
        // Arrange
        var path = CreateTempFile(
        [
            "#nullable enable",
            "MyType",
            "*REMOVED*OldType",
            "MyType.MyMethod() -> void"
        ]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadPublicApiFile(path, preserveRemovedItems: true);

        // Assert
        Assert.True(apiFile.HasNullableEnable);
        Assert.Contains("MyType", apiFile.PublicApis);
        Assert.Contains("*REMOVED*OldType", apiFile.PublicApis);
        Assert.Contains("MyType.MyMethod() -> void", apiFile.PublicApis);
        Assert.Equal(3, apiFile.Count);
    }

    [Fact]
    public void ParsesLinesCorrectly_WithoutPreserveRemovedItems()
    {
        // Arrange
        var path = CreateTempFile(
        [
            "#nullable enable",
            "MyType",
            "*REMOVED*OldType",
            "MyType.MyMethod() -> void"
        ]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadPublicApiFile(path, preserveRemovedItems: false);

        // Assert
        Assert.True(apiFile.HasNullableEnable);
        Assert.Contains("MyType", apiFile.PublicApis);
        Assert.DoesNotContain("*REMOVED*OldType", apiFile.PublicApis);
        Assert.Contains("MyType.MyMethod() -> void", apiFile.PublicApis);
        Assert.Equal(2, apiFile.Count);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RemovesDuplicatesAndWhitespace(bool preserveRemovedItems)
    {
        // Arrange
        var path = CreateTempFile(
        [
            "#nullable enable",
            "  MyType  ",
            "MyType",
            "",
            "\t"
        ]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadPublicApiFile(path, preserveRemovedItems: preserveRemovedItems);

        // Assert
        Assert.Single(apiFile.PublicApis);
        Assert.Equal("MyType", apiFile.PublicApis[0]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EmptyFile_ResultsInNoApis(bool preserveRemovedItems)
    {
        // Arrange
        var path = CreateTempFile([]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadPublicApiFile(path, preserveRemovedItems: preserveRemovedItems);

        // Assert
        Assert.Empty(apiFile.PublicApis);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SortsEntriesAlphabetically(bool preserveRemovedItems)
    {
        // Arrange
        var path = CreateTempFile(
        [
            "CType",
            "AType",
            "BType"
        ]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadPublicApiFile(path, preserveRemovedItems: preserveRemovedItems);

        // Assert
        Assert.Equal(["AType", "BType", "CType"], apiFile.PublicApis);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SortsEntriesAlphabetically_WithNullableEnable(bool preserveRemovedItems)
    {
        // Arrange
        var path = CreateTempFile(
        [
            "#nullable enable",
            "CType",
            "AType",
            "BType"
        ]);
        var apiFile = new PublicApiFile();

        // Act
        apiFile.LoadPublicApiFile(path, preserveRemovedItems: preserveRemovedItems);

        // Assert
        Assert.Equal(["AType", "BType", "CType"], apiFile.PublicApis);
        Assert.True(apiFile.HasNullableEnable);
    }
}
