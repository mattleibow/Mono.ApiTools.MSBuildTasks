namespace Mono.ApiTools.MSBuildTasks.Tests.TestAssembly;

public class AllTheThings
{
    public int IntField;

    public int? NullableIntField;

    public string StringField = "";

    public string? NullableStringField;

    public string StringReturn() => throw new NotImplementedException();

    public string? NullableStringReturn() => throw new NotImplementedException();

    public string? BasicMethodReturning(int param1, string param2) => throw new NotImplementedException();

    public void BasicMethodVoid(int param1, string param2) => throw new NotImplementedException();

    public string? WithReferencing(in int inParam, ref int refParam, out int outParam) => throw new NotImplementedException();

    public bool GetOnlyProperty { get; }

    public bool GetSetProperty { get; set; }

    public bool SetOnlyProperty { set => throw new NotImplementedException(); }

    public int this[int index] => throw new NotImplementedException();

    public int this[int index, string? name] => throw new NotImplementedException();

    public void MethodWithParams(params string[] strings) => throw new NotImplementedException();

    public void MethodWithParamsAndOptional(string? optional = null, params string[] strings) => throw new NotImplementedException();
}
