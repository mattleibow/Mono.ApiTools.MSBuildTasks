# Mono.ApiTools.MSBuildTasks

MSBuild tasks to help with adjusting .NET assemblies during a build.

## ⚡ Quick Start

```
dotnet add package Mono.ApiTools.MSBuildTasks
```

Then use the tasks in your MSBuild project files (`.csproj`, `.targets`, etc.).

## Features

### 📄 GeneratePublicApiFiles

Generate `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` files compatible with
[Microsoft.CodeAnalysis.PublicApiAnalyzers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.PublicApiAnalyzers)
for tracking API surface changes. Supports nullable annotations, oblivious markers, and
`[Experimental]` attribute prefixes.

```xml
<GeneratePublicApiFiles
    Assembly="$(OutputPath)YourLibrary.dll"
    Files="@(PublicApiFiles)"
    ReferenceSearchPaths="@(ReferencePath->'%(RootDir)%(Directory)')" />
```

| Property | Required | Description |
|----------|----------|-------------|
| `Assembly` | ✅ | The assembly to generate public API files from |
| `Files` | ✅ | The `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` file items |
| `ReferenceSearchPaths` | | Directories to search for assembly dependencies |

### 🧹 RemoveObsoleteSymbols

Remove `[Obsolete]` types and members from assemblies — useful for cleaning up reference assemblies.

```xml
<RemoveObsoleteSymbols
    Assembly="$(OutputPath)YourAssembly.dll"
    OnlyErrors="true"
    OutputAssembly="$(OutputPath)YourAssembly.ref.dll" />
```

| Property | Required | Description |
|----------|----------|-------------|
| `Assembly` | ✅ | The assembly to scan |
| `OnlyErrors` | | Only remove `[Obsolete("...", true)]` error members (default: `true`) |
| `OutputAssembly` | | Output path; defaults to modifying the input assembly in place |

### 🔧 AdjustReferencedAssemblyVersion

Update an assembly reference version to match the actual referenced assembly — useful for
strong-named assemblies that need version alignment.

```xml
<AdjustReferencedAssemblyVersion
    Assembly="$(OutputPath)YourAssembly.dll"
    ReferencedAssembly="$(OutputPath)Dependency.dll" />
```

| Property | Required | Description |
|----------|----------|-------------|
| `Assembly` | ✅ | The assembly to modify |
| `ReferencedAssembly` | ✅ | The reference assembly providing the correct version |
| `OutputAssembly` | | Output path; defaults to modifying the input assembly in place |

### 🔄 ReplaceReferencedAssembly

Replace one assembly reference with another entirely — name, version, public key token and all.

```xml
<ReplaceReferencedAssembly
    Assembly="$(OutputPath)YourAssembly.dll"
    ReferencedAssemblyName="OldDependency"
    NewReference="$(OutputPath)NewDependency.dll" />
```

| Property | Required | Description |
|----------|----------|-------------|
| `Assembly` | ✅ | The assembly to modify |
| `ReferencedAssemblyName` | ✅ | Name of the existing reference to replace |
| `NewReference` | ✅ | The new reference assembly |
| `OutputAssembly` | | Output path; defaults to modifying the input assembly in place |

## Common Use Cases

- **API surface management** — Generate and maintain PublicAPI files for library versioning
- **Reference assemblies** — Remove obsolete symbols from contract assemblies
- **Strong-named assemblies** — Align reference versions without recompiling
- **Assembly binding** — Redirect references to different assembly implementations

---

📖 [GitHub Repository](https://github.com/mattleibow/Mono.ApiTools.MSBuildTasks) · MIT License