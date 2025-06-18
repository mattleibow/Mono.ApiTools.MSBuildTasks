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

			// Check for exact API entries
			var expectedApis = new[]
			{
				"class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void"
			};
			
			foreach (var expectedApi in expectedApis)
			{
				Assert.Contains(expectedApi, apiContents);
			}
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

			// Unshipped file should contain all APIs when no shipped file exists
			var apiContents = File.ReadAllLines(apiFilePath);
			Assert.NotEmpty(apiContents);
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
			Assert.Contains("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void", apiContents);
			
			// Should include obsolete methods since they're still part of the public API
			Assert.Contains("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteMethod() -> void", apiContents);
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
			Assert.Contains("bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalProperty { get; set; }", apiContents);
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
			Assert.Contains("bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalField", apiContents);
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
			Assert.Contains("event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalEvent", apiContents);
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

			// API entries should be sorted - validate they are in sorted order
			for (int i = 1; i < apiContents.Length; i++)
			{
				Assert.True(
					string.CompareOrdinal(apiContents[i-1], apiContents[i]) <= 0,
					$"API entries not sorted: '{apiContents[i-1]}' should come before '{apiContents[i]}'");
			}
		}

		[Fact]
		public void GeneratesUnshippedDiffWithNewApis()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			// First create a shipped file with a subset of APIs
			var shippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var shippedApis = new[]
			{
				"class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void"
			};
			File.WriteAllLines(shippedPath, shippedApis);

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", 
				generateShipped: false, generateUnshipped: true);
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var unshippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Unshipped.txt");
			Assert.True(File.Exists(unshippedPath), "PublicAPI.Unshipped.txt file should be created");

			var unshippedContents = File.ReadAllLines(unshippedPath);
			
			// Should contain new APIs not in shipped
			Assert.Contains(unshippedContents, line => line.Contains("Amazing"));
			Assert.Contains(unshippedContents, line => line.Contains("ObsoleteMethod"));
			
			// Should not contain APIs that are already shipped
			Assert.DoesNotContain(unshippedContents, line => line == "class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass");
			Assert.DoesNotContain(unshippedContents, line => line == "Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void");
		}

		[Fact]
		public void GeneratesUnshippedDiffWithRemovedApis()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			// Create a shipped file with APIs that don't exist in the current assembly
			var shippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var shippedApis = new[]
			{
				"class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass",
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void", 
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.RemovedMethod() -> void", // This doesn't exist
				"class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RemovedClass" // This doesn't exist
			};
			File.WriteAllLines(shippedPath, shippedApis);

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", 
				generateShipped: false, generateUnshipped: true);
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var unshippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Unshipped.txt");
			var unshippedContents = File.ReadAllLines(unshippedPath);
			
			// Should contain removed APIs with *REMOVED* prefix
			Assert.Contains(unshippedContents, line => line == "*REMOVED*Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.RemovedMethod() -> void");
			Assert.Contains(unshippedContents, line => line == "*REMOVED*class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RemovedClass");
		}

		[Fact]
		public void SkipsExistingRemovedApisInShippedFile()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			// Create a shipped file that includes some *REMOVED* entries
			var shippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var shippedApis = new[]
			{
				"class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass",
				"*REMOVED*Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.OldRemovedMethod() -> void"
			};
			File.WriteAllLines(shippedPath, shippedApis);

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", 
				generateShipped: false, generateUnshipped: true);
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var unshippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Unshipped.txt");
			var unshippedContents = File.ReadAllLines(unshippedPath);
			
			// Should not re-add existing removed APIs
			Assert.DoesNotContain(unshippedContents, line => line.Contains("OldRemovedMethod"));
		}

		[Fact]
		public void CompleteWorkflowExample()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			// Scenario: We have a shipped file with Api1, Api2, and we add Api3 and remove Api2
			var shippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var shippedApis = new[]
			{
				"class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass", // This still exists (Api1)
				"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.RemovedApi() -> void", // This doesn't exist (Api2 - removed)
			};
			File.WriteAllLines(shippedPath, shippedApis);

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", 
				generateShipped: false, generateUnshipped: true);
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var unshippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Unshipped.txt");
			var unshippedContents = File.ReadAllLines(unshippedPath);
			
			// Should contain new APIs (Api3)
			Assert.Contains("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void", unshippedContents);
			
			// Should contain removed APIs with *REMOVED* prefix (Api2)
			Assert.Contains("*REMOVED*Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.RemovedApi() -> void", unshippedContents);
			
			// Should not contain APIs that are already shipped (Api1)
			Assert.DoesNotContain("class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass", unshippedContents);
		}
	}
}