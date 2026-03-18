# Mono.ApiTools.MSBuildTasks

MSBuild tasks to help with adjusting .NET assemblies during a build.

## Tasks

### AdjustReferencedAssemblyVersion

This task will allow you to update the version number of an assembly reference to match the one you actually have as part of the build.

An example use case is where an assembly is strong named and references a version of an assembly. However, you do not wish to use that version and want to override the value in the assembly with another version.

```xml
<AdjustReferencedAssemblyVersion
    Assembly="input assembly item that references another assembly"
    ReferencedAssembly="the assembly that is referenced"
    OutputAssembly="OPTIONAL assembly output path - if not provided then will overwite Assembly" />
```


### GeneratePublicApiFiles

This task will generate PublicAPI files from an assembly that can be used with the Microsoft.CodeAnalysis.PublicApiAnalyzers package.

This is useful for creating baseline API files for libraries to track public API changes over time.

```xml
<GeneratePublicApiFiles
    Assembly="input assembly item"
    OutputDirectory="OPTIONAL directory where API files will be generated - defaults to assembly directory"
    ShippedFileName="OPTIONAL name for shipped API file - defaults to PublicAPI.Shipped.txt"
    UnshippedFileName="OPTIONAL name for unshipped API file - defaults to PublicAPI.Unshipped.txt"
    GenerateShippedFile="OPTIONAL true to generate shipped file - defaults to true"
    GenerateUnshippedFile="OPTIONAL true to generate unshipped file - defaults to false" />
```

### RemoveObsoleteSymbols

This task will remove obsolete types members from an assembly.

This is useful for removing items from reference assemblies. This will allow the main assembly to retain the obsolete members but will not be visible to the developer.

```xml
<RemoveObsoleteSymbols
    Assembly="input assembly item"
    OnlyErrors="OPTIONAL true to only remove obsolete items with IsError set, otherwise all obsolete members - defaults to true"
    OutputAssembly="OPTIONAL assembly output path - if not provided then will overwite Assembly" />
```
