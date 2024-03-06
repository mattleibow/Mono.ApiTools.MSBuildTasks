namespace Mono.ApiTools.MSBuildTasks.Tests.TestAssembly
{
	[Obsolete("BAD", true)]
	public class ObsoleteErrorRootClass
	{
		[Obsolete("BAD", true)]
		public void ObsoleteErrorMethod()
		{
		}

		[Obsolete("BAD", true)]
		public bool ObsoleteErrorProperty { get; set; }

		[Obsolete("BAD", true)]
		public bool ObsoleteErrorField;

		[Obsolete("BAD", true)]
		public event EventHandler? ObsoleteErrorEvent;
	}
}
