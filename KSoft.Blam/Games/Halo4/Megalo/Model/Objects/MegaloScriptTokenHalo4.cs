
namespace KSoft.Blam.Games.Halo4.Megalo.Model
{
	using MegaloModel = Blam.Megalo.Model;

	partial class MegaloScriptModelHalo4
	{
		internal override MegaloModel.MegaloScriptToken NewToken()
		{
			return new MegaloScriptTokenHalo4();
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	sealed class MegaloScriptTokenHalo4
		: MegaloModel.MegaloScriptToken
	{
		#region IBitStreamSerializable Members
		protected override void SerializeType(MegaloModel.MegaloScriptModel model, IO.BitStream s,
			ref Blam.Megalo.MegaloScriptTokenAbstractType abstractType)
		{
			int type = (int)abstractType.ToHalo4();
			s.StreamNoneable(ref type, 3);
			if (s.IsReading)
				abstractType = ((MegaloScriptTokenTypeHalo4)type).ToAbstract();
		}
		#endregion
	};
}