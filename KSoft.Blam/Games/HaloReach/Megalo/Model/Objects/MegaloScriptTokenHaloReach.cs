
namespace KSoft.Blam.Games.HaloReach.Megalo.Model
{
	using MegaloModel = Blam.Megalo.Model;

	partial class MegaloScriptModelHaloReach
	{
		internal override MegaloModel.MegaloScriptToken NewToken()
		{
			return new MegaloScriptTokenHaloReach();
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	sealed class MegaloScriptTokenHaloReach
		: MegaloModel.MegaloScriptToken
	{
		#region IBitStreamSerializable Members
		protected override void SerializeType(MegaloModel.MegaloScriptModel model, IO.BitStream s,
			ref Blam.Megalo.MegaloScriptTokenAbstractType abstractType)
		{
			int type = (int)abstractType.ToHaloReach();
			s.StreamNoneable(ref type, 3);
			if (s.IsReading)
				abstractType = ((MegaloScriptTokenTypeHaloReach)type).ToAbstract();
		}
		#endregion
	};
}