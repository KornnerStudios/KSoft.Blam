using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Proto
{
	using MegaloScriptModelTagElementStreamFlags = Model.MegaloScriptModelTagElementStreamFlags;

	// The order which params are encoded should be determined by the order they appear in the xml
	// SigId should be used for UI and such purposes
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {SigId}, Name = {Name}, Kind = {Kind}")]
	public sealed class MegaloScriptProtoParam
		: IO.ITagElementStringNameStreamable
	{
		public int SigId;
		public MegaloScriptValueType Type;
		public string Name;
		public MegaloScriptParamType Kind;
		public bool Optional;

		public MegaloScriptProtoParam()
		{
			Kind = MegaloScriptParamType.Input;
			Optional = false;
		}

		#region ITagElementStringNameStreamable Members
		const string kSigIdAttributeName = "sigID";
		const string kTypeAttributeName = "type";
		const string kNameAttirbuteName = "name";
		const string kKindAttributeName = "kind";

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var db = s.Owner as MegaloScriptDatabase;
			bool reading = s.IsReading;

			//string type_name = reading ? null : db.ValueTypeNames[Type.NameIndex];

			s.StreamAttributeOpt(kSigIdAttributeName, ref SigId, Predicates.IsNotZero);
			db.SerializeValueTypeReference(s, kTypeAttributeName, ref Type);
			if (!s.StreamAttributeOpt(kNameAttirbuteName, ref Name, Predicates.IsNotNullOrEmpty) &&
				 reading)
				Name = string.Format("Param{0}", SigId.ToString());
			s.StreamAttributeEnumOpt(kKindAttributeName, ref Kind);
			s.StreamAttributeOpt("optional", ref Optional, Predicates.IsTrue);
		}
		internal void WriteExtraModelInfo<TDoc, TCursor>(MegaloScriptDatabase db, IO.TagElementStream<TDoc, TCursor, string> s,
			bool multipleParameters, Model.MegaloScriptModelTagElementStreamFlags flags)
			where TDoc : class
			where TCursor : class
		{
			if ((flags & MegaloScriptModelTagElementStreamFlags.WriteParamKinds) != 0 && Kind > MegaloScriptParamType.Input)
				s.WriteAttributeEnum(kKindAttributeName, Kind);
			if ((flags & MegaloScriptModelTagElementStreamFlags.WriteParamSigIds) != 0 && multipleParameters)
				s.WriteAttribute(kSigIdAttributeName, SigId);
			if ((flags & MegaloScriptModelTagElementStreamFlags.WriteParamTypes) != 0)
				s.WriteAttribute(kTypeAttributeName, db.ValueTypeNames[Type.NameIndex]);
			if ((flags & MegaloScriptModelTagElementStreamFlags.WriteParamNames) != 0)
				s.WriteAttribute(kNameAttirbuteName, Name);
		}

		internal static KeyValuePair<int, string> SigIdNamePairFromStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			int key = TypeExtensions.kNone; string value = null;
			s.StreamAttribute(kSigIdAttributeName, ref key);
			s.StreamAttribute(kNameAttirbuteName, ref value);

			return new KeyValuePair<int, string>(key, value);
		}
		internal static void SigIdNamePairToStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			KeyValuePair<int, string> pair)
			where TDoc : class
			where TCursor : class
		{
			int key = pair.Key; string value = pair.Value;
			s.StreamAttribute(kSigIdAttributeName, ref key);
			s.StreamAttribute(kNameAttirbuteName, ref key);
		}
		#endregion
	};
}