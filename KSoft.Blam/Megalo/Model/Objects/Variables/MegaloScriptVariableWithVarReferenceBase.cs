
namespace KSoft.Blam.Megalo.Model
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract partial class MegaloScriptVariableWithVarReferenceBase
		: MegaloScriptVariableBase
	{
		#region Var
		protected MegaloScriptVariableReferenceData mVar;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public MegaloScriptVariableReferenceData Var {
			get { return mVar; }
			set { mVar = value;
				NotifyPropertyChanged(kVarChangedEventArgs);
		} }
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