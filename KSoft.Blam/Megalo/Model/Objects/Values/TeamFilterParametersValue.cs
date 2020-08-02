#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	using Proto;

	using MegaloScriptPlayerFilterTypeBitStreamer = IO.EnumBitStreamer<MegaloScriptPlayerFilterType>;

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Type = {FilterType}")]
	public sealed partial class MegaloScriptTeamFilterParametersValue
		: MegaloScriptValueBase
	{
		#region FilterType
		MegaloScriptPlayerFilterType mFilterType;
		public MegaloScriptPlayerFilterType FilterType {
			get { return mFilterType; }
			set { mFilterType = value;
				NotifyPropertyChanged(kFilterTypeChanged);
		} }
		#endregion

		#region Player
		MegaloScriptVariableReferenceData mPlayer = MegaloScriptVariableReferenceData.Player;
		public MegaloScriptVariableReferenceData Player {
			get { return mPlayer; }
			set { mPlayer = value;
				NotifyPropertyChanged(kPlayerChanged);
		} }
		#endregion
		#region PlayerAddOrRemove (Bool)
		MegaloScriptVariableReferenceData mPlayerAddOrRemove = MegaloScriptVariableReferenceData.Custom;
		public MegaloScriptVariableReferenceData PlayerAddOrRemove {
			get { return mPlayerAddOrRemove; }
			set { mPlayerAddOrRemove = value;
				NotifyPropertyChanged(kPlayerAddOrRemoveChanged);
		} }
		#endregion

		public MegaloScriptTeamFilterParametersValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.TeamFilterParameters);
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptTeamFilterParametersValue)model.CreateValue(ValueType);
			result.FilterType = FilterType;
			result.Player = Player;
			result.PlayerAddOrRemove = PlayerAddOrRemove;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = (MegaloScriptTeamFilterParametersValue)other;

			return FilterType == obj.FilterType &&
				Player.Equals(obj.Player) && PlayerAddOrRemove.Equals(obj.PlayerAddOrRemove);
		}

		#region ChangeValue
		public void ChangeValue(MegaloScriptModel model,
			MegaloScriptVariableReferenceData playerData, MegaloScriptVariableReferenceData playerAddOrRemoveData)
		{
			Util.MarkUnusedVariable(ref model);

			FilterType = MegaloScriptPlayerFilterType.PlayerMask;
			Player = playerData;
			PlayerAddOrRemove = playerAddOrRemoveData;
		}
		public void ChangeValue(MegaloScriptModel model, MegaloScriptPlayerFilterType filterType)
		{
			Contract.Requires(filterType != MegaloScriptPlayerFilterType.PlayerMask, "Wrong ChangeValue overload");

			Util.MarkUnusedVariable(ref model);

			FilterType = filterType;
			Player = MegaloScriptVariableReferenceData.Player;
			PlayerAddOrRemove = MegaloScriptVariableReferenceData.Custom;
		}
		#endregion

		#region SetAs
		public void SetAsNoOne()
		{
			Player = MegaloScriptVariableReferenceData.Player;
			PlayerAddOrRemove = MegaloScriptVariableReferenceData.Custom;

			FilterType = MegaloScriptPlayerFilterType.NoOne;
		}
		public void SetAsEveryone()
		{
			Player = MegaloScriptVariableReferenceData.Player;
			PlayerAddOrRemove = MegaloScriptVariableReferenceData.Custom;

			FilterType = MegaloScriptPlayerFilterType.Everyone;
		}
		public void SetAsAlliesOfTeam()
		{
			Player = MegaloScriptVariableReferenceData.Player;
			PlayerAddOrRemove = MegaloScriptVariableReferenceData.Custom;

			FilterType = MegaloScriptPlayerFilterType.AlliesOfTeam;
		}
		public void SetAsEnemiesOfTeam()
		{
			Player = MegaloScriptVariableReferenceData.Player;
			PlayerAddOrRemove = MegaloScriptVariableReferenceData.Custom;

			FilterType = MegaloScriptPlayerFilterType.EnemiesOfTeam;
		}
		public void SetAsPlayerMask(MegaloScriptVariableReferenceData player, MegaloScriptVariableReferenceData addOrRemove)
		{
			Contract.Requires(player.ReferenceKind == MegaloScriptVariableReferenceType.Player);
			Contract.Requires(addOrRemove.ReferenceKind == MegaloScriptVariableReferenceType.Custom);

			Player = player;
			PlayerAddOrRemove = addOrRemove;

			FilterType = MegaloScriptPlayerFilterType.PlayerMask;
		}
		public void SetAsDefault()
		{
			Player = MegaloScriptVariableReferenceData.Player;
			PlayerAddOrRemove = MegaloScriptVariableReferenceData.Custom;

			FilterType = MegaloScriptPlayerFilterType.NoOne;
		}
		#endregion

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.Stream(ref mFilterType, 3, MegaloScriptPlayerFilterTypeBitStreamer.Instance);

			if(FilterType == MegaloScriptPlayerFilterType.PlayerMask)
			{
				mPlayer.SerializePlayer(model, s);
				mPlayerAddOrRemove.SerializeCustom(model, s);
			}
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeEnum("filterType", ref mFilterType);

			if(FilterType == MegaloScriptPlayerFilterType.PlayerMask)
			{
				using (s.EnterCursorBookmark("Player"))		mPlayer.SerializePlayer(model, s);
				using (s.EnterCursorBookmark("AddOrRemove"))mPlayerAddOrRemove.SerializeCustom(model, s);
			}
		}
		#endregion
	};
}
