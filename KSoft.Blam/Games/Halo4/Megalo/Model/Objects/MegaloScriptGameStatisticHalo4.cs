
namespace KSoft.Blam.Games.Halo4.Megalo.Model
{
	partial class MegaloScriptModelHalo4
	{
		protected override Blam.Megalo.Model.MegaloScriptGameStatistic NewGameStatistic()
		{
			return new MegaloScriptGameStatisticHalo4();
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	sealed class MegaloScriptGameStatisticHalo4
		: Blam.Megalo.Model.MegaloScriptGameStatistic
	{
		public override bool SupportsUnk5 { get { return true; } }
		public override bool SupportsIsScoreToWin { get { return true; } }
	};
}