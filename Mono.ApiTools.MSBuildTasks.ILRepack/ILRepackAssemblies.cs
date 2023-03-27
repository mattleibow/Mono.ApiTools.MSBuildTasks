using ILRepacking;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Linq;

namespace Mono.ApiTools.MSBuildTasks;

public class ILRepackAssemblies : Task
{
	public bool AllowDuplicateResources { get; set; }

	public bool AllowMultipleAssemblyLevelAttributes { get; set; }

	public bool AllowWildCards { get; set; }

	//public bool AllowZeroPeKind { get; set; }

	public ITaskItem? AttributeFile { get; set; }

	//public bool Closed { get; set; }

	public bool CopyAttributes { get; set; }

	public bool DebugInfo { get; set; }

	public bool DelaySign { get; set; }

	public ITaskItem? ExcludeFile { get; set; }

	//public int FileAlignment { get; set; }

	[Required]
	public ITaskItem[] InputAssemblies { get; set; } = null!;

	public bool Internalize { get; set; }

	public ITaskItem? KeyFile { get; set; }

	public string? KeyContainer { get; set; }

	public bool Parallel { get; set; }

	//public bool PauseBeforeExit { get; set; }

	[Required]
	public ITaskItem OutputFile { get; set; } = null!;

	//public bool PublicKeyTokens { get; set; }

	//public bool StrongNameLost { get; set; }

	//public ILRepack.Kind? TargetKind { get; set; }

	//public string? TargetPlatformDirectory { get; set; }

	//public string? TargetPlatformVersion { get; set; }

	public ITaskItem[]? SearchDirectories { get; set; }

	public bool UnionMerge { get; set; }

	public string? Version { get; set; }

	public bool XmlDocumentation { get; set; }

	public bool LogVerbose { get; set; }

	public bool NoRepackRes { get; set; }

	public bool KeepOtherVersionReferences { get; set; }

	public bool LineIndexation { get; set; }

	//public List<Regex> ExcludeInternalizeMatches { get; set; }
	//public Hashtable AllowedDuplicateTypes { get; set; }
	//public List<string> AllowedDuplicateNameSpaces { get; set; }

	public string? RepackDropAttribute { get; set; }

	public bool RenameInternalized { get; set; }

	public override bool Execute()
	{
		try
		{
			var options = new RepackOptions
			{
				AllowDuplicateResources = AllowDuplicateResources,
				// AllowedDuplicateNameSpaces
				// AllowedDuplicateTypes
				AllowMultipleAssemblyLevelAttributes = AllowMultipleAssemblyLevelAttributes,
				AllowWildCards = AllowWildCards,
				// AllowZeroPeKind
				AttributeFile = AttributeFile?.ItemSpec,
				// Closed
				CopyAttributes = CopyAttributes,
				DebugInfo = DebugInfo,
				DelaySign = DelaySign,
				ExcludeFile = ExcludeFile?.ItemSpec,
				// ExcludeInternalizeMatches
				// FileAlignment
				InputAssemblies = InputAssemblies.Select(a => a.ItemSpec).ToArray(),
				Internalize = Internalize,
				KeepOtherVersionReferences = KeepOtherVersionReferences,
				KeyContainer = KeyContainer,
				KeyFile = KeyFile?.ItemSpec,
				LineIndexation = LineIndexation,
				// Log
				// LogFile
				LogVerbose = LogVerbose,
				NoRepackRes = NoRepackRes,
				OutputFile = OutputFile?.ItemSpec,
				Parallel = Parallel,
				PauseBeforeExit = false,
				// PublicKeyTokens
				RenameInternalized = RenameInternalized,
				RepackDropAttribute = RepackDropAttribute,
				SearchDirectories = SearchDirectories?.Select(a => a.ItemSpec).ToArray() ?? Array.Empty<string>(),
				// StrongNameLost
				// TargetKind
				// TargetPlatformDirectory
				// TargetPlatformVersion
				UnionMerge = UnionMerge,
				Version = Version is not null ? new Version(Version) : null,
				XmlDocumentation = XmlDocumentation
			};

			var logger = new ILRepackLogger(Log);

			var repacker = new ILRepack(options, logger);
			repacker.Repack();
		}
		catch (Exception e)
		{
			Log.LogErrorFromException(e, true);
		}

		return !Log.HasLoggedErrors;
	}

	private class ILRepackLogger : ILRepacking.ILogger
	{
		public ILRepackLogger(TaskLoggingHelper log)
		{
			Logger = log;
		}

		public TaskLoggingHelper Logger { get; }

		public bool ShouldLogVerbose { get; set; }

		public void DuplicateIgnored(string ignoredType, object ignoredObject) =>
			Logger.LogWarning($"{nameof(ILRepack)}: Duplicate ignored: {ignoredType} ({ignoredObject?.GetType().Name})");

		public void Error(string msg) => Logger.LogError(msg);

		public void Info(string msg) => Logger.LogMessage(msg);

		public void Log(object str) => Logger.LogMessage(str?.ToString());

		public void Verbose(string msg) => Logger.LogMessage(msg);

		public void Warn(string msg) => Logger.LogWarning(msg);
	}
}
