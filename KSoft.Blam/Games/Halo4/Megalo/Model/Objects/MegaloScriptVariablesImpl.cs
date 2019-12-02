
namespace KSoft.Blam.Games.Halo4.Megalo.Model
{
	using BaseModel = Blam.Megalo.Model;

	partial class MegaloScriptModelHalo4
	{
		internal override BaseModel.MegaloScriptCustomVariable NewCustomVariable()
		{
			return new MegaloScriptCustomVariableHalo4();
		}
		internal override BaseModel.MegaloScriptTeamVariable NewTeamVariable()
		{
			return new MegaloScriptTeamVariableHalo4();
		}
		internal override BaseModel.MegaloScriptPlayerVariable NewPlayerVariable()
		{
			return new MegaloScriptPlayerVariableHalo4();
		}
		internal override BaseModel.MegaloScriptObjectVariable NewObjectVariable()
		{
			return new MegaloScriptObjectVariableHalo4();
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	sealed class MegaloScriptCustomVariableHalo4
		: BaseModel.MegaloScriptCustomVariable
	{
		public override bool SupportsUnknown { get { return true; } }

		protected override void Serialize(BaseModel.MegaloScriptModel model, IO.BitStream s)
		{
			base.Serialize(model, s);
			s.Stream(ref mUnknown);
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	sealed class MegaloScriptTeamVariableHalo4
		: BaseModel.MegaloScriptTeamVariable
	{
		public override bool SupportsUnknown { get { return true; } }

		protected override void Serialize(BaseModel.MegaloScriptModel model, IO.BitStream s)
		{
			base.Serialize(model, s);
			s.Stream(ref mUnknown);
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	sealed class MegaloScriptPlayerVariableHalo4
		: BaseModel.MegaloScriptPlayerVariable
	{
		public override bool SupportsUnknown { get { return true; } }

		protected override void Serialize(BaseModel.MegaloScriptModel model, IO.BitStream s)
		{
			base.Serialize(model, s);
			s.Stream(ref mUnknown);
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	sealed class MegaloScriptObjectVariableHalo4
		: BaseModel.MegaloScriptObjectVariable
	{
		public override bool SupportsUnknown { get { return true; } }

		protected override void Serialize(BaseModel.MegaloScriptModel model, IO.BitStream s)
		{
			base.Serialize(model, s);
			s.Stream(ref mUnknown);
		}
	};
}