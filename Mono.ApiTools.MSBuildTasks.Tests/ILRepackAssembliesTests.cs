using Microsoft.Build.Utilities;
using Mono.Cecil;
using System.IO;
using System.Linq;
using Xunit;

namespace Mono.ApiTools.MSBuildTasks.Tests;

public class ILRepackAssembliesTests : MSBuildTaskTestFixture<ILRepackAssemblies>
{
	protected ILRepackAssemblies GetNewTask(string output, params string[] input) =>
		new()
		{
			InputAssemblies = input.Select(i => new TaskItem(Path.Combine(DestinationDirectory, i))).ToArray(),
			OutputFile = new TaskItem(Path.Combine(DestinationDirectory, output)),
			BuildEngine = this,
		};

	[Fact]
	public void CanMergeAssemblies()
	{
		CopyTestFiles("SkiaSharp.dll", "SkiaSharp.HarfBuzz.dll", "HarfBuzzSharp.dll");

		var task = GetNewTask("SkiaSharp_Merged.dll", "SkiaSharp.dll", "SkiaSharp.HarfBuzz.dll", "HarfBuzzSharp.dll");
		var success = task.Execute();

		Assert.True(success, $"{task.GetType()}.Execute() failed.");

		Assert.True(File.Exists(Path.Combine(DestinationDirectory, "SkiaSharp_Merged.dll")));
	}

	[Fact]
	public void MergedAssemblyHasExpectedTypes()
	{
		CopyTestFiles("SkiaSharp.dll", "SkiaSharp.HarfBuzz.dll", "HarfBuzzSharp.dll");

		var task = GetNewTask("SkiaSharp_Merged.dll", "SkiaSharp.dll", "SkiaSharp.HarfBuzz.dll", "HarfBuzzSharp.dll");
		var success = task.Execute();

		Assert.True(success, $"{task.GetType()}.Execute() failed.");

		using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(DestinationDirectory, "SkiaSharp_Merged.dll"));

		Assert.NotNull(assembly.MainModule.GetType("SkiaSharp.SKSurface"));
		Assert.NotNull(assembly.MainModule.GetType("HarfBuzzSharp.Blob"));
		Assert.NotNull(assembly.MainModule.GetType("SkiaSharp.HarfBuzz.SKShaper"));
	}
}
