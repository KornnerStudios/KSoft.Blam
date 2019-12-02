using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MegaloScriptProtoVariableReference
		: IO.ITagElementStringNameStreamable
	{
		ListLimitTraits TypeTraits = ListLimitTraits.Null;
		/// <summary>The reference types for this variable</summary>
		public List<MegaloScriptProtoVariableReferenceMember> Members { get; private set; }
		public Dictionary<string, MegaloScriptProtoVariableReferenceMember> NameToMember { get; private set; }

		public int TypeBitLength { get { return TypeTraits.IndexBitLength; } }

		public MegaloScriptProtoVariableReference()
		{
			Members = new List<MegaloScriptProtoVariableReferenceMember>();
			NameToMember = new Dictionary<string, MegaloScriptProtoVariableReferenceMember>();
		}

		#region ITagElementStringNameStreamable Members
		public static void StreamType<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string keyName, ref MegaloScriptVariableReferenceType type)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnum(keyName, ref type);
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamableElements("Member", Members);

			if (s.IsReading)
			{
				TypeTraits.MaxCount = Members.Count;
				TypeTraits.InitializeBitLengths();

				int index = 0;
				foreach (var member in Members)
				{
					member.TypeIndex = index++;
					NameToMember.Add(member.Name, member);
				}
			}
		}
		#endregion
	};
}