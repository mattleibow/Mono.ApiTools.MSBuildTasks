using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using System;
using System.IO;
using System.Linq;

namespace Mono.ApiTools.MSBuildTasks
{
	public class RemoveObsoleteSymbols : Task
	{
		[Required]
		public ITaskItem Assembly { get; set; } = null!;

		public bool OnlyErrors { get; set; } = true;

		public ITaskItem? OutputAssembly { get; set; }

		public override bool Execute()
		{
			using var resolver = new DefaultAssemblyResolver();
			resolver.RemoveSearchDirectory(".");
			resolver.RemoveSearchDirectory("bin");
			resolver.AddSearchDirectory(Path.GetDirectoryName(Assembly.ItemSpec));

			using var mainAssembly = AssemblyDefinition.ReadAssembly(Assembly.ItemSpec, new ReaderParameters
			{
				InMemory = true,
				AssemblyResolver = resolver
			});

			var module = mainAssembly.MainModule;

			Log.LogMessage($"Scanning assembly {mainAssembly.Name} for obsolete types...");

			foreach (var type in module.Types.ToArray())
			{
				ProcessType(type);
			}

			var dest = (OutputAssembly ?? Assembly).ItemSpec;
			Log.LogMessage($"Saving assembly {mainAssembly.Name} to {dest}...");
			mainAssembly.Write(dest);

			return !Log.HasLoggedErrors;
		}

		private void ProcessType(TypeDefinition type)
		{
			if (ShouldRemove(type))
			{
				Log.LogMessage($"Removing type '{type.FullName}'...");

				if (type.DeclaringType is null)
					type.Module.Types.Remove(type);
				else
					type.DeclaringType.NestedTypes.Remove(type);

				return;
			}

			foreach (var property in type.Properties.ToArray())
			{
				if (ShouldRemove(property))
				{
					Log.LogMessage($"Removing property '{property.FullName}'...");
					type.Properties.Remove(property);
				}
			}

			foreach (var method in type.Methods.ToArray())
			{
				if (ShouldRemove(method))
				{
					Log.LogMessage($"Removing method '{method.FullName}'...");
					type.Methods.Remove(method);
				}
			}

			foreach (var evnt in type.Events.ToArray())
			{
				if (ShouldRemove(evnt))
				{
					Log.LogMessage($"Removing event '{evnt.FullName}'...");
					type.Events.Remove(evnt);
				}
			}

			foreach (var field in type.Fields.ToArray())
			{
				if (ShouldRemove(field))
				{
					Log.LogMessage($"Removing field '{field.FullName}'...");
					type.Fields.Remove(field);
				}
			}

			foreach (var nestedType in type.NestedTypes)
			{
				ProcessType(nestedType);
			}
		}

		private static readonly string ObsoleteAttribute = typeof(ObsoleteAttribute).FullName;

		private bool ShouldRemove(ICustomAttributeProvider symbol)
		{
			var obs = symbol.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == ObsoleteAttribute);
			if (obs is null)
				return false;

			if (!OnlyErrors)
				return true;

			if (obs.ConstructorArguments.Count >= 2 && obs.ConstructorArguments[1].Value is bool isError && isError)
				return true;

			return false;
		}
	}
}
