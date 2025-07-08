# Mono.ApiTools.MSBuildTasks

MSBuild tasks to help with adjusting .NET assemblies during a build.

## Features

This package provides four powerful MSBuild tasks for assembly manipulation:

### 🔧 **AdjustReferencedAssemblyVersion**
Update assembly reference versions to match your build dependencies.

```xml
<AdjustReferencedAssemblyVersion
    Assembly="YourAssembly.dll"
    ReferencedAssembly="ReferencedAssembly.dll"
    OutputAssembly="YourAssembly.dll" />
```

### 📄 **GeneratePublicApiFiles**
Generate PublicAPI files compatible with Microsoft.CodeAnalysis.PublicApiAnalyzers for API change tracking.

```xml
<GeneratePublicApiFiles
    Assembly="YourLibrary.dll"
    OutputDirectory="$(MSBuildProjectDirectory)"
    GenerateShippedFile="true" />
```

### 🧹 **RemoveObsoleteSymbols**
Clean up obsolete members from reference assemblies while preserving them in the main assembly.

```xml
<RemoveObsoleteSymbols
    Assembly="YourAssembly.dll"
    OnlyErrors="true"
    OutputAssembly="YourAssembly.ref.dll" />
```

### 🔄 **ReplaceReferencedAssembly**
Replace assembly references with different versions or implementations.

```xml
<ReplaceReferencedAssembly
    Assembly="YourAssembly.dll"
    ReferencedAssemblyName="OldReference"
    NewReference="NewReference.dll" />
```

## Getting Started

1. Install the package:
   ```
   dotnet add package Mono.ApiTools.MSBuildTasks
   ```

2. Use the tasks in your MSBuild project files (.csproj, .targets, etc.)

3. All tasks support optional `OutputAssembly` parameter - if not specified, the original assembly will be modified in place.

## Common Use Cases

- **Strong-named assemblies**: Update reference versions without recompiling
- **API surface management**: Generate and maintain PublicAPI files for libraries
- **Reference assemblies**: Remove obsolete symbols from contract assemblies
- **Assembly binding**: Redirect references to different assembly versions

---

📖 For detailed documentation and examples, visit the [GitHub repository](https://github.com/mattleibow/Mono.ApiTools.MSBuildTasks).