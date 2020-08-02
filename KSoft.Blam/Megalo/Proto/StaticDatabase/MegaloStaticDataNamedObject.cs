using System;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class MegaloStaticDataNamedObject
		: IMegaloStaticDataNamedObject
	{
		public const string kUndefinedPrefix = "UNDEFINED";
		protected static readonly Predicate<string> kIsAvailable =
			s => !string.IsNullOrEmpty(s) && !s.StartsWith(kUndefinedPrefix, StringComparison.Ordinal);

		public string Name;

		internal void UpdateForUndefined(string suffix, int index)
		{
			if (index.IsNone())
				Name = kUndefinedPrefix + "_" + suffix;
			else
			{
				Name = string.Format(Util.InvariantCultureInfo,
					kUndefinedPrefix + "_{0}_{1}",
					suffix, index.ToString(Util.InvariantCultureInfo));
			}
		}

		#region IMegaloStaticDataNamedObject Members
		string IMegaloStaticDataNamedObject.Name	{ get { return Name; } }
		#endregion

		#region ITagElementStringNameStreamable Members
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("name", ref Name);
		}
		#endregion
	};

	/// <summary>
	/// Availability exposed by having a non-empty Name
	/// </summary>
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class MegaloStaticDataNamedObjectWithAvailability
		: MegaloStaticDataNamedObject
	{
		public bool IsAvailable { get { return kIsAvailable(Name); } }

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOpt("name", ref Name, kIsAvailable);
		}
		#endregion
	};
}
