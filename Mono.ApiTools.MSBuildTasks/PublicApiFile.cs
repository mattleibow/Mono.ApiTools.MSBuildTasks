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

	public void LoadShippedPublicApiFile(string publicApiPath) =>
		LoadPublicApiFile(publicApiPath, preserveRemovedItems: false);

	public void LoadUnshippedPublicApiFile(string publicApiPath) =>
		LoadPublicApiFile(publicApiPath, preserveRemovedItems: true);

	public void LoadPublicApiFile(string publicApiPath, bool preserveRemovedItems)
	{
		HasNullableEnable = true;
		nullablePublicApis.Clear();
		nonNullablePublicApis.Clear();

		var lines = File.ReadAllLines(publicApiPath);
		var cleanedLines = lines
			.Where(line => !string.IsNullOrWhiteSpace(line) && (preserveRemovedItems || !line.StartsWith(RemovedPrefix)))
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
		var addedApis = publicApis.Except(shippedPublicApiFile.publicApis, Comparer.Instance);

		// Add the new APIs to the diff
		diff.publicApis.AddRange(addedApis);

		// Find removed APIs
		var removedApis = shippedPublicApiFile.publicApis.Except(publicApis, Comparer.Instance);

		// In some cases, there may have been a transition from oblivious to nullable APIs
		// so we have to also check for these
		removedApis = removedApis.Except(nonNullablePublicApis, Comparer.Instance);

		// Add the removed APIs to the diff with a prefix
		diff.publicApis.AddRange(removedApis.Select(api => RemovedPrefix + api));

		// Sort the public APIs
		diff.publicApis.Sort(Comparer.Instance);

		return diff;
	}

	public bool IsEquivalentTo(PublicApiFile unshippedPublicApiFile)
	{
		var currentItems = GetLines(this);
		var unshippedItems = GetLines(unshippedPublicApiFile);

		if (currentItems.Count != unshippedItems.Count)
			return false;

		if (!currentItems.SequenceEqual(unshippedItems, StringComparer.Ordinal))
			return false;

		return true;

		static List<string> GetLines(PublicApiFile file) =>
			file.publicApis
				.Where(api => !string.IsNullOrWhiteSpace(api))
				.Select(api =>
				{
					var newApi = api.Trim();

					// The oblivious marker is more for a visual indication to us that it is missing nullability
					// and the actual API will differ if it needs to.
					//
					// For example:
					// For types:
					//   If the type is `My.Namespace.MyType`, then oblivious or not, it is the same type and the
					//   "oblivious" version is exactly the same.
					// For methods:
					//   If the method is `My.Namespace.MyType.MyMethod() -> void`, then oblivious or not, it
					//   is also the same.
					//   But, if the method is `My.Namespace.MyType.MyMethod() -> string`, then the NON oblivious
					//   version is different: `My.Namespace.MyType.MyMethod() -> string!`.
					if (newApi.StartsWith(ObliviousPrefix))
						newApi = newApi.Substring(ObliviousPrefix.Length).Trim();

					return newApi;
				})
				.OrderBy(api => api, StringComparer.Ordinal)
				.ToList();
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
				if (!ShouldCollect(member))
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

		var oblivious = IsOblivious(symbol);
		if (oblivious.IsOblivious)
		{
			publicApiName = ObliviousPrefix + publicApiName;
			// publicApiName += $" (oblivious: {oblivious.Reason})";
		}

		return publicApiName;
	}

	private bool ShouldCollect(ISymbol symbol)
	{
		if (symbol.ToDisplayString().Contains("SomeMethod"))
		{

		}
		// properties are not mapped, the get and set methods are used instead
			if (symbol is IPropertySymbol)
				return false;

		if (symbol is IMethodSymbol methodSymbol)
		{
			// events are mapped via the main event, not the add/remove methods
			if (methodSymbol is { AssociatedSymbol: IEventSymbol })
				return false;

			// enums don't have a constructor
			if (methodSymbol is { MethodKind: MethodKind.Constructor, ContainingType.TypeKind: TypeKind.Enum })
				return false;

			// only include a delegate's 'Invoke' in order to track the signature, the rest can be skipped
			if (methodSymbol is { ContainingType.TypeKind: TypeKind.Delegate, MethodKind: not MethodKind.DelegateInvoke })
				return false;
		}

		// private or internal members, or members on private or internal types, should be skipped
		if (!IsSymbolPublic(symbol))
			return false;

		// protected types or members inside a type that can never be extended, should be skipped
		if (!IsProtectedSymbolExtendable(symbol))
			return false;

		return true;
	}

	private static bool IsProtectedSymbolExtendable(ISymbol symbol)
	{
		while (symbol != null)
		{
			if (symbol.DeclaredAccessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal)
			{
				var containing = symbol.ContainingType;

				// type is sealed, so can never be extended
				if (containing.IsSealed)
					return false;

				var ctors = containing.GetMembers(WellKnownMemberNames.InstanceConstructorName);

				// if there are no instance constructors, then the type is not extendable
				if (ctors.Length == 0)
					return false;

				// check ctors for any public ones
				foreach (var ctor in ctors)
				{
					if (IsSymbolDeclaredPublic(ctor))
						return true;
				}
			}

			symbol = symbol.ContainingType;
		}

		return true;
	}

	private static bool IsSymbolPublic(ISymbol symbol)
	{
		switch (symbol.Kind)
		{
			// aliases are uber private - they're only visible in the same file that they were declared in
			case SymbolKind.Alias:
				return false;

			// parameters are only as visible as their containing symbol
			case SymbolKind.Parameter:
				return IsSymbolPublic(symbol.ContainingSymbol);

			// type parameters are private
			case SymbolKind.TypeParameter:
				return false;
		}

		while (symbol != null && symbol.Kind != SymbolKind.Namespace)
		{
			if (!IsSymbolDeclaredPublic(symbol))
				return false;

			symbol = symbol.ContainingSymbol;
		}

		return true;
	}

	private static bool IsSymbolDeclaredPublic(ISymbol symbol) =>
		symbol.DeclaredAccessibility switch
		{
			// if anything is private or internal, then it is not collected
			Accessibility.NotApplicable or
			Accessibility.Private or
			Accessibility.Internal or
			Accessibility.ProtectedAndInternal => false,
			_ => true,
		};

	private static (bool IsOblivious, string Reason) IsOblivious(ISymbol symbol) =>
		symbol switch
		{
			// e.g. public class SomeType
			INamedTypeSymbol { Kind: SymbolKind.NamedType } namedType => IsObliviousTypeDeclaration(namedType),
			// e.g. public SomeType FieldName
			IFieldSymbol field => IsObliviousField(field),
			// e.g. public void SomeMethod() or events or properties
			IMethodSymbol method => IsObliviousMethod(method),
			_ => default
		};

	private static (bool IsOblivious, string Reason) IsObliviousField(IFieldSymbol field)
	{
		var fieldType = IsObliviousTypeReference(field.Type);
		if (fieldType.IsOblivious)
			return (true, $"Field type is oblivious: {fieldType.Reason}");
		return default;
	}

	private static (bool IsOblivious, string Reason) IsObliviousMethod(IMethodSymbol method)
	{
		var retType = IsObliviousTypeReference(method.ReturnType);
		if (retType.IsOblivious)
			return (true, $"Return type is oblivious: {retType.Reason}");

		foreach (var param in method.Parameters)
		{
			var paramType = IsObliviousTypeReference(param.Type);
			if (paramType.IsOblivious)
				return (true, $"Parameter {param.Name} type is oblivious: {paramType.Reason}");
		}

		foreach (var param in method.TypeParameters)
		{
			var paramType = IsObliviousTypeParameter(param);
			if (paramType.IsOblivious)
				return (true, $"Generic type parameter {param.Name} is oblivious: {paramType.Reason}");
		}

		return default;
	}

	private static (bool IsOblivious, string Reason) IsObliviousTypeDeclaration(INamedTypeSymbol type)
	{
		foreach (var param in type.TypeParameters)
		{
			var paramType = IsObliviousTypeParameter(param);
			if (paramType.IsOblivious)
				return (true, $"Generic type parameter {param.Name} is oblivious: {paramType.Reason}");
		}

		return default;
	}

	private static (bool IsOblivious, string Reason) IsObliviousTypeReference(ITypeSymbol type)
	{
		// Check if the type is a ref type and has no nullability annotations
		if (type.IsReferenceType && type.NullableAnnotation == NullableAnnotation.None)
			return (true, "Type was oblivious");

		if (type is IArrayTypeSymbol arrayType)
		{
			var array = IsObliviousArray(arrayType);
			if (array.IsOblivious)
				return (true, $"Array is oblivious: {array.Reason}");
		}

		// TODO: containing types

		// Check if the type arguments have no nullability annotations
		if (type is INamedTypeSymbol namedType)
		{
			foreach (var arg in namedType.TypeArguments)
			{
				var typeRef = IsObliviousTypeReference(arg);
				if (typeRef.IsOblivious)
					return (true, $"Type argument {arg.Name} is oblivious: {typeRef.Reason}");
			}
		}

		return default;
	}

	private static (bool IsOblivious, string Reason) IsObliviousArray(IArrayTypeSymbol array)
	{
		if (array.NullableAnnotation == NullableAnnotation.None)
			return (true, "Array is oblivious");

		var elementType = IsObliviousTypeReference(array.ElementType);
		if (elementType.IsOblivious)
			return (true, $"Array element type is oblivious: {elementType.Reason}");

		return default;
	}

	private static (bool IsOblivious, string Reason) IsObliviousTypeParameter(ITypeParameterSymbol typeParam)
	{
		// Check if the type is a ref type and has no nullability annotations
		// e.g. where T : class~
		if (typeParam.HasReferenceTypeConstraint && typeParam.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.None)
			return (true, "Type parameter is oblivious");

		// Check if the constraints have no nullability annotations
		// e.g. where T : SomeReferenceType~
		foreach (var constraint in typeParam.ConstraintTypes)
		{
			var constraintType = IsObliviousTypeReference(constraint);
			if (constraintType.IsOblivious)
				return (true, $"Constraint {constraint.Name} is oblivious: {constraintType.Reason}");
		}

		return default;
	}

	private class Comparer : IComparer<string>, IEqualityComparer<string>
	{
		public static readonly Comparer Instance = new Comparer();

		public int Compare(string? x, string? y) =>
			StringComparer.Ordinal.Compare(WithoutPrefixes(x), WithoutPrefixes(y));

		public bool Equals(string? x, string? y) =>
			StringComparer.Ordinal.Equals(WithoutPrefixes(x), WithoutPrefixes(y));

		public int GetHashCode(string obj) =>
			WithoutPrefixes(obj).GetHashCode();

		private static string WithoutPrefixes(string? api)
		{
			if (api is null || string.IsNullOrWhiteSpace(api))
				return string.Empty;

			if (api.StartsWith(RemovedPrefix))
				api = api.Substring(RemovedPrefix.Length);

			if (api.StartsWith(ObliviousPrefix))
				api = api.Substring(ObliviousPrefix.Length);

			return api;
		}
	}
}
