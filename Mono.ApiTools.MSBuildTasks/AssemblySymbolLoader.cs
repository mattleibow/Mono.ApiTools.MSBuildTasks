// Copied portions from .NET SDK's AssemblySymbolLoader source in dotnet/sdk:
// https://github.com/dotnet/sdk/blob/feb621b704347e6fb72b816a257f1ba2fdc6a385/src/Compatibility/Microsoft.DotNet.ApiSymbolExtensions/AssemblySymbolLoader.cs

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mono.ApiTools.MSBuildTasks;

internal class AssemblySymbolLoader
{
	private static readonly HashSet<string> assembliesToIgnore = [
		"System.ServiceModel.Internals",
		"Microsoft.Internal.Tasks.Dataflow",
		"MSDATASRC",
		"ADODB",
		"Microsoft.StdFormat",
		"stdole",
		"PresentationUI",
		"Microsoft.VisualBasic.Activities.Compiler",
		"SMDiagnostics",
		"System.Xaml.Hosting",
		"Microsoft.Transactions.Bridge",
		"Microsoft.Workflow.Compiler"
	];

	private readonly Dictionary<string, string> explicitReferences = new(StringComparer.OrdinalIgnoreCase);
	private readonly HashSet<string> searchDirectories = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, MetadataReference> loadedAssemblies;
	private readonly TaskLoggingHelper logger;
	private CSharpCompilation compilation;

	public AssemblySymbolLoader(TaskLoggingHelper log)
	{
		logger = log;
		loadedAssemblies = [];

		var compilationOptions = new CSharpCompilationOptions(
			OutputKind.DynamicallyLinkedLibrary,
			nullableContextOptions: NullableContextOptions.Enable,
			metadataImportOptions: MetadataImportOptions.Public);

		compilation = CSharpCompilation.Create(nameof(AssemblySymbolLoader), options: compilationOptions);
	}

	public void AddSearchDirectory(params string?[]? paths)
	{
		if (paths is null || paths.Length == 0)
			return;

		foreach (string? path in paths)
		{
			if (path is null)
				continue;

			// add the directory
			if (Directory.Exists(path))
			{
				searchDirectories.Add(path);
				continue;
			}

			// if it is a file, make sure we add the directory
			if (File.Exists(path))
			{
				var assemblyName = Path.GetFileName(path);

				// the assembly was found in a directory already added
				if (explicitReferences.TryGetValue(assemblyName, out var directory))
				{
					logger.LogMessage($"Assembly '{assemblyName}' is explicitly already referenced: {directory}.");
					continue;
				}

				var directoryName = Path.GetDirectoryName(path);
				if (directoryName is not null)
				{
					explicitReferences.Add(assemblyName, directoryName);
					searchDirectories.Add(directoryName);
				}
			}
		}
	}

	public IAssemblySymbol Load(string assemblyPath)
	{
		var name = Path.GetFileName(assemblyPath);
		if (!loadedAssemblies.TryGetValue(name, out var metadataReference))
		{
			using var stream = File.OpenRead(assemblyPath);
			metadataReference = CreateAndAddReferenceToCompilation(name, stream);
		}

		if (compilation.GetAssemblyOrModuleSymbol(metadataReference) is IAssemblySymbol assemblySymbol)
			return assemblySymbol;

		throw new Exception($"Could not load assembly from path: {assemblyPath}.");
	}

	private MetadataReference CreateAndAddReferenceToCompilation(string assemblyName, Stream fileStream)
	{
		using var reader = new PEReader(fileStream);
		if (!reader.HasMetadata)
			throw new ArgumentException($"Assembly '{assemblyName}' does not contain metadata.");

		var image = reader.GetEntireImage();
		var imageContent = image.GetContent();
		var metadataReference = MetadataReference.CreateFromImage(imageContent);

		loadedAssemblies.Add(assemblyName, metadataReference);

		compilation = compilation.AddReferences(metadataReference);

		ResolveReferences(reader);

		return metadataReference;
	}

	private void ResolveReferences(PEReader peReader)
	{
		var reader = peReader.GetMetadataReader();
		foreach (var assemblyReferenceHandle in reader.AssemblyReferences)
		{
			var assemblyReference = reader.GetAssemblyReference(assemblyReferenceHandle);
			var assemblyReferenceNameWithoutExtension = reader.GetString(assemblyReference.Name);
			var assemblyReferenceName = assemblyReferenceNameWithoutExtension + ".dll";

			// skip assemblies that should never get loaded because they are purely internal
			if (assembliesToIgnore.Contains(assemblyReferenceNameWithoutExtension))
			{
				logger.LogMessage($"Assembly '{assemblyReferenceNameWithoutExtension}' is internal and will be ignored.");
				continue;
			}

			// the assembly reference is already loaded
			if (loadedAssemblies.ContainsKey(assemblyReferenceName))
				continue;

			// an explicit path for this specific assembly was passed in directly
			if (explicitReferences.TryGetValue(assemblyReferenceName, out string? fullReferencePath))
			{
				logger.LogMessage($"Explicitly loading assembly '{assemblyReferenceName}' from path: {fullReferencePath}.");
				if (LoadReference(fullReferencePath, assemblyReferenceName))
				{
					logger.LogMessage($"Successfully loaded assembly '{assemblyReferenceName}' from path: {fullReferencePath}.");
					continue;
				}
				else
				{
					logger.LogMessage($"Failed to load assembly '{assemblyReferenceName}' from path: {fullReferencePath}.");
				}
			}

			// look in the search directories for the dependency
			foreach (string referencePathDirectory in searchDirectories)
			{
				if (LoadReference(referencePathDirectory, assemblyReferenceName))
				{
					logger.LogMessage($"Successfully loaded assembly '{assemblyReferenceName}' from directory: {referencePathDirectory}.");
					break;
				}
			}

			logger.LogMessage($"Could not find assembly '{assemblyReferenceName}' in any of the search directories: {string.Join(", ", searchDirectories)}.");
		}

		bool LoadReference(string dir, string assemblyName)
		{
			// TODO: add version check

			var potentialPath = Path.Combine(dir, assemblyName);

			if (!File.Exists(potentialPath))
				return false;

			using var resolvedStream = File.OpenRead(potentialPath);
			CreateAndAddReferenceToCompilation(assemblyName, resolvedStream);

			return true;
		}
	}
}
