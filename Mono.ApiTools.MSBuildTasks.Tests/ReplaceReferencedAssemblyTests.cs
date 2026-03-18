using Microsoft.Build.Utilities;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Mono.ApiTools.MSBuildTasks.Tests
{
	public class ReplaceReferencedAssemblyTests : MSBuildTaskTestFixture<ReplaceReferencedAssembly>
	{
		public ReplaceReferencedAssemblyTests(ITestOutputHelper output, string testContextDirectory = null)
			: base(output, testContextDirectory)
		{
		}

		protected ReplaceReferencedAssembly GetNewTask(string assembly, string oldRef, string newRef) =>
			new()
			{
				Assembly = new TaskItem(Path.Combine(DestinationDirectory, assembly)),
				ReferencedAssemblyName = oldRef,
				NewReference = new TaskItem(Path.Combine(DestinationDirectory, newRef)),
				BuildEngine = this,
			};

		[Fact]
		public void DetectsNoChange()
		{
			CopyTestFiles(
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Bad.dll");

			var task = GetNewTask(
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Bad",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Bad.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			Assert.Single(LogMessageEvents);
			Assert.Contains("already updated", LogMessageEvents[0].Message);
		}

		[Fact]
		public void DetectsNoReference()
		{
			CopyTestFiles(
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Bad.dll",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Good.dll");

			var task = GetNewTask(
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Good",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Bad.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			Assert.Single(LogWarningEvents);
			Assert.Contains("did not reference", LogWarningEvents[0].Message);
		}

		[Fact]
		public void UpdatesVersion()
		{
			CopyTestFiles(
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Bad.dll",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Good.dll");

			var task = GetNewTask(
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Bad",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Good.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			Assert.Equal(2, LogMessageEvents.Count);
			Assert.Contains("Updating assembly reference", LogMessageEvents[0].Message);
			Assert.Contains("reference Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Bad", LogMessageEvents[0].Message);
			Assert.Contains("from Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Bad, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", LogMessageEvents[0].Message);
			Assert.Contains("to Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Good, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", LogMessageEvents[0].Message);
			Assert.Equal("New reference is Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Good, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null.", LogMessageEvents[1].Message);
		}
	}
}
