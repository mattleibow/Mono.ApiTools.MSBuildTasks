using System;

namespace Mono.ApiTools.MSBuildTasks.Tests.TestAssembly
{
	// An error-level [Obsolete] enum together with a consumer whose obsolete members use it.
	// When those obsolete members are removed, their compiler-generated artifacts (the property's
	// accessors and backing field) are removed alongside them, so nothing is left behind that still
	// mentions the enum and the enum itself can be removed too.
	//
	// Every consuming member is itself error-obsolete on purpose: C# forbids a non-obsolete member
	// from using an [Obsolete(error: true)] type, so a real assembly never has a kept member that
	// depends on a removed error-obsolete type.
	//
	// These are intentionally internal: they exercise removal behaviour only and are not part of the
	// public API surface validated by the PublicAPI analyzer.
	[Obsolete("BAD", true)]
	internal enum ObsoleteErrorEnum
	{
		None,
		First,
		Second,
	}

	internal class EnumConsumer
	{
		[Obsolete("BAD", true)]
		public ObsoleteErrorEnum ErrorProperty { get; set; }

		[Obsolete("BAD", true)]
		public ObsoleteErrorEnum ErrorMethod(ObsoleteErrorEnum value) => value;

		[Obsolete("BAD", true)]
		public ObsoleteErrorEnum ErrorField;

		public int KeptProperty { get; set; }
	}
}
