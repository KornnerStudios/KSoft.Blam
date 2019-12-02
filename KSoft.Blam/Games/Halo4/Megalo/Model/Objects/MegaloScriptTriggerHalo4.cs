
namespace KSoft.Blam.Games.Halo4.Megalo.Model
{
	using MegaloModel = Blam.Megalo.Model;

	partial class MegaloScriptModelHalo4
	{
		protected override MegaloModel.MegaloScriptTrigger NewTrigger()
		{
			return new MegaloScriptTriggerHalo4();
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	sealed class MegaloScriptTriggerHalo4
		: MegaloModel.MegaloScriptTrigger
	{
		// 0x0 FirstCondition
		// 0x2 ConditionCount
		// 0x4 FirstAction
		// 0x6 ActionCount
		// 0x8 ExecutionMode
		// 0x9 TriggerType
		// 0xA ObjectFilterIndex		sbyte
		// 0xB GameObjectFilterIndex	sbyte
		// 0xC GameObjectType
		// 0xD FrameUpdateFrequency		sbyte
		// 0xE FrameUpdateOffset		sbyte

		#region IBitStreamSerializable Members
		protected override int kTypeBitLength { get { return 4; } }

		public override void Serialize(MegaloModel.MegaloScriptModel model, IO.BitStream s)
		{
			base.Serialize(model, s);

			SerializeFrameUpdate(model, s);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(MegaloModel.MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(model, s);

			SerializeFrameUpdate(s);

			SerializeReferences(model, s);
		}
		#endregion
	};
}