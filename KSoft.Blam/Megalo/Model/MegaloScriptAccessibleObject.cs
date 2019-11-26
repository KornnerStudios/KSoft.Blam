using System;
using ComponentModel = System.ComponentModel;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	#region IMegaloScriptAccessibleObject
	[System.Reflection.Obfuscation(Exclude=false)]
	[Contracts.ContractClass(typeof(IMegaloScriptAccessibleObjectContract))]
	public interface IMegaloScriptAccessibleObject
		: ComponentModel.INotifyPropertyChanged
	{
		string CodeName { get; set; }
	};
	[System.Reflection.Obfuscation(Exclude=false)]
	[Contracts.ContractClassFor(typeof(IMegaloScriptAccessibleObject))]
	abstract class IMegaloScriptAccessibleObjectContract
		: IMegaloScriptAccessibleObject
	{
		public string CodeName {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				throw new NotImplementedException();
			}
			set {
				Contract.Requires<ArgumentNullException>(value != null);
				throw new NotImplementedException();
			}
		}

#pragma warning disable 67
		public event ComponentModel.PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
	};
	#endregion

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("CodeName = {CodeName}")]
	public abstract partial class MegaloScriptAccessibleObjectBase
		: IMegaloScriptAccessibleObject
	{
		#region CodeName
		string mCodeName;
		public string CodeName {
			get { return mCodeName; }
			set { mCodeName = value;
				NotifyPropertyChanged(kCodeNameChanged);
		} }
		#endregion

		// TODO: DefaultCodeNameStringIndex

		protected MegaloScriptAccessibleObjectBase()
		{
			mCodeName = "";
		}

		#region ITagElementStringNameStreamable Members
		protected void SerializeCodeName<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt("name", ref mCodeName, Predicates.IsNotNullOrEmpty))
				mCodeName = "";
		}
		#endregion
	};
}