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

		protected GeneratePublicApiFiles GetNewTask(params string[] fileNames) =>
			new()
			{
				Files = fileNames.Select(f => new TaskItem(Path.Combine(DestinationDirectory, f))).ToArray(),
				BuildEngine = this,
			};

		[Fact]
		public void GeneratesUnshippedApiFileWithNewApis()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			// Create a shipped file with a subset of APIs
			var shippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var shippedApis = @"class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void";
			File.WriteAllText(shippedPath, shippedApis);

			var unshippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Unshipped.txt");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", "PublicAPI.Shipped.txt", "PublicAPI.Unshipped.txt");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");
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
		public void GeneratesUnshippedApiFileWithRemovedApis()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			// Create a shipped file with APIs that don't exist in the current assembly
			var shippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var shippedApis = @"class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.RemovedMethod() -> void
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RemovedClass";
			File.WriteAllText(shippedPath, shippedApis);

			var unshippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Unshipped.txt");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", "PublicAPI.Shipped.txt", "PublicAPI.Unshipped.txt");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

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
		public void SupportsNullableEnableInShippedFile()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			// Create a shipped file with #nullable enable
			var shippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Shipped.txt");
			var shippedApis = @"#nullable enable
class Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass
Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void";
			File.WriteAllText(shippedPath, shippedApis);

			var unshippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Unshipped.txt");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", "PublicAPI.Shipped.txt", "PublicAPI.Unshipped.txt");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");
			Assert.True(File.Exists(unshippedPath), "PublicAPI.Unshipped.txt file should be created");

			var actualContent = File.ReadAllText(unshippedPath);
			Assert.StartsWith("#nullable enable", actualContent);
		}

		[Fact]
		public void ErrorWhenNoAssemblyFileProvided()
		{
			var task = GetNewTask("PublicAPI.Unshipped.txt");
			var success = task.Execute();

			Assert.False(success, "Task should fail when no assembly file is provided");
		}

		[Fact]
		public void ErrorWhenNoPublicApiFilesProvided()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
			var success = task.Execute();

			Assert.False(success, "Task should fail when no PublicAPI files are provided");
		}

		[Fact]
		public void WorksWithOnlyUnshippedFile()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var unshippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Unshipped.txt");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", "PublicAPI.Unshipped.txt");
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");
			Assert.True(File.Exists(unshippedPath), "PublicAPI.Unshipped.txt file should be created");

			// When no shipped file exists, unshipped should contain all APIs
			var actualContent = File.ReadAllText(unshippedPath);
			Assert.Equal(ExpectedFullApiContent, actualContent);
		}
	}
}