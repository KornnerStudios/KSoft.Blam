
namespace KSoft.Blam.Megalo.Model
{
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}", Name = "{Name}")]
	public sealed class MegaloScriptVirtualTrigger
		: MegaloScriptTriggerBase
	{
		public override MegaloScriptModelObjectType ObjectType { get { return MegaloScriptModelObjectType.VirtualTrigger; } }

		public MegaloScriptVirtualTrigger()
		{
		}
		// Only for future possible support for re-usable virtual triggers
		internal MegaloScriptVirtualTrigger(MegaloScriptConditionActionReferences refs) : base(refs)
		{
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			References.Serialize(model, s);
		}
		#endregion

		// #NOTE_BLAM: Can be embedded via MegaloScriptModelObjectHandle. Attributes must not conflict with that type's
		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			SerializeIdOpt(model, s);
			SerializeNameOpt(s);

			// We intentionally don't nest References in another element
			References.Serialize(model, s);
		}
		#endregion
	};
}