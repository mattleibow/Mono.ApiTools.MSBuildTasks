using System;

namespace Mono.ApiTools.MSBuildTasks.Tests.TestAssembly
{
	// A top-level enum marked as an error-level obsolete. Because the enum is removed, the
	// auto-property below would strand a reference to it (through its compiler-generated backing
	// field) unless the property removal also cleans the backing field up — otherwise Cecil cannot
	// write the assembly back out ("declared in another module").
	//
	// Every member that consumes the enum is itself marked error-obsolete: using an
	// [Obsolete(error: true)] type from a non-obsolete member is a compile error, so a well-formed
	// assembly never references a removed-error type from a kept member.
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
