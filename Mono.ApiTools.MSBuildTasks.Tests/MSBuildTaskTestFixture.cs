using Microsoft.Build.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Xunit.Abstractions;

namespace Mono.ApiTools.MSBuildTasks.Tests
{
	public abstract class MSBuildTaskTestFixture<TTask> : IDisposable, IBuildEngine
		where TTask : ITask
	{
		protected readonly ITestOutputHelper Output;
		protected readonly string DestinationDirectory;

		protected List<BuildErrorEventArgs> LogErrorEvents = new();
		protected List<BuildMessageEventArgs> LogMessageEvents = new();
		protected List<CustomBuildEventArgs> LogCustomEvents = new();
		protected List<BuildWarningEventArgs> LogWarningEvents = new();

		public MSBuildTaskTestFixture(ITestOutputHelper output, string testContextDirectory = null)
		{
			Output = output;
			DestinationDirectory = testContextDirectory ?? Path.Combine(Path.GetTempPath(), GetType().Name, Path.GetRandomFileName());
		}

		void IDisposable.Dispose()
		{
			if (Directory.Exists(DestinationDirectory))
				Directory.Delete(DestinationDirectory, true);
		}

		protected void CopyTestFiles(params string[] files)
		{
			foreach (var file in files)
			{
				var dest = Path.Combine(DestinationDirectory, file);
				var destFolder = Path.GetDirectoryName(dest);

				if (!Directory.Exists(destFolder))
					Directory.CreateDirectory(destFolder);

				File.Copy(file, dest);
			}
		}

		protected void WriteFile(string fileName, string contents)
		{
			if (!Directory.Exists(DestinationDirectory))
				Directory.CreateDirectory(DestinationDirectory);

			var filePath = Path.Combine(DestinationDirectory, fileName);
			File.WriteAllText(filePath, contents);
		}

		// IBuildEngine

		bool IBuildEngine.ContinueOnError => false;

		int IBuildEngine.LineNumberOfTaskNode => 0;

		int IBuildEngine.ColumnNumberOfTaskNode => 0;

		string IBuildEngine.ProjectFileOfTaskNode => $"Fake{GetType().Name}Project.proj";

		bool IBuildEngine.BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs) => throw new NotImplementedException();

		void IBuildEngine.LogCustomEvent(CustomBuildEventArgs e)
		{
			Output.WriteLine($"CUSTOM: {e.Message}");
			LogCustomEvents.Add(e);
		}

		void IBuildEngine.LogErrorEvent(BuildErrorEventArgs e)
		{
			Output.WriteLine($"ERROR: {e.Message}");
			LogErrorEvents.Add(e);
		}

		void IBuildEngine.LogMessageEvent(BuildMessageEventArgs e)
		{
			Output.WriteLine($"MESSAGE: {e.Message}");
			LogMessageEvents.Add(e);
		}

		void IBuildEngine.LogWarningEvent(BuildWarningEventArgs e)
		{
			Output.WriteLine($"WARNING: {e.Message}");
			LogWarningEvents.Add(e);
		}
	}
}
