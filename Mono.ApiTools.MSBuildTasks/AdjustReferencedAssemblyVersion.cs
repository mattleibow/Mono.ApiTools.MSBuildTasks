using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using System.IO;
using System.Linq;

namespace Mono.ApiTools.MSBuildTasks
{
	public class AdjustReferencedAssemblyVersion : Task
	{
		[Required]
		public ITaskItem Assembly { get; set; } = null!;

		[Required]
		public ITaskItem ReferencedAssembly { get; set; } = null!;

		public ITaskItem? OutputAssembly { get; set; }

		public override bool Execute()
		{
			using var resolver = new DefaultAssemblyResolver();
			resolver.RemoveSearchDirectory(".");
			resolver.RemoveSearchDirectory("bin");
			resolver.AddSearchDirectory(Path.GetDirectoryName(Assembly.ItemSpec));
			resolver.AddSearchDirectory(Path.GetDirectoryName(ReferencedAssembly.ItemSpec));

			using var mainAssembly = AssemblyDefinition.ReadAssembly(Assembly.ItemSpec, new ReaderParameters
			{
				InMemory = true,
				AssemblyResolver = resolver
			});

			using var refAssembly = AssemblyDefinition.ReadAssembly(ReferencedAssembly.ItemSpec);

			var mainRefs = mainAssembly.MainModule.AssemblyReferences;
			var mainReference = mainRefs.FirstOrDefault(r => r.Name == refAssembly.Name.Name);

			if (mainReference != null)
			{
				if (mainReference.Version != refAssembly.Name.Version)
				{
					Log.LogMessage($"Updating assembly reference {mainReference.Name} from {mainReference.Version} to {refAssembly.Name.Version}.");

					mainReference.Version = refAssembly.Name.Version;

					mainAssembly.Write((OutputAssembly ?? Assembly).ItemSpec);
				}
				else
				{
					Log.LogMessage($"Assembly reference {mainReference.Name} already updated to {mainReference.Version}.");
				}
			}
			else
			{
				Log.LogWarning($"Assembly {mainAssembly.Name.Name} did not reference {refAssembly.Name.Name}.");
			}

			return !Log.HasLoggedErrors;
		}
	}
}
