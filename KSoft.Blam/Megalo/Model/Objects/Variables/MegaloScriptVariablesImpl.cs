
namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModel
	{
		internal virtual MegaloScriptCustomVariable NewCustomVariable()
		{
			return new MegaloScriptCustomVariable();
		}
		internal virtual MegaloScriptTimerVariable NewTimerVariable()
		{
			return new MegaloScriptTimerVariable();
		}
		internal virtual MegaloScriptTeamVariable NewTeamVariable()
		{
			return new MegaloScriptTeamVariable();
		}
		internal virtual MegaloScriptPlayerVariable NewPlayerVariable()
		{
			return new MegaloScriptPlayerVariable();
		}
		internal virtual MegaloScriptObjectVariable NewObjectVariable()
		{
			return new MegaloScriptObjectVariable();
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public class MegaloScriptCustomVariable
		: MegaloScriptVariableWithVarReferenceBase
	{
		public MegaloScriptCustomVariable() : base(MegaloScriptVariableReferenceData.Custom)
		{
		}

		protected override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			mVar.SerializeCustom(model, s);
			SerializeNetworkState(s);
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public class MegaloScriptTimerVariable
		: MegaloScriptVariableWithVarReferenceBase
	{
		public override bool HasNetworkState { get { return false; } }

		public MegaloScriptTimerVariable() : base(MegaloScriptVariableReferenceData.Custom)
		{
		}

		protected override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			mVar.SerializeCustom(model, s);
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public partial class MegaloScriptTeamVariable
		: MegaloScriptVariableBase
	{
		#region TeamDesignator
		int mTeamDesignator;
		public int TeamDesignator {
			get { return mTeamDesignator; }
			set { mTeamDesignator = value;
				NotifyPropertyChanged(kTeamDesignatorChanged);
		} }
		#endregion

		protected override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			MegaloScriptEnumValue.SerializeValue(model, s, model.Database.TeamDesignatorValueType, ref mTeamDesignator);
			SerializeNetworkState(s);
		}
		protected override void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize<TDoc, TCursor>(model, s);

			MegaloScriptEnumValue.SerializeValue(model, s, model.Database.TeamDesignatorValueType, ref mTeamDesignator,
				IO.TagElementNodeType.Text);
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public class MegaloScriptPlayerVariable
		: MegaloScriptVariableBase
	{
		protected override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			SerializeNetworkState(s);
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public class MegaloScriptObjectVariable
		: MegaloScriptVariableBase
	{
		protected override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			SerializeNetworkState(s);
		}
	};
}