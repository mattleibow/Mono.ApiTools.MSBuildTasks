// Copied portions from Roslyn's PublicApiAnalyzers source in dotnet/roslyn:
// https://github.com/dotnet/roslyn/blob/2714d9298c8fdd8eac3ea23d6fe5d2b4be214e1a/src/RoslynAnalyzers/PublicApiAnalyzers/Core/Analyzers/DeclarePublicApiAnalyzer.Impl.cs#L623-L667

using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;
using System.Text;

namespace Mono.ApiTools.MSBuildTasks;

public class PublicApiFile
{
	private const string NullableEnableString = "#nullable enable";

	private const string RemovedPrefix = "*REMOVED*";
	private const string ObliviousPrefix = "~";

	private static readonly SymbolDisplayFormat publicApiFormat =
		new(
			globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
			typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
			propertyStyle: SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
			genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
			memberOptions:
				SymbolDisplayMemberOptions.IncludeParameters |
				SymbolDisplayMemberOptions.IncludeContainingType |
				SymbolDisplayMemberOptions.IncludeExplicitInterface |
				SymbolDisplayMemberOptions.IncludeModifiers |
				SymbolDisplayMemberOptions.IncludeConstantValue,
			parameterOptions:
				SymbolDisplayParameterOptions.IncludeExtensionThis |
				SymbolDisplayParameterOptions.IncludeParamsRefOut |
				SymbolDisplayParameterOptions.IncludeType |
				SymbolDisplayParameterOptions.IncludeName |
				SymbolDisplayParameterOptions.IncludeDefaultValue,
			miscellaneousOptions:
				SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

	private static readonly SymbolDisplayFormat publicApiFormatWithNullability =
		publicApiFormat.WithMiscellaneousOptions(
			SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
			SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
			SymbolDisplayMiscellaneousOptions.IncludeNotNullableReferenceTypeModifier);

	private List<string> nullablePublicApis = [];
	private List<string> nonNullablePublicApis = [];

	private List<string> publicApis => HasNullableEnable
		? nullablePublicApis
		: nonNullablePublicApis;

	public bool HasNullableEnable { get; private set; } = true;

	public IReadOnlyList<string> NullablePublicApis => nullablePublicApis.AsReadOnly();

	public IReadOnlyList<string> NonNullablePublicApis => nonNullablePublicApis.AsReadOnly();

	public IReadOnlyList<string> PublicApis => publicApis.AsReadOnly();

	public int Count => PublicApis.Count;

	public void LoadAssembly(TaskLoggingHelper logger, string assemblyPath, string[] searchPaths)
	{
		HasNullableEnable = true;
		nullablePublicApis.Clear();
		nonNullablePublicApis.Clear();

		var loader = new AssemblySymbolLoader(logger);
		loader.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));
		loader.AddSearchDirectory(searchPaths);

		var assemblySymbol = loader.Load(assemblyPath);

		foreach (var ns in assemblySymbol.GlobalNamespace.GetNamespaceMembers())
		{
			CollectNamespaceTypes(ns, assemblySymbol);
		}

