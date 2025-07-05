#pragma warning disable RS0041

namespace Mono.ApiTools.MSBuildTasks.Tests.TestAssembly;

public class AllTheThings
{
	internal int InternalField;

	public int IntField;

	public int? NullableIntField;

	public string StringField = "";

	public string? NullableStringField;

	public string StringReturn() => throw new NotImplementedException();

	public string? NullableStringReturn() => throw new NotImplementedException();

	public string? BasicMethodReturning(int param1, string param2) => throw new NotImplementedException();

#nullable disable

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

#nullable enable

	public void BasicMethodVoid(int param1, string param2) => throw new NotImplementedException();

	public string? WithReferencing(in int inParam, ref int refParam, out int outParam) => throw new NotImplementedException();

	public bool GetOnlyProperty { get; }

	public bool GetSetProperty { get; set; }

	public bool SetOnlyProperty { set => throw new NotImplementedException(); }

	public int this[int index] => throw new NotImplementedException();

	public int this[int index, string? name] => throw new NotImplementedException();

	public void MethodWithParams(params string[] strings) => throw new NotImplementedException();

	public void MethodWithParamsAndOptional(string? optional = null, params string[] strings) => throw new NotImplementedException();

	public void TestLocalDelegates()
	{
		Task.Run(() => throw new NotImplementedException());
	}
}

internal class InternalClass
{
	public int InternalField;
}

public record class RecordClass;

public record struct RecordStruct;

public enum EnumThing
{
	First,
	Second
}

public delegate string DelegateThing(string param1, int param2);
