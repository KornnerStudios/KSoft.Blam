using System;
using ComponentModel = System.ComponentModel;

namespace KSoft.Blam.Megalo.Model
{
	#region IMegaloScriptAccessibleObject
	[System.Reflection.Obfuscation(Exclude=false)]
	public interface IMegaloScriptAccessibleObject
		: ComponentModel.INotifyPropertyChanged
	{
		string CodeName { get; set; }
	};
	#endregion

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("CodeName = {CodeName}")]
	public abstract partial class MegaloScriptAccessibleObjectBase
		: IMegaloScriptAccessibleObject
	{
		#region CodeName
		string mCodeName = string.Empty;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public string CodeName {
			get { return mCodeName; }
			set { ArgumentNullException.ThrowIfNull(value);
				mCodeName = value;
				NotifyPropertyChanged(kCodeNameChangedEventArgs);
		} }
		#endregion

		// #TODO_IMPLEMENT: DefaultCodeNameStringIndex

		#region ITagElementStringNameStreamable Members
		protected void SerializeCodeName<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			string? code_name = mCodeName;
			if (!s.StreamAttributeOpt("name", ref code_name, Predicates.IsNotNullOrEmpty))
			{
				code_name = "";
			}
			mCodeName = code_name ?? "";
		}
		#endregion
	};
}
