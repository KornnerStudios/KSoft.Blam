using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MegaloScriptProtoVariableSet
		: IO.ITagElementStringNameStreamable
	{
		public Dictionary<MegaloScriptVariableType, ListLimitTraits> Traits { get; private set; }

		public MegaloScriptProtoVariableSet()
		{
			Traits = new Dictionary<MegaloScriptVariableType, ListLimitTraits>();
		}

		#region ITagElementStringNameStreamable Members
		public static void StreamSetType<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string keyName, ref MegaloScriptVariableSet type)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnum(keyName, ref type);
		}

		static void StreamType<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			object ctxt, ref MegaloScriptVariableType type)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursorEnum(ref type);
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamableElements("Variable", Traits, (object)null, StreamType);
		}
		#endregion
	};
}