using Microsoft.Build.Utilities;
using System.IO;
using System.Linq;
using Xunit;

namespace Mono.ApiTools.MSBuildTasks.Tests
{
	public class RemoveObsoleteSymbolsTests : MSBuildTaskTestFixture<RemoveObsoleteSymbols>
	{
		protected RemoveObsoleteSymbols GetNewTask(string assembly, bool onlyErrors = true, string outputPath = null) =>
			new()
			{
				Assembly = new TaskItem(Path.Combine(DestinationDirectory, assembly)),
				OnlyErrors = onlyErrors,
				OutputAssembly = new TaskItem(Path.Combine(DestinationDirectory, outputPath)),
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
			};

			AssertRemovedMembers(removed);
		}

		private void AssertRemovedMembers(params string[] removed)
		{
			var messages = LogMessageEvents
				.Select(e => e.Message)
				.Where(m => !m.StartsWith("Scanning assembly"))
				.Where(m => !m.StartsWith("Saving assembly"))
				.ToArray();

			Assert.Equal(removed.Length, messages.Length);

			foreach (var item in removed)
			{
				Assert.Contains($"Removing {item}...", messages);
			}
		}
	}
}
