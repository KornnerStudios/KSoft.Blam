using System;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class MegaloStaticDataNamedObject
		: IMegaloStaticDataNamedObject
	{
		public const string kUndefinedPrefix = "UNDEFINED";
		protected static readonly Predicate<string> kIsAvailable =
			s => !string.IsNullOrEmpty(s) && !s.StartsWith(kUndefinedPrefix);

		public string Name;

		internal void UpdateForUndefined(string suffix, int index)
		{
			if (index.IsNone())
				Name = kUndefinedPrefix + "_" + suffix;
			else
				Name = string.Format(kUndefinedPrefix + "_{0}_{1}", suffix, index.ToString());
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
}