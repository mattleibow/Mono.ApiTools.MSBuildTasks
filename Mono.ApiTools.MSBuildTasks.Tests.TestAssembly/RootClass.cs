namespace Mono.ApiTools.MSBuildTasks.Tests.TestAssembly
{
	public class RootClass
	{
		// methods

		public void NormalMethod()
		{
		}

		[Obsolete("BAD")]
		public void ObsoleteMethod()
		{
		}

		[Obsolete("BAD", true)]
		public void ObsoleteErrorMethod()
		{
		}

		// properties

		public bool NormalProperty { get; set; }

		[Obsolete("BAD")]
		public bool ObsoleteProperty { get; set; }

		[Obsolete("BAD", true)]
		public bool ObsoleteErrorProperty { get; set; }

		// fields

		public bool NormalField;

		[Obsolete("BAD")]
		public bool ObsoleteField;

		[Obsolete("BAD", true)]
		public bool ObsoleteErrorField;

		// events

		public event EventHandler? NormalEvent;

		[Obsolete("BAD")]
		public event EventHandler? ObsoleteEvent;

		[Obsolete("BAD", true)]
		public event EventHandler? ObsoleteErrorEvent;

		public class NestedClass
		{
			// methods

			public void NormalMethod()
			{
			}

			[Obsolete("BAD")]
			public void ObsoleteMethod()
			{
			}

			[Obsolete("BAD", true)]
			public void ObsoleteErrorMethod()
			{
			}

			// properties

			public bool NormalProperty { get; set; }

			[Obsolete("BAD")]
			public bool ObsoleteProperty { get; set; }

			[Obsolete("BAD", true)]
			public bool ObsoleteErrorProperty { get; set; }

			// fields

			public bool NormalField;

			[Obsolete("BAD")]
			public bool ObsoleteField;

			[Obsolete("BAD", true)]
			public bool ObsoleteErrorField;

			// events

			public event EventHandler? NormalEvent;

			[Obsolete("BAD")]
			public event EventHandler? ObsoleteEvent;

			[Obsolete("BAD", true)]
			public event EventHandler? ObsoleteErrorEvent;

			public class NestedNestedClass
			{
				// methods

				public void NormalMethod()
				{
				}

				[Obsolete("BAD")]
				public void ObsoleteMethod()
				{
				}

				[Obsolete("BAD", true)]
				public void ObsoleteErrorMethod()
				{
				}

				// properties

				public bool NormalProperty { get; set; }

				[Obsolete("BAD")]
				public bool ObsoleteProperty { get; set; }

				[Obsolete("BAD", true)]
				public bool ObsoleteErrorProperty { get; set; }

				// fields

				public bool NormalField;

				[Obsolete("BAD")]
				public bool ObsoleteField;

				[Obsolete("BAD", true)]
				public bool ObsoleteErrorField;

				// events

				public event EventHandler? NormalEvent;

				[Obsolete("BAD")]
				public event EventHandler? ObsoleteEvent;

				[Obsolete("BAD", true)]
				public event EventHandler? ObsoleteErrorEvent;
			}
		}
	}
}
