#nullable disable

#pragma warning disable RS0041

namespace Mono.ApiTools.MSBuildTasks.Tests.TestAssembly;

public interface IObliviousGenericInterface<T>
    where T : ObliviousClass
{
}

public class ObliviousGenericClass<T>
	where T : ObliviousClass
{
	public ObliviousGenericClass(T instance)
	{
	}
}
