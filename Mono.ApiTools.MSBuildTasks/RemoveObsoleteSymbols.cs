using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using System;
using System.IO;
using System.Linq;

namespace Mono.ApiTools.MSBuildTasks
{
	public class RemoveObsoleteSymbols : Microsoft.Build.Utilities.Task
	{
		// special obsolete messages:
		// https://github.com/dotnet/roslyn/blob/891584232dc8112f33376e9ee9486051a1014b24/src/Compilers/Core/Portable/MetadataReader/PEModule.cs#L1192-L1193

		private readonly string[] SpecialObsoleteMessages = new[]
		{
			"Types with embedded references are not supported in this version of your compiler.",
			"Constructors of types with required members are not supported in this version of your compiler."
		};

		[Required]
		public ITaskItem Assembly { get; set; } = null!;

		public bool OnlyErrors { get; set; } = true;

		public ITaskItem? OutputAssembly { get; set; }

		public override bool Execute()
		{
			var mainAssemblyPath = Path.GetFullPath(Assembly.ItemSpec);

			using var resolver = new DefaultAssemblyResolver();
			resolver.RemoveSearchDirectory(".");
			resolver.RemoveSearchDirectory("bin");
			resolver.AddSearchDirectory(Path.GetDirectoryName(mainAssemblyPath));

			using var mainAssembly = AssemblyDefinition.ReadAssembly(mainAssemblyPath, new ReaderParameters
			{
				InMemory = true,
				AssemblyResolver = resolver
			});

			var module = mainAssembly.MainModule;

			Log.LogMessage($"Scanning assembly {mainAssembly.Name} for obsolete types...");

			var removed = 0;
			foreach (var type in module.Types.ToArray())
			{
				removed += ProcessType(type);
			}

			var outputAssemblyPath = Path.GetFullPath((OutputAssembly ?? Assembly).ItemSpec);
			if (removed == 0)
			{
				Log.LogMessage("No obsolete types found.");
				if (mainAssemblyPath != outputAssemblyPath)
				{
					Log.LogMessage($"Copying assembly {mainAssembly.Name} to {outputAssemblyPath}...");
					File.Copy(mainAssemblyPath, outputAssemblyPath, true);
				}
			}
			else
			{
				Log.LogMessage($"Removed {removed} obsolete symbols.");
				Log.LogMessage($"Saving assembly {mainAssembly.Name} to {outputAssemblyPath}...");
				mainAssembly.Write(outputAssemblyPath);
			}

			return !Log.HasLoggedErrors;
		}

		private int ProcessType(TypeDefinition type)
		{
			if (ShouldRemove(type))
			{
				Log.LogMessage($"Removing type '{type.FullName}'...");

				if (type.DeclaringType is null)
					type.Module.Types.Remove(type);
				else
					type.DeclaringType.NestedTypes.Remove(type);

				return 1;
			}

			var removed = 0;

			foreach (var property in type.Properties.ToArray())
			{
				if (ShouldRemove(property))
				{
					removed += RemoveProperty(type, property);
				}
			}

			foreach (var evnt in type.Events.ToArray())
			{
				if (ShouldRemove(evnt))
				{
					removed += RemoveEvent(type, evnt);
				}
			}

			foreach (var method in type.Methods.ToArray())
			{
				if (ShouldRemove(method))
				{
					Log.LogMessage($"Removing method '{method.FullName}'...");
					type.Methods.Remove(method);
					removed++;
				}
			}

			foreach (var field in type.Fields.ToArray())
			{
				if (ShouldRemove(field))
				{
					Log.LogMessage($"Removing field '{field.FullName}'...");
					type.Fields.Remove(field);
					removed++;
				}
			}

			foreach (var nestedType in type.NestedTypes.ToArray())
			{
				removed += ProcessType(nestedType);
			}

			return removed;
		}

		// Removes a property together with its accessor methods (get_/set_ and any other methods)
		// and the compiler-generated backing field of an auto-property. Cecil stores accessors and
		// backing fields in the type independently of the property metadata, so removing only the
		// property would leave orphan members behind (and, when the property type is itself being
		// removed, a dangling reference that the metadata writer cannot emit).
		private int RemoveProperty(TypeDefinition type, PropertyDefinition property)
		{
			Log.LogMessage($"Removing property '{property.FullName}'...");
			type.Properties.Remove(property);

			RemoveAccessor(type, property.GetMethod, property.FullName);
			RemoveAccessor(type, property.SetMethod, property.FullName);
			foreach (var other in property.OtherMethods.ToArray())
				RemoveAccessor(type, other, property.FullName);

			RemoveBackingField(type, $"<{property.Name}>k__BackingField", property.FullName);

			return 1;
		}

		// Removes an event together with its accessor methods (add_/remove_/raise_ and others) and,
		// for a field-like event, the compiler-generated backing delegate field (which shares the
		// event's name).
		private int RemoveEvent(TypeDefinition type, EventDefinition evnt)
		{
			Log.LogMessage($"Removing event '{evnt.FullName}'...");
			type.Events.Remove(evnt);

			RemoveAccessor(type, evnt.AddMethod, evnt.FullName);
			RemoveAccessor(type, evnt.RemoveMethod, evnt.FullName);
			RemoveAccessor(type, evnt.InvokeMethod, evnt.FullName);
			foreach (var other in evnt.OtherMethods.ToArray())
				RemoveAccessor(type, other, evnt.FullName);

			RemoveBackingField(type, evnt.Name, evnt.FullName, evnt.EventType);

			return 1;
		}

		private void RemoveAccessor(TypeDefinition type, MethodDefinition? accessor, string owner)
		{
			if (accessor is null)
				return;

			if (type.Methods.Remove(accessor))
				Log.LogMessage($"Removing accessor method '{accessor.FullName}' of '{owner}'...");
		}

		// Removes a compiler-generated backing field by name. The field is only removed when it is
		// private (so a user-declared field is never touched) and, when a type is supplied, when its
		// type matches — this keeps the field-like event match from removing an unrelated member.
		private void RemoveBackingField(TypeDefinition type, string fieldName, string owner, TypeReference? expectedType = null)
		{
			var field = type.Fields.FirstOrDefault(f => f.Name == fieldName && f.IsPrivate);
			if (field is null)
				return;

			if (expectedType is not null && field.FieldType.FullName != expectedType.FullName)
				return;

			if (type.Fields.Remove(field))
				Log.LogMessage($"Removing backing field '{field.FullName}' of '{owner}'...");
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
			{
				if (obs.ConstructorArguments[0].Value is string message)
				{
					if (!SpecialObsoleteMessages.Contains(message))
						return true;
				}
			}

			return false;
		}
	}
}
