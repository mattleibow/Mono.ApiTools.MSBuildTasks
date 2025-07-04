using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using System.IO;
using System.Linq;

namespace Mono.ApiTools.MSBuildTasks
{
	public class ReplaceReferencedAssembly : Microsoft.Build.Utilities.Task
	{
		[Required]
		public ITaskItem Assembly { get; set; } = null!;

		[Required]
		public string ReferencedAssemblyName { get; set; } = null!;

		[Required]
		public ITaskItem NewReference { get; set; } = null!;

		public ITaskItem? OutputAssembly { get; set; }

		public override bool Execute()
		{
			using var resolver = new DefaultAssemblyResolver();
			resolver.RemoveSearchDirectory(".");
			resolver.RemoveSearchDirectory("bin");
			resolver.AddSearchDirectory(Path.GetDirectoryName(Assembly.ItemSpec));
			resolver.AddSearchDirectory(Path.GetDirectoryName(NewReference.ItemSpec));

			using var mainAssembly = AssemblyDefinition.ReadAssembly(Assembly.ItemSpec, new ReaderParameters
			{
				InMemory = true,
				AssemblyResolver = resolver
			});

			using var refAssembly = AssemblyDefinition.ReadAssembly(NewReference.ItemSpec);

			var mainRefs = mainAssembly.MainModule.AssemblyReferences;
			var mainReference = mainRefs.FirstOrDefault(r => r.Name == ReferencedAssemblyName);

			if (mainReference != null)
			{
				if (mainReference.FullName != refAssembly.FullName)
				{
					Log.LogMessage($"Updating assembly reference {mainReference.Name} from {mainReference.FullName} to {refAssembly.FullName}.");

					mainReference.Name = refAssembly.Name.Name;
					mainReference.Version = refAssembly.Name.Version;
					mainReference.Culture = refAssembly.Name.Culture;
					mainReference.PublicKeyToken = refAssembly.Name.PublicKeyToken;
					mainReference.IsRetargetable = refAssembly.Name.IsRetargetable;

					Log.LogMessage($"New reference is {mainReference.FullName}.");

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
