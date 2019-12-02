
namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("Index = {TypeIndex}, Name = {Name}")]
	public sealed class MegaloScriptProtoVariableReferenceMember
		: IO.ITagElementStringNameStreamable
	{
		public int TypeIndex { get; internal set; }

		/// <summary>Not strictly engine based; our own markup for the reference type's value</summary>
		public MegaloScriptProtoVariableDataType Type;
		public string Name;
		public MegaloScriptProtoVariableReferenceMemberFlags Flags;
		/// <summary>The 'data type' information</summary>
		public MegaloScriptValueType EnumValueType;
		/// <summary>How we should interpret 'data'</summary>
		public MegaloScriptValueType ValueType;
		/// <summary>What we should name 'data' (instead of just presenting it as 'data')</summary>
		public string ValueName;

		public bool IsReadOnly	{ get { return (Flags & MegaloScriptProtoVariableReferenceMemberFlags.Readonly) != 0; } }
		public bool HasDataType { get { return (Flags & MegaloScriptProtoVariableReferenceMemberFlags.HasDataType) != 0; } }
		public bool HasDataValue{ get { return (Flags & MegaloScriptProtoVariableReferenceMemberFlags.HasDataValue) != 0; } }

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var db = s.Owner as MegaloScriptDatabase;
			bool reading = s.IsReading;

			s.StreamAttributeEnum("type", ref Type);
			s.StreamAttribute("name", ref Name);

			bool read_only = IsReadOnly;
			s.StreamAttributeOpt("readonly", ref read_only, Predicates.IsTrue);

			if (db.SerializeValueTypeReference(s, "paramTypeEnum", ref EnumValueType, isOptional:true))
				Flags |= MegaloScriptProtoVariableReferenceMemberFlags.HasDataType;

			if (db.SerializeValueTypeReference(s, "paramValueType", ref ValueType, isOptional:true))
			{
				Flags |= MegaloScriptProtoVariableReferenceMemberFlags.HasDataValue;
				s.StreamAttribute("paramValueName", ref ValueName);
			}

			if (reading)
			{
				if(read_only)
					Flags |= MegaloScriptProtoVariableReferenceMemberFlags.Readonly;
			}
		}
		#endregion
	};
}