
namespace KSoft.Blam.Megalo.Model
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract partial class MegaloScriptVariableWithVarReferenceBase
		: MegaloScriptVariableBase
	{
		#region Var
		protected MegaloScriptVariableReferenceData mVar;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mVar), AlwaysNotify = true)]
		public partial MegaloScriptVariableReferenceData Var { get; set; }
		#endregion

		protected MegaloScriptVariableWithVarReferenceBase(MegaloScriptVariableReferenceData var)
		{
			Var = var;
		}

		#region ITagElementStringNameStreamable Members
		protected override void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(model, s);

			mVar.Serialize(model, s, streamRefKind: false);
		}
		#endregion
	};
}