		nullablePublicApis.Sort(Comparer.Instance);
		nonNullablePublicApis.Sort(Comparer.Instance);
	}

	public void LoadShippedPublicApiFile(string shippedPublicApiPath)
	{
		HasNullableEnable = true;
		nullablePublicApis.Clear();
		nonNullablePublicApis.Clear();

		var lines = File.ReadAllLines(shippedPublicApiPath);
		var cleanedLines = lines
			.Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith(RemovedPrefix))
			.Select(line => line.Trim())
			.Distinct()
			.ToList();

		if (cleanedLines.Count == 0)
			return;

		cleanedLines.Sort(Comparer.Instance);

		HasNullableEnable = lines[0] == NullableEnableString;
		if (HasNullableEnable)
			cleanedLines.RemoveAt(0);

		publicApis.AddRange(cleanedLines);
	}

	public PublicApiFile GenerateUnshippedPublicApiFile(PublicApiFile shippedPublicApiFile)
	{
		var diff = new PublicApiFile();
		diff.HasNullableEnable = shippedPublicApiFile.HasNullableEnable;

		// Find added APIs
		var addedApis = publicApis.Except(shippedPublicApiFile.publicApis);
		diff.publicApis.AddRange(addedApis);

		// Find removed APIs
		var removedApis = shippedPublicApiFile.publicApis.Except(publicApis);
		diff.publicApis.AddRange(removedApis.Select(api => RemovedPrefix + api));

		// Sort the public APIs
		diff.publicApis.Sort(Comparer.Instance);

		return diff;
	}

	public void Save(string unshippedPublicApiPath)
	{
		var lines = ToFileContents();

		var directory = Path.GetDirectoryName(unshippedPublicApiPath);
		Directory.CreateDirectory(directory);

		File.WriteAllLines(unshippedPublicApiPath, lines, Encoding.UTF8);
	}

	public List<string> ToFileContents()
	{
		var lines = publicApis.ToList();

		if (HasNullableEnable)
			lines.Insert(0, NullableEnableString);

		return lines;
	}

	private void CollectNamespaceTypes(INamespaceSymbol namespaceSymbol, IAssemblySymbol assemblySymbol)
	{
		// collect types in this namespace
		foreach (var type in namespaceSymbol.GetTypeMembers())
		{
			CollectTypeMembers(assemblySymbol, type);
		}

		// collect nested namespaces
		foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
		{
			CollectNamespaceTypes(nestedNamespace, assemblySymbol);
		}
	}

	private void CollectTypeMembers(IAssemblySymbol assemblySymbol, INamedTypeSymbol type)
	{
		if (type.DeclaredAccessibility == Accessibility.Public ||
			type.DeclaredAccessibility == Accessibility.ProtectedOrInternal ||
			type.DeclaredAccessibility == Accessibility.Protected)
		{
			// collect the type
			CollectSymbols(assemblySymbol, type);

			// collect the members of the type
			foreach (var member in type.GetMembers())
			{
				// properties are not mapped, the get and set methods are used instead
				if (member is IPropertySymbol)
					continue;

				// events are mapped via the main event, not the add/remove methods
				if (member is IMethodSymbol method && method.AssociatedSymbol is IEventSymbol)
					continue;

				// collect the member
				CollectSymbols(assemblySymbol, member);
			}

			// collect the nested types
			foreach (var nested in type.GetTypeMembers())
			{
				CollectTypeMembers(assemblySymbol, nested);
			}
		}
	}

	private void CollectSymbols(IAssemblySymbol assemblySymbol, ISymbol symbol)
	{
		var nonNullableApi = GetApiString(assemblySymbol, symbol, null, publicApiFormat);
		if (!string.IsNullOrWhiteSpace(nonNullableApi))
			nonNullablePublicApis.Add(nonNullableApi);

		var nullableApi = GetApiString(assemblySymbol, symbol, null, publicApiFormatWithNullability);
		if (!string.IsNullOrWhiteSpace(nullableApi))
			nullablePublicApis.Add(nullableApi);
	}

	private string GetApiString(IAssemblySymbol assemblySymbol, ISymbol symbol, string? experimentName, SymbolDisplayFormat format)
	{
		string publicApiName = symbol.ToDisplayString(format);

		ITypeSymbol? memberType = null;
		if (symbol is IMethodSymbol method)
			memberType = method.ReturnType;
		else if (symbol is IPropertySymbol property)
			memberType = property.Type;
		else if (symbol is IEventSymbol @event)
			memberType = @event.Type;
		else if (symbol is IFieldSymbol field)
			memberType = field.Type;

		if (memberType != null)
			publicApiName = publicApiName + " -> " + memberType.ToDisplayString(format);

		if (((symbol as INamespaceSymbol)?.IsGlobalNamespace).GetValueOrDefault())
			return string.Empty;

		if (symbol.ContainingAssembly != null && !symbol.ContainingAssembly.Equals(assemblySymbol))
			publicApiName += $" (forwarded, contained in {symbol.ContainingAssembly.Name})";

		if (experimentName != null)
			publicApiName = "[" + experimentName + "]" + publicApiName;

		if (IsOblivious(symbol))
			publicApiName = ObliviousPrefix + publicApiName;

		return publicApiName;
	}

	private bool IsOblivious(ISymbol symbol) =>
		symbol switch
		{
			INamedTypeSymbol { Kind: SymbolKind.NamedType } namedType => IsObliviousTypeDeclaration(namedType),
			IFieldSymbol field => IsObliviousField(field),
			IMethodSymbol method => IsObliviousMethod(method),
			IArrayTypeSymbol array => IsObliviousArray(array),
			ITypeSymbol type => IsObliviousType(type),
			_ => false
		};

	private static bool IsObliviousField(IFieldSymbol field) =>
		IsObliviousType(field.Type);

	private static bool IsObliviousMethod(IMethodSymbol method) =>
		IsObliviousType(method.ReturnType) || method.Parameters.Any(p => IsObliviousType(p.Type)); // TODO: type parameters

	private static bool IsObliviousTypeDeclaration(INamedTypeSymbol type) => false; // TODO: type parameters

	private static bool IsObliviousType(ITypeSymbol type)
	{
		// Check if the type is a ref type and has no nullability annotations
		if (type.IsReferenceType && type.NullableAnnotation == NullableAnnotation.None)
			return true;

		// TODO: containing types

		if (type is INamedTypeSymbol namedType)
			return namedType.TypeArguments.Any(a => IsObliviousType(a));

		return false;
	}

	private static bool IsObliviousArray(IArrayTypeSymbol array) =>
		array.NullableAnnotation == NullableAnnotation.None || IsObliviousType(array.ElementType);

	private class Comparer : IComparer<string>
	{
		public static readonly Comparer Instance = new Comparer();

		public int Compare(string? x, string? y)
		{
			if (x?.StartsWith(RemovedPrefix) == true)
				x = x.Substring(RemovedPrefix.Length);

			if (y?.StartsWith(RemovedPrefix) == true)
				y = y.Substring(RemovedPrefix.Length);

			return StringComparer.Ordinal.Compare(x, y);
		}
	}
}
