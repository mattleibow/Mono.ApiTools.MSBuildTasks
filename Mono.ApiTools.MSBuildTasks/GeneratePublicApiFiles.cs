using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Mono.ApiTools.MSBuildTasks;

public class GeneratePublicApiFiles : Task
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

		using var resolver = new DefaultAssemblyResolver();
		resolver.RemoveSearchDirectory(".");
		resolver.RemoveSearchDirectory("bin");
		resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));

		using var assembly = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters
		{
			InMemory = true,
			AssemblyResolver = resolver
		});

		Log.LogMessage($"Generating public API files for assembly {assembly.Name}...");

		var currentApis = ExtractPublicApis(assembly);

		// Read existing shipped APIs if the file exists
		var shippedApis = new HashSet<string>();
		var shippedHasNullableEnable = false;
		if (shippedFile != null && File.Exists(shippedFile.ItemSpec))
		{
			var shippedContent = File.ReadAllText(shippedFile.ItemSpec);
			shippedHasNullableEnable = shippedContent.StartsWith("#nullable enable");

			var shippedLines = File.ReadAllLines(shippedFile.ItemSpec)
				.Where(line =>
					!string.IsNullOrWhiteSpace(line) &&
					!line.StartsWith("#nullable") &&
					!line.StartsWith("*REMOVED*"));
			foreach (var line in shippedLines)
			{
				shippedApis.Add(line);
			}
			Log.LogMessage($"Read {shippedApis.Count} existing APIs from shipped file");
		}

		// Generate and write unshipped file if specified
		if (unshippedFile != null)
		{
			var unshippedApis = GenerateUnshippedDiff(currentApis, shippedApis);
			WriteApiFile(unshippedApis, unshippedFile.ItemSpec, shippedHasNullableEnable);
			Log.LogMessage($"Generated unshipped API file: {unshippedFile.ItemSpec} with {unshippedApis.Count} entries");
		}

		return !Log.HasLoggedErrors;
	}

	private List<string> GenerateUnshippedDiff(List<string> currentApis, HashSet<string> shippedApis)
	{
		var unshippedApis = new List<string>();
		var currentApiSet = new HashSet<string>(currentApis);

		// Add new APIs (in current but not in shipped)
		foreach (var api in currentApis)
		{
			if (!shippedApis.Contains(api))
			{
				unshippedApis.Add(api);
			}
		}

		// Add removed APIs (in shipped but not in current) with *REMOVED* prefix
		foreach (var shippedApi in shippedApis)
		{
			if (!currentApiSet.Contains(shippedApi))
			{
				unshippedApis.Add("*REMOVED*" + shippedApi);
			}
		}

		unshippedApis.Sort(StringComparer.Ordinal);
		return unshippedApis;
	}

	private List<string> ExtractPublicApis(AssemblyDefinition assembly)
	{
		var apis = new List<string>();

		foreach (var type in assembly.MainModule.Types.Where(t => IsPublicType(t)))
		{
			if (ShouldIncludeType(type))
			{
				ProcessType(type, apis);
			}
		}

		apis.Sort(StringComparer.Ordinal);
		return apis;
	}

	private void ProcessType(TypeDefinition type, List<string> apis)
	{
		// Add the type itself if it's not a compiler-generated type
		if (!IsCompilerGenerated(type))
		{
			apis.Add(GetTypeSignature(type));
		}

		// Process members
		foreach (var field in type.Fields.Where(f => IsPublicMember(f)))
		{
			apis.AddRange(GetFieldSignatures(field));
		}
		foreach (var property in type.Properties.Where(p => IsPublicProperty(p)))
		{
			apis.AddRange(GetPropertySignatures(property));
		}
		foreach (var method in type.Methods.Where(m => IsPublicMethod(m)))
		{
			apis.AddRange(GetMethodSignatures(method));
		}
		foreach (var @event in type.Events.Where(e => IsPublicEvent(e)))
		{
			apis.AddRange(GetEventSignatures(@event));
		}

		// Process nested types
		foreach (var nestedType in type.NestedTypes.Where(t => IsPublicType(t)))
		{
			ProcessType(nestedType, apis);
		}
	}

	private bool IsPublicType(TypeDefinition type)
	{
		return type.IsPublic || type.IsNestedPublic;
	}

	private bool ShouldIncludeType(TypeDefinition type)
	{
		// Skip compiler-generated types like <Module>
		if (type.Name == "<Module>")
			return false;

		// Skip types with special names (like those generated by compiler)
		if (type.IsSpecialName && type.Name.StartsWith("<"))
			return false;

		return true;
	}

	private bool IsCompilerGenerated(IMemberDefinition member)
	{
		return member.HasCustomAttributes &&
			member.CustomAttributes.Any(a => a.AttributeType.Name == "CompilerGeneratedAttribute");
	}

	private bool IsPublicMember(FieldDefinition field)
	{
		return field.IsPublic && !field.IsSpecialName && !IsCompilerGenerated(field);
	}

	private bool IsPublicProperty(PropertyDefinition property)
	{
		var getter = property.GetMethod;
		var setter = property.SetMethod;
		return (getter?.IsPublic == true || setter?.IsPublic == true) && !IsCompilerGenerated(property);
	}

	private bool IsPublicMethod(MethodDefinition method)
	{
		return method.IsPublic &&
			(!method.IsSpecialName || method.IsConstructor) &&
			!method.IsGetter &&
			!method.IsSetter &&
			!method.IsAddOn &&
			!method.IsRemoveOn &&
			!IsCompilerGenerated(method);
	}

	private bool IsPublicEvent(EventDefinition @event)
	{
		var addMethod = @event.AddMethod;
		var removeMethod = @event.RemoveMethod;
		return (addMethod?.IsPublic == true || removeMethod?.IsPublic == true) && !IsCompilerGenerated(@event);
	}

	private string GetTypeSignature(TypeDefinition type)
	{
		return GetFullTypeName(type).Trim();
	}

	private IEnumerable<string> GetFieldSignatures(FieldDefinition member)
	{
		yield return $"{GetFullMemberName(member)} -> {GetTypeReference(member.FieldType, member.CustomAttributes)}";
	}

	private IEnumerable<string> GetPropertySignatures(PropertyDefinition property)
	{
		if (property.GetMethod is not null)
			yield return $"{GetFullMemberName(property)}.get -> {GetTypeReference(property.PropertyType, property.CustomAttributes)}";

		if (property.SetMethod is not null)
			yield return $"{GetFullMemberName(property)}.set -> void";
	}

	private IEnumerable<string> GetMethodSignatures(MethodDefinition method)
	{
		var sb = new StringBuilder();

		if (method.IsStatic)
			sb.Append("static ");
		if (method.IsAbstract)
			sb.Append("abstract ");
		else if (method.IsVirtual && method.IsNewSlot)
			sb.Append("virtual ");
		else if (method.IsVirtual)
			sb.Append("override ");

		// Method name (for constructors, this will be the full type name)
		if (method.IsConstructor)
		{
			sb.Append(GetFullTypeName(method.DeclaringType));
			sb.Append(".");
			sb.Append(method.DeclaringType.Name);
		}
		else
		{
			sb.Append(GetFullMemberName(method));
		}

		// Generic parameters
		if (method.HasGenericParameters)
		{
			sb.Append("<");
			sb.Append(string.Join(", ", method.GenericParameters.Select(gp => gp.Name)));
			sb.Append(">");
		}

		// Parameters
		sb.Append("(");
		if (method.HasParameters)
		{
			var parameters = method.Parameters.Select(p =>
			{
				var paramStr = "";
				if (p.IsOut)
					paramStr += "out ";
				else if (p.ParameterType.IsByReference)
					paramStr += "ref ";

				paramStr += GetTypeReference(p.ParameterType);
				paramStr += " " + p.Name;

				if (p.HasDefault)
				{
					paramStr += " = " + FormatConstantValue(p.Constant);
				}

				return paramStr;
			});
			sb.Append(string.Join(", ", parameters));
		}
		sb.Append(")");

		// Return type for methods (constructors have -> void)
		sb.Append(" -> ");
		if (method.IsConstructor)
		{
			sb.Append("void");
		}
		else
		{
			sb.Append(GetTypeReference(method.ReturnType));
		}

		yield return sb.ToString().Trim();
	}

	private IEnumerable<string> GetEventSignatures(EventDefinition @event)
	{
		var sb = new StringBuilder();

		var addMethod = @event.AddMethod;
		if (addMethod?.IsStatic == true)
			sb.Append("static ");

		sb.Append(GetFullMemberName(@event));
		sb.Append(" -> ");
		sb.Append(GetTypeReference(@event.EventType, @event.CustomAttributes));
		sb.Append("?");

		yield return sb.ToString().Trim();
	}

	private string GetFullTypeName(TypeReference type)
	{
		if (type.DeclaringType != null)
		{
			return GetFullTypeName(type.DeclaringType) + "." + type.Name;
		}

		var namespaceName = string.IsNullOrEmpty(type.Namespace) ? "" : type.Namespace + ".";
		return namespaceName + type.Name;
	}

	private string GetFullMemberName(MemberReference member)
	{
		return GetFullTypeName(member.DeclaringType) + "." + member.Name;
	}

	private string GetTypeReference(TypeReference type, ICollection<CustomAttribute>? attributes = null)
	{
		if (type.IsByReference)
		{
			return GetTypeReference(type.GetElementType(), attributes);
		}

		if (GetKeywordType(type) is string keywordType)
		{
			return !HasNullable(attributes)
				? type.IsValueType
					? keywordType
					: keywordType + "!"
				: keywordType + "?";
		}

		if (type.Name == "Nullable`1" && type is GenericInstanceType gen && gen.GenericArguments.Count == 1)
		{
			return GetTypeReference(gen.GenericArguments[0], attributes) + "?";
		}

		// For other types, use the full name
		if (type.DeclaringType != null)
		{
			return GetTypeReference(type.DeclaringType) + "." + type.Name;
		}

		return type.FullName.Replace("/", ".");
	}

	private static bool HasNullable(ICollection<CustomAttribute>? attributes) =>
		attributes?.Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute") ?? false;

	private static string? GetKeywordType(TypeReference type) =>
		type.FullName switch
		{
			"System.Void" => "void",
			"System.Boolean" => "bool",
			"System.Byte" => "byte",
			"System.SByte" => "sbyte",
			"System.Char" => "char",
			"System.Decimal" => "decimal",
			"System.Double" => "double",
			"System.Single" => "float",
			"System.Int32" => "int",
			"System.UInt32" => "uint",
			"System.Int64" => "long",
			"System.UInt64" => "ulong",
			"System.Int16" => "short",
			"System.UInt16" => "ushort",
			"System.Object" => "object",
			"System.String" => "string",
			_ => null,
		};

	private string FormatConstantValue(object value)
	{
		if (value == null)
			return "null";
		if (value is string str)
			return $"\"{str}\"";
		if (value is char ch)
			return $"'{ch}'";
		if (value is bool b)
			return b ? "true" : "false";
		return value.ToString();
	}

	private void WriteApiFile(List<string> apis, string filePath, bool includeNullableEnable)
	{
		var directory = Path.GetDirectoryName(filePath);
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		var lines = new List<string>();
		if (includeNullableEnable)
		{
			lines.Add("#nullable enable");
		}
		lines.AddRange(apis);

		File.WriteAllLines(filePath, lines, Encoding.UTF8);
	}
}
