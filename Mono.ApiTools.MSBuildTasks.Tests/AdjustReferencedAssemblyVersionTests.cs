using Microsoft.Build.Utilities;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Mono.ApiTools.MSBuildTasks.Tests
{
	public class AdjustReferencedAssemblyVersionTests : MSBuildTaskTestFixture<AdjustReferencedAssemblyVersion>
	{
		public AdjustReferencedAssemblyVersionTests(ITestOutputHelper output, string testContextDirectory = null)
			: base(output, testContextDirectory)
		{
		}

		protected AdjustReferencedAssemblyVersion GetNewTask(string assembly, string referenced) =>
			new()
			{
				Assembly = new TaskItem(Path.Combine(DestinationDirectory, assembly)),
				ReferencedAssembly = new TaskItem(Path.Combine(DestinationDirectory, referenced)),
				BuildEngine = this,
			};

		[Fact]
		public void DetectsMatchingVersions()
		{
			CopyTestFiles("SkiaSharp.HarfBuzz.dll", "SkiaSharp.dll");

			var task = GetNewTask("SkiaSharp.HarfBuzz.dll", "SkiaSharp.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			Assert.Single(LogMessageEvents);
			Assert.Contains("already updated", LogMessageEvents[0].Message);
		}

		[Fact]
		public void DetectsNoReference()
		{
			CopyTestFiles("HarfBuzzSharp.dll", "SkiaSharp.dll");

			var task = GetNewTask("HarfBuzzSharp.dll", "SkiaSharp.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			Assert.Single(LogWarningEvents);
			Assert.Contains("did not reference", LogWarningEvents[0].Message);
		}

		[Fact]
		public void UpdatesVersion()
		{
			CopyTestFiles("Svg.Skia.dll", "SkiaSharp.dll");

			var task = GetNewTask("Svg.Skia.dll", "SkiaSharp.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			Assert.Single(LogMessageEvents);
			Assert.Contains("Updating assembly reference", LogMessageEvents[0].Message);
			Assert.Contains("2.80.0.0 to 2.88.0.0", LogMessageEvents[0].Message);
		}
	}
}
