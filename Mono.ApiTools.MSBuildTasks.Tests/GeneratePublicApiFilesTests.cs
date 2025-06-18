using Microsoft.Build.Utilities;
using System.IO;
using System.Linq;
using Xunit;

namespace Mono.ApiTools.MSBuildTasks.Tests
{
	public class GeneratePublicApiFilesTests : MSBuildTaskTestFixture<GeneratePublicApiFiles>
	{
		protected GeneratePublicApiFiles GetNewTask(string assembly, string outputDirectory = null, bool generateShipped = true, bool generateUnshipped = false) =>
			new()
			{
				Assembly = new TaskItem(Path.Combine(DestinationDirectory, assembly)),
				OutputDirectory = outputDirectory != null ? new TaskItem(Path.Combine(DestinationDirectory, outputDirectory)) : null,
				GenerateShippedFile = generateShipped,
				GenerateUnshippedFile = generateUnshipped,
				BuildEngine = this,
			};

		[Fact]
		public void GeneratesShippedApiFile()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var apiFilePath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			Assert.True(File.Exists(apiFilePath), "PublicAPI.Shipped.txt file should be created");

			var apiContents = File.ReadAllLines(apiFilePath);
			Assert.NotEmpty(apiContents);

			// Should contain some expected API entries
			Assert.Contains(apiContents, line => line.Contains("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass"));
			Assert.Contains(apiContents, line => line.Contains("NormalMethod"));
		}

		[Fact]
		public void GeneratesUnshippedApiFile()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", 
				generateShipped: false, generateUnshipped: true);
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var apiFilePath = Path.Combine(DestinationDirectory, "PublicAPI.Unshipped.txt");
			Assert.True(File.Exists(apiFilePath), "PublicAPI.Unshipped.txt file should be created");

			// Unshipped file should be empty initially
			var apiContents = File.ReadAllLines(apiFilePath);
			Assert.Empty(apiContents);
		}

		[Fact]
		public void GeneratesBothFiles()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", 
				generateShipped: true, generateUnshipped: true);
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var shippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var unshippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Unshipped.txt");

			Assert.True(File.Exists(shippedPath), "PublicAPI.Shipped.txt file should be created");
			Assert.True(File.Exists(unshippedPath), "PublicAPI.Unshipped.txt file should be created");
		}

		[Fact]
		public void GeneratesInCustomOutputDirectory()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", "api-output");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var apiFilePath = Path.Combine(DestinationDirectory, "api-output", "PublicAPI.Shipped.txt");
			Assert.True(File.Exists(apiFilePath), "PublicAPI.Shipped.txt file should be created in custom directory");
		}

		[Fact]
		public void ExcludesObsoleteMembers()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var apiFilePath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var apiContents = File.ReadAllLines(apiFilePath);

			// Should include normal methods
			Assert.Contains(apiContents, line => line.Contains("NormalMethod"));
			
			// Should include obsolete methods since they're still part of the public API
			Assert.Contains(apiContents, line => line.Contains("ObsoleteMethod"));
		}

		[Fact]
		public void IncludesPublicProperties()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var apiFilePath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var apiContents = File.ReadAllLines(apiFilePath);

			// Should include properties
			Assert.Contains(apiContents, line => line.Contains("NormalProperty"));
		}

		[Fact]
		public void IncludesPublicFields()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var apiFilePath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var apiContents = File.ReadAllLines(apiFilePath);

			// Should include fields
			Assert.Contains(apiContents, line => line.Contains("NormalField"));
		}

		[Fact]
		public void IncludesPublicEvents()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var apiFilePath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var apiContents = File.ReadAllLines(apiFilePath);

			// Should include events
			Assert.Contains(apiContents, line => line.Contains("NormalEvent"));
		}

		[Fact]
		public void ApiEntriesAreSorted()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var apiFilePath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var apiContents = File.ReadAllLines(apiFilePath);

			// API entries should be sorted
			var sortedContents = apiContents.OrderBy(x => x).ToArray();
			Assert.Equal(sortedContents, apiContents);
		}
	}
}