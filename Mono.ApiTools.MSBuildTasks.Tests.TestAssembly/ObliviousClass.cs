#pragma warning disable RS0041

#nullable disable

namespace Mono.ApiTools.MSBuildTasks.Tests.TestAssembly;

public class ObliviousClass
{
    public string ObliviousMethod(int param1, string param2) => throw new NotImplementedException();

    public
#nullable enable
        string
#nullable disable
        ObliviousMethodNullableReturn(int param1, string param2) => throw new NotImplementedException();

    public string ObliviousMethodNullableValueParam(
#nullable enable
        int param1,
#nullable disable
        string param2) => throw new NotImplementedException();

    public string ObliviousMethodNullableRefParam(int param1,
#nullable enable
        string param2
#nullable disable
        ) => throw new NotImplementedException();
}
