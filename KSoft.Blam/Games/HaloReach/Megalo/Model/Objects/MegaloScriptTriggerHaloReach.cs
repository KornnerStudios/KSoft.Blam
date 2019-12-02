
namespace KSoft.Blam.Games.HaloReach.Megalo.Model
{
	using MegaloModel = Blam.Megalo.Model;

	partial class MegaloScriptModelHaloReach
	{
		protected override MegaloModel.MegaloScriptTrigger NewTrigger()
		{
			return new MegaloScriptTriggerHaloReach();
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	sealed class MegaloScriptTriggerHaloReach
		: MegaloModel.MegaloScriptTrigger
	{
		// 0x0 FirstCondition
		// 0x2 ConditionCount
		// 0x4 ExecutionMode
		// 0x5 TriggerType
		// 0x6 ObjectFilterIndex		sbyte
		// PAD8
		// 0x8 FirstAction
		// 0xA ActionCount

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(MegaloModel.MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(model, s);

			SerializeReferences(model, s);
		}
		#endregion
	};
}