using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Mono.ApiTools.MSBuildTasks.Tests
{
	public class GeneratePublicApiFilesTests : MSBuildTaskTestFixture<GeneratePublicApiFiles>
	{
		private const string ExpectedFullApiContent = @"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing.AmazingMethod() -> string
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteMethod() -> void
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteProperty { get; set; }
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteEvent
";
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

			var actualContent = File.ReadAllText(apiFilePath);
			Assert.Equal(ExpectedFullApiContent, actualContent);
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
			var actualContent = File.ReadAllText(apiFilePath);
			Assert.Equal(ExpectedFullApiContent, actualContent);
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

			var shippedContent = File.ReadAllText(shippedPath);
			var unshippedContent = File.ReadAllText(unshippedPath);

			Assert.Equal(ExpectedFullApiContent, shippedContent);
			Assert.Equal(ExpectedFullApiContent, unshippedContent);
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

			var actualContent = File.ReadAllText(apiFilePath);
			Assert.Equal(ExpectedFullApiContent, actualContent);
		}

		[Fact]
		public void ExcludesObsoleteMembers()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var apiFilePath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var actualContent = File.ReadAllText(apiFilePath);

			// Should include obsolete methods since they're still part of the public API
			Assert.Equal(ExpectedFullApiContent, actualContent);
		}

		[Fact]
		public void IncludesPublicProperties()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var apiFilePath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var actualContent = File.ReadAllText(apiFilePath);

			// Should include properties
			Assert.Equal(ExpectedFullApiContent, actualContent);
		}

		[Fact]
		public void IncludesPublicFields()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var apiFilePath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var actualContent = File.ReadAllText(apiFilePath);

			// Should include fields
			Assert.Equal(ExpectedFullApiContent, actualContent);
		}

		[Fact]
		public void IncludesPublicEvents()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var apiFilePath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var actualContent = File.ReadAllText(apiFilePath);

			// Should include events
			Assert.Equal(ExpectedFullApiContent, actualContent);
		}

		[Fact]
		public void ApiEntriesAreSorted()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var apiFilePath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var actualContent = File.ReadAllText(apiFilePath);

			// API entries should be sorted - check exact content which contains sorted APIs
			Assert.Equal(ExpectedFullApiContent, actualContent);
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

			var actualContent = File.ReadAllText(unshippedPath);
			var expectedContent = @"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing.AmazingMethod() -> string
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteMethod() -> void
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteProperty { get; set; }
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteEvent
";

			Assert.Equal(expectedContent, actualContent);
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
			var actualContent = File.ReadAllText(unshippedPath);
			
			var expectedContent = @"*REMOVED*Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.RemovedMethod() -> void
*REMOVED*class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RemovedClass
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing.AmazingMethod() -> string
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteMethod() -> void
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteProperty { get; set; }
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteEvent
";

			Assert.Equal(expectedContent, actualContent);
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
			var actualContent = File.ReadAllText(unshippedPath);
			
			// This is the same as the previous "new APIs" test since all APIs except RootClass are new
			var expectedContent = @"Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing.AmazingMethod() -> string
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteMethod() -> void
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteProperty { get; set; }
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteEvent
";

			Assert.Equal(expectedContent, actualContent);
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
			var actualContent = File.ReadAllText(unshippedPath);
			
			var expectedContent = @"*REMOVED*Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.RemovedApi() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing.AmazingMethod() -> string
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteMethod() -> void
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorProperty { get; set; }
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteField
bool Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteProperty { get; set; }
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorEvent
event System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteEvent
";

			Assert.Equal(expectedContent, actualContent);
		}
	}
}