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
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mFilterType), AlwaysNotify = true)]
		public partial MegaloScriptPlayerFilterType FilterType { get; set; }
		#endregion

		#region Player
		MegaloScriptVariableReferenceData mPlayer = MegaloScriptVariableReferenceData.Player;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mPlayer), AlwaysNotify = true)]
		public partial MegaloScriptVariableReferenceData Player { get; set; }
		#endregion
		#region PlayerAddOrRemove (Bool)
		MegaloScriptVariableReferenceData mPlayerAddOrRemove = MegaloScriptVariableReferenceData.Custom;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mPlayerAddOrRemove), AlwaysNotify = true)]
		public partial MegaloScriptVariableReferenceData PlayerAddOrRemove { get; set; }
		#endregion

		public MegaloScriptTeamFilterParametersValue(MegaloScriptValueType valueType) : base(valueType)
		{
			ThrowIfUnexpectedBaseType(valueType, MegaloScriptValueBaseType.TeamFilterParameters);
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
			var obj = KSoft.Debug.TypeCheck.CastReference<MegaloScriptTeamFilterParametersValue>(other);

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
			if (filterType == MegaloScriptPlayerFilterType.PlayerMask)
			{
				throw new System.ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Filter type is {0}; use the player-mask ChangeValue overload.",
					filterType), nameof(filterType));
			}

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
			ThrowIfUnexpectedReferenceKind(player, MegaloScriptVariableReferenceType.Player, nameof(player));
			ThrowIfUnexpectedReferenceKind(addOrRemove, MegaloScriptVariableReferenceType.Custom, nameof(addOrRemove));

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

			if (FilterType == MegaloScriptPlayerFilterType.PlayerMask)
			{
				using (s.EnterCursorBookmark("Player"))		{ mPlayer.SerializePlayer(model, s); }
				using (s.EnterCursorBookmark("AddOrRemove")){ mPlayerAddOrRemove.SerializeCustom(model, s); }
			}
		}
		#endregion
	};
}
