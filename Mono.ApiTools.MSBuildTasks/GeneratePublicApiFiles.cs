using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis;

namespace Mono.ApiTools.MSBuildTasks;

public class GeneratePublicApiFiles : Microsoft.Build.Utilities.Task
{
	/// <summary>
	/// The collection of files to search for PublicAPI files.
	/// This should include both PublicAPI.Shipped.txt and PublicAPI.Unshipped.txt files.
	/// </summary>
	[Required]
	public ITaskItem[] Files { get; set; } = null!;

	/// <summary>
	/// The input assembly to generate public API files for.
	/// </summary>
	[Required]
	public ITaskItem Assembly { get; set; } = null!;

	/// <summary>
	/// The collection of search paths to locate the assembly references.
	/// This can be directories or individual assembly files.
	/// </summary>
	public ITaskItem[] ReferenceSearchPaths { get; set; } = null!;

	public override bool Execute()
	{
		if (Files == null || Files.Length == 0)
		{
			Log.LogError("No files specified.");
			return false;
		}

		if (Assembly == null)
		{
			Log.LogError("Assembly property is required.");
			return false;
		}

		Log.LogMessage($"Using assembly: {Assembly.ItemSpec}");

		var assemblyPath = Path.GetFullPath(Assembly.ItemSpec);
		if (!File.Exists(assemblyPath))
		{
			Log.LogError($"Assembly file '{assemblyPath}' does not exist.");
			return false;
		}

		// Find PublicAPI files
		var shippedFile = Files.FirstOrDefault(f => Path.GetFileName(f.ItemSpec).Equals("PublicAPI.Shipped.txt", StringComparison.OrdinalIgnoreCase));
		var unshippedFile = Files.FirstOrDefault(f => Path.GetFileName(f.ItemSpec).Equals("PublicAPI.Unshipped.txt", StringComparison.OrdinalIgnoreCase));
		if (shippedFile == null && unshippedFile == null)
		{
			Log.LogError("No PublicAPI.Shipped.txt or PublicAPI.Unshipped.txt files found in the Files collection.");
			return false;
		}

		// Load the assembly and get public APIs
		var publicApiFile = new PublicApiFile();
		try
		{
			Log.LogMessage($"Generating public API files for assembly {assemblyPath}...");
			publicApiFile.LoadAssembly(Log, assemblyPath, ReferenceSearchPaths.Select(s => s.ItemSpec).ToArray());
		}
		catch (Exception ex)
		{
			Log.LogError($"Error generating public API files: {ex.Message}");
			return false;
		}

		// Read existing shipped APIs if the file exists
		PublicApiFile shippedPublicApiFile;
		if (shippedFile != null && File.Exists(shippedFile.ItemSpec))
		{
			shippedPublicApiFile = new PublicApiFile();
			shippedPublicApiFile.LoadShippedPublicApiFile(shippedFile.ItemSpec);

			Log.LogMessage($"Read {shippedPublicApiFile.Count} existing APIs from shipped file");
		}
		else
		{
			shippedPublicApiFile = new PublicApiFile();
		}

		// Generate and write unshipped file if specified
		if (unshippedFile != null)
		{
			var unshippedPublicApiFile = publicApiFile.GenerateUnshippedPublicApiFile(shippedPublicApiFile);
			unshippedPublicApiFile.Save(unshippedFile.ItemSpec);
			Log.LogMessage($"Generated unshipped API file: {unshippedFile.ItemSpec} with {unshippedPublicApiFile.Count} entries");
		}

		return !Log.HasLoggedErrors;
	}
}
