
namespace KSoft.Blam.Megalo.Model
{
	using MegaloScriptVariableNetworkStateBitStreamer = IO.EnumBitStreamer<MegaloScriptVariableNetworkState>;

	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract partial class MegaloScriptVariableBase
		: MegaloScriptAccessibleObjectBase
		, IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		#region NetworkState
		MegaloScriptVariableNetworkState mNetworkState;
		public MegaloScriptVariableNetworkState NetworkState {
			get { return mNetworkState; }
			set { mNetworkState = value;
				NotifyPropertyChanged(kNetworkStateChanged);
		} }
		#endregion
		#region Unknown
		protected bool mUnknown;
		public bool Unknown {
			get { return mUnknown; }
			set { mUnknown = value;
				NotifyPropertyChanged(kUnknownChanged);
		} }
		#endregion

		public virtual bool HasNetworkState { get { return true; } }
		public virtual bool SupportsUnknown { get { return false; } }

		protected MegaloScriptVariableBase()
		{
		}

		#region IBitStreamSerializable Members
		protected void SerializeNetworkState(IO.BitStream s)
		{
			s.Stream(ref mNetworkState, 2, MegaloScriptVariableNetworkStateBitStreamer.Instance);
		}
		protected abstract void Serialize(MegaloScriptModel model, IO.BitStream s);

		void IO.IBitStreamSerializable.Serialize(IO.BitStream s)
		{
			Serialize((MegaloScriptModel)s.Owner, s);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		protected virtual void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (HasNetworkState)
				s.StreamAttributeEnumOpt("networkState", ref mNetworkState, e => e != MegaloScriptVariableNetworkState.None);

			SerializeCodeName(s);

			if (SupportsUnknown)
				s.StreamAttributeOpt("unknown", ref mUnknown, Predicates.IsTrue);
		}

		void IO.ITagElementStreamable<string>.Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			Serialize((MegaloScriptModel)s.Owner, s);
		}
		#endregion
	};
}