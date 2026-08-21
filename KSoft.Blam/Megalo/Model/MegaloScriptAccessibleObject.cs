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
		public string CodeName {
			get { return mCodeName; }
			set { ArgumentNullException.ThrowIfNull(value);
				mCodeName = value;
				NotifyPropertyChanged(kCodeNameChanged);
		} }
		#endregion

		// #TODO_IMPLEMENT: DefaultCodeNameStringIndex

		#region ITagElementStringNameStreamable Members
		protected void SerializeCodeName<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt("name", ref mCodeName, Predicates.IsNotNullOrEmpty))
			{
				mCodeName = "";
			}
		}
		#endregion
	};
}
