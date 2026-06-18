using Microsoft.Build.Utilities;
using Mono.Cecil;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Mono.ApiTools.MSBuildTasks.Tests
{
	public class RemoveObsoleteSymbolsTests : MSBuildTaskTestFixture<RemoveObsoleteSymbols>
	{
		public RemoveObsoleteSymbolsTests(ITestOutputHelper output, string testContextDirectory = null)
			: base(output, testContextDirectory)
		{
		}

		protected RemoveObsoleteSymbols GetNewTask(string assembly, bool onlyErrors = true, string outputPath = null) =>
			new()
			{
				Assembly = new TaskItem(Path.Combine(DestinationDirectory, assembly)),
				OnlyErrors = onlyErrors,
				OutputAssembly = outputPath is null ? null : new TaskItem(Path.Combine(DestinationDirectory, outputPath)),
				BuildEngine = this,
			};

		[Fact]
		public void RemovesErrorObsoleteMembers()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", true);
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var removed = new[] {
				// RootClass
				"property 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass::ObsoleteErrorProperty()'",
				"method 'System.Void Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass::ObsoleteErrorMethod()'",
				"field 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass::ObsoleteErrorField'",
				"event 'System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass::ObsoleteErrorEvent'",
				// NestedClass
				"property 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass::ObsoleteErrorProperty()'",
				"method 'System.Void Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass::ObsoleteErrorMethod()'",
				"field 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass::ObsoleteErrorField'",
				"event 'System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass::ObsoleteErrorEvent'",
				// NestedNestedClass
				"property 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass/NestedNestedClass::ObsoleteErrorProperty()'",
				"method 'System.Void Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass/NestedNestedClass::ObsoleteErrorMethod()'",
				"field 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass/NestedNestedClass::ObsoleteErrorField'",
				"event 'System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass/NestedNestedClass::ObsoleteErrorEvent'",
				// ObsoleteRootClass
				"property 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass::ObsoleteErrorProperty()'",
				"method 'System.Void Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass::ObsoleteErrorMethod()'",
				"field 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass::ObsoleteErrorField'",
				"event 'System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass::ObsoleteErrorEvent'",
				// ObsoleteErrorRootClass
				"type 'Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass'",
				// ObsoleteErrorEnum (a top-level obsolete-error enum) and the error-obsolete members
				// of EnumConsumer that use it; the property's accessors and backing field are removed
				// alongside it.
				"type 'Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorEnum'",
				"property 'Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorEnum Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.EnumConsumer::ErrorProperty()'",
				"method 'Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorEnum Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.EnumConsumer::ErrorMethod(Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorEnum)'",
				"field 'Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorEnum Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.EnumConsumer::ErrorField'",
			};

			AssertRemovedMembers(removed);
		}

		[Fact]
		public void RemovesAllObsoleteMembers()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", false);
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			var removed = new[] {
				// RootClass
				"property 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass::ObsoleteErrorProperty()'",
				"property 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass::ObsoleteProperty()'",
				"method 'System.Void Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass::ObsoleteErrorMethod()'",
				"method 'System.Void Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass::ObsoleteMethod()'",
				"field 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass::ObsoleteErrorField'",
				"field 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass::ObsoleteField'",
				"event 'System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass::ObsoleteErrorEvent'",
				"event 'System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass::ObsoleteEvent'",
				// NestedClass
				"property 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass::ObsoleteErrorProperty()'",
				"property 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass::ObsoleteProperty()'",
				"method 'System.Void Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass::ObsoleteErrorMethod()'",
				"method 'System.Void Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass::ObsoleteMethod()'",
				"field 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass::ObsoleteErrorField'",
				"field 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass::ObsoleteField'",
				"event 'System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass::ObsoleteErrorEvent'",
				"event 'System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass::ObsoleteEvent'",
				// NestedNestedClass
				"property 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass/NestedNestedClass::ObsoleteErrorProperty()'",
				"property 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass/NestedNestedClass::ObsoleteProperty()'",
				"method 'System.Void Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass/NestedNestedClass::ObsoleteErrorMethod()'",
				"method 'System.Void Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass/NestedNestedClass::ObsoleteMethod()'",
				"field 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass/NestedNestedClass::ObsoleteErrorField'",
				"field 'System.Boolean Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass/NestedNestedClass::ObsoleteField'",
				"event 'System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass/NestedNestedClass::ObsoleteErrorEvent'",
				"event 'System.EventHandler Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass/NestedClass/NestedNestedClass::ObsoleteEvent'",
				// ObsoleteRootClass
				"type 'Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteRootClass'",
				// ObsoleteErrorRootClass
				"type 'Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorRootClass'",
				// ObsoleteErrorEnum (a top-level obsolete-error enum) and every obsolete EnumConsumer
				// member that consumes it.
				"type 'Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorEnum'",
				"property 'Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorEnum Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.EnumConsumer::ErrorProperty()'",
				"method 'Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorEnum Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.EnumConsumer::ErrorMethod(Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorEnum)'",
				"field 'Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorEnum Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.EnumConsumer::ErrorField'",
			};

			AssertRemovedMembers(removed);
		}

		private void AssertRemovedMembers(params string[] removed)
		{
			var messages = LogMessageEvents
				.Select(e => e.Message)
				.Where(m => !m.StartsWith("Scanning assembly"))
				.Where(m => m != $"Removed {removed.Length} obsolete symbols.")
				.Where(m => !m.StartsWith("Removing accessor method"))
				.Where(m => !m.StartsWith("Removing backing field"))
				.Where(m => !m.Contains("k__BackingField"))
				.Where(m => !m.StartsWith("Saving assembly"))
				.ToArray();

			Assert.Equal(removed.Length, messages.Length);

			foreach (var item in removed)
			{
				Assert.Contains($"Removing {item}...", messages);
			}
		}

		[Fact]
		public void RemovesAccessorMethodsOfObsoleteProperties()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", true);
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");

			using var assembly = AssemblyDefinition.ReadAssembly(
				Path.Combine(DestinationDirectory, "Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll"));

			var rootClass = assembly.MainModule.GetType("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.RootClass");
			var methodNames = rootClass.Methods.Select(m => m.Name).ToArray();
			var fieldNames = rootClass.Fields.Select(f => f.Name).ToArray();

			// The obsolete-error property is gone...
			Assert.DoesNotContain(rootClass.Properties, p => p.Name == "ObsoleteErrorProperty");
			// ...and so are its accessor methods (the bug was that these were left behind).
			Assert.DoesNotContain("get_ObsoleteErrorProperty", methodNames);
			Assert.DoesNotContain("set_ObsoleteErrorProperty", methodNames);
			// ...and its compiler-generated backing field.
			Assert.DoesNotContain("<ObsoleteErrorProperty>k__BackingField", fieldNames);

			// The obsolete-error event and its accessor methods are gone too.
			Assert.DoesNotContain(rootClass.Events, e => e.Name == "ObsoleteErrorEvent");
			Assert.DoesNotContain("add_ObsoleteErrorEvent", methodNames);
			Assert.DoesNotContain("remove_ObsoleteErrorEvent", methodNames);
			// ...along with the field-like event's backing delegate field.
			Assert.DoesNotContain("ObsoleteErrorEvent", fieldNames);

			// Normal members and their accessors are untouched.
			Assert.Contains(rootClass.Properties, p => p.Name == "NormalProperty");
			Assert.Contains("get_NormalProperty", methodNames);
			Assert.Contains("set_NormalProperty", methodNames);
		}

		[Fact]
		public void RemovesObsoleteErrorEnumAndWritesValidAssembly()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", true);

			// Removing an [Obsolete(error: true)] enum also removes the obsolete members that use it,
			// along with their accessors and backing fields, so nothing is left behind that still
			// mentions the enum and the assembly can be written back out.
			var success = task.Execute();

			Assert.True(success, $"{task.GetType()}.Execute() failed.");
			Assert.Empty(LogErrorEvents);

			// The output assembly must be re-readable (i.e. it was written correctly).
			using var assembly = AssemblyDefinition.ReadAssembly(
				Path.Combine(DestinationDirectory, "Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll"));
			Assert.DoesNotContain(
				assembly.MainModule.GetTypes(),
				t => t.FullName == "Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.ObsoleteErrorEnum");
		}

		[Fact]
		public void RemovesEnumConsumerObsoleteMembersButKeepsUnrelated()
		{
			CopyTestFiles("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll");

			var task = GetNewTask("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll", true);
			Assert.True(task.Execute(), $"{task.GetType()}.Execute() failed.");

			using var assembly = AssemblyDefinition.ReadAssembly(
				Path.Combine(DestinationDirectory, "Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.dll"));
			var module = assembly.MainModule;
			var consumer = module.GetType("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.EnumConsumer");
			var methodNames = consumer.Methods.Select(m => m.Name).ToArray();
			var fieldNames = consumer.Fields.Select(f => f.Name).ToArray();

			// The error-obsolete members that consume the removed enum are gone, including the
			// property's accessors and compiler-generated backing field.
			Assert.DoesNotContain(consumer.Properties, p => p.Name == "ErrorProperty");
			Assert.DoesNotContain("get_ErrorProperty", methodNames);
			Assert.DoesNotContain("set_ErrorProperty", methodNames);
			Assert.DoesNotContain("<ErrorProperty>k__BackingField", fieldNames);
			Assert.DoesNotContain(consumer.Methods, m => m.Name == "ErrorMethod");
			Assert.DoesNotContain(consumer.Fields, f => f.Name == "ErrorField");

			// A non-obsolete member of the same type survives.
			Assert.Contains(consumer.Properties, p => p.Name == "KeptProperty");

			// Completely unrelated types are untouched.
			var unrelated = module.GetType("Mono.ApiTools.MSBuildTasks.Tests.TestAssembly.Amazing");
			Assert.NotNull(unrelated);
			Assert.Contains(unrelated.Methods, m => m.Name == "AmazingMethod");
		}
	}
}
