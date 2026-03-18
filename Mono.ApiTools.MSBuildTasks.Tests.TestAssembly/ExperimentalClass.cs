#pragma warning disable RS0041

using System.Diagnostics.CodeAnalysis;

namespace Mono.ApiTools.MSBuildTasks.Tests.TestAssembly;

[Experimental("TEST001")]
public class ExperimentalClass
{
	public int NormalField;

	public string ExperimentalMethod() => throw new NotImplementedException();

	public bool ExperimentalProperty { get; set; }

	public event EventHandler? ExperimentalEvent;
}

public class ClassWithExperimentalMembers
{
	[Experimental("TEST002")]
	public void ExperimentalMethod() => throw new NotImplementedException();

	public void NormalMethod() => throw new NotImplementedException();
}
