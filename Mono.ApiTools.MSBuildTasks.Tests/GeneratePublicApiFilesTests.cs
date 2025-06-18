using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Mono.ApiTools.MSBuildTasks.Tests;

public class GeneratePublicApiFilesTests : MSBuildTaskTestFixture<GeneratePublicApiFiles>
{
	public GeneratePublicApiFilesTests(ITestOutputHelper output, string testContextDirectory = null)
		: base(output, testContextDirectory)
	{
	}

	protected GeneratePublicApiFiles GetNewTask() =>
		new()
		{
			Assembly = new TaskItem(Path.Combine(DestinationDirectory, "Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll")),
			Files = Directory.GetFiles(DestinationDirectory, "PublicAPI.*.txt")
				.Select(f => new TaskItem(f))
				.ToArray(),
			BuildEngine = this,
		};

	[Fact]
	public void GeneratesUnshippedApiFileWithOnlyNewApis()
	{
		CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
		WriteFile("PublicAPI.Unshipped.txt", "#nullable enable");

		// Create a shipped file with a subset of APIs
		var shippedApis =
			$"""
			{TestPartialShippedApiContent}
			""";
		WriteFile("PublicAPI.Shipped.txt", shippedApis);

		var unshippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Unshipped.txt");

		var task = GetNewTask();
		var success = task.Execute();

		Assert.True(success, $"{task.GetType()}.Execute() failed.");
		Assert.True(File.Exists(unshippedPath), "PublicAPI.Unshipped.txt file should be created");

		var actualContent = File.ReadAllText(unshippedPath);

		var expectedContent =
			"""
			#nullable enable
			Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing
			Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing.Amazing() -> void
			Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing.AmazingMethod() -> string!
			""";

		Output.WriteLine("Actual Unshipped API Content:");
		Output.WriteLine(actualContent);
		Output.WriteLine("Expected Unshipped API Content:");
		Output.WriteLine(expectedContent);

		Assert.Equal(expectedContent, actualContent);
	}

	[Fact]
	public void GeneratesUnshippedApiFileWithOnlyRemovedApis()
	{
		CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
		WriteFile("PublicAPI.Unshipped.txt", "#nullable enable");

		// Create a shipped file with APIs that don't exist in the current assembly
		var shippedApis =
			$"""
			{ExpectedFullApiContent}
			Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Sad
			Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Sad.Sad() -> void
			Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Sad.SadMethod() -> string!
			""";
		WriteFile("PublicAPI.Shipped.txt", shippedApis);

		var unshippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Unshipped.txt");

		var task = GetNewTask();
		var success = task.Execute();

		Assert.True(success, $"{task.GetType()}.Execute() failed.");

		var actualContent = File.ReadAllText(unshippedPath);
		var expectedContent =
			"""
			#nullable enable
			*REMOVED*Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Sad
			*REMOVED*Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Sad.Sad() -> void
			*REMOVED*Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Sad.SadMethod() -> string!
			""";

		Output.WriteLine("Actual Unshipped API Content:");
		Output.WriteLine(actualContent);
		Output.WriteLine("Expected Unshipped API Content:");
		Output.WriteLine(expectedContent);

		Assert.Equal(expectedContent, actualContent);
	}

	[Fact]
	public void GeneratesUnshippedApiFileWithNewAndRemovedApis()
	{
		CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");
		WriteFile("PublicAPI.Unshipped.txt", "#nullable enable");

		// Create a shipped file with APIs that don't exist in the current assembly
		var shippedApis =
			$"""
			{TestPartialShippedApiContent}
			Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Sad
			Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Sad.Sad() -> void
			Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Sad.SadMethod() -> string!
			""";
		WriteFile("PublicAPI.Shipped.txt", shippedApis);

		var unshippedPath = Path.Combine(DestinationDirectory, "PublicAPI.Unshipped.txt");

		var task = GetNewTask();
		var success = task.Execute();

		Assert.True(success, $"{task.GetType()}.Execute() failed.");

		var actualContent = File.ReadAllText(unshippedPath);
		var expectedContent =
			"""
			#nullable enable
			Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing
			Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing.Amazing() -> void
			Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing.AmazingMethod() -> string!
			*REMOVED*Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Sad
			*REMOVED*Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Sad.Sad() -> void
			*REMOVED*Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Sad.SadMethod() -> string!
			""";

		Output.WriteLine("Actual Unshipped API Content:");
		Output.WriteLine(actualContent);
		Output.WriteLine("Expected Unshipped API Content:");
		Output.WriteLine(expectedContent);

		Assert.Equal(expectedContent, actualContent);
	}

	[Fact]
	public void ErrorWhenNoAssemblyPropertyProvided()
	{
		WriteFile("PublicAPI.Shipped.txt", ExpectedFullApiContent);
		WriteFile("PublicAPI.Unshipped.txt", "#nullable enable");

		var task = new GeneratePublicApiFiles
		{
			Files = Directory.GetFiles(DestinationDirectory, "PublicAPI.*.txt")
				.Select(f => new TaskItem(f))
				.ToArray(),
			BuildEngine = this,
		};
		var success = task.Execute();

		Assert.False(success, "Task should fail when Assembly property is not provided");
	}

	[Fact]
	public void ErrorWhenNoPublicApiFilesProvided()
	{
		CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

		var task = GetNewTask();
		var success = task.Execute();

		Assert.False(success, "Task should fail when no PublicAPI files are provided");
	}

	private const string ExpectedFullApiContent = """
		#nullable enable
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.AllTheThings() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.BasicMethodReturning(int param1, string! param2) -> string?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.BasicMethodVoid(int param1, string! param2) -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.GetOnlyProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.GetSetProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.GetSetProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.IntField -> int
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.MethodWithParams(params string![]! strings) -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.MethodWithParamsAndOptional(string? optional = null, params string![]! strings) -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.NullableIntField -> int?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.NullableStringField -> string?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.NullableStringReturn() -> string?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.SetOnlyProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.StringField -> string!
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.StringReturn() -> string!
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.this[int index, string? name].get -> int
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.this[int index].get -> int
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.WithReferencing(in int inParam, ref int refParam, out int outParam) -> string?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing.Amazing() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing.AmazingMethod() -> string!
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorRootClass() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteRootClass() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedClass() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NestedNestedClass() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.RootClass() -> void
		""";

	private const string TestPartialShippedApiContent = """
		#nullable enable
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.AllTheThings() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.BasicMethodReturning(int param1, string! param2) -> string?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.BasicMethodVoid(int param1, string! param2) -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.GetOnlyProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.GetSetProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.GetSetProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.IntField -> int
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.MethodWithParams(params string![]! strings) -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.MethodWithParamsAndOptional(string? optional = null, params string![]! strings) -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.NullableIntField -> int?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.NullableStringField -> string?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.NullableStringReturn() -> string?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.SetOnlyProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.StringField -> string!
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.StringReturn() -> string!
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.this[int index, string? name].get -> int
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.this[int index].get -> int
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.AllTheThings.WithReferencing(in int inParam, ref int refParam, out int outParam) -> string?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass.ObsoleteErrorRootClass() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteErrorProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass.ObsoleteRootClass() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedClass() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NestedNestedClass() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.NormalProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteErrorProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NestedNestedClass.ObsoleteProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.NormalProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteErrorProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NestedClass.ObsoleteProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.NormalProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteErrorProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteEvent -> System.EventHandler?
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteField -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteMethod() -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteProperty.get -> bool
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.ObsoleteProperty.set -> void
		Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass.RootClass() -> void
		""";
}