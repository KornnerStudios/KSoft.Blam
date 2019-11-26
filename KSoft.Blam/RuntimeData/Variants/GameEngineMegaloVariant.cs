using System;
using System.Collections.Generic;

namespace KSoft.Blam.RuntimeData.Variants
{
	using LocaleStringTableInfo = Localization.StringTables.LocaleStringTableInfo;
	using LocaleStringTable = Localization.StringTables.LocaleStringTable;

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum GameEngineMegaloVariantTagElementStreamFlags
	{
		UseStringTableNames = 1<<0,
		UseUserOptionNames = 1<<1,
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract partial class GameEngineMegaloVariant
		: IGameEngineVariant
	{
		GameEngineType IGameEngineVariant.EngineType { get { return GameEngineType.Megalo; } }

		protected int mEncodingVersion;
		public int EngineVersion;
		public abstract GameEngineBaseVariant BaseVariant { get; }
		public List<MegaloVariantPlayerTraitsBase> PlayerTraits { get; private set; }
		public List<MegaloVariantUserDefinedOption> UserDefinedOptions { get; private set; }
		public LocaleStringTable StringTable { get; private set; }
		public int BaseNameStringIndex;
		public LocaleStringTable NameString { get; private set; }
		public LocaleStringTable DescriptionString { get; private set; }
		public LocaleStringTable CategoryString { get; private set; }
		public int EngineIconIndex, EngineCategory;
		public MegaloVariantMapPermissions MapPermissions { get; private set; }
		public MegaloVariantPlayerRatingParameters PlayerRatingParameters { get; private set; }
		public short ScoreToWinRound;

		#region Option toggles
		public Collections.BitSet DisabledEngineOptions { get; private set; }
		public Collections.BitSet HiddenEngineOptions { get; private set; }
		public Collections.BitSet DisabledUserOptions { get; private set; }
		public Collections.BitSet HiddenUserOptions { get; private set; }
		bool HasEngineOptionToggles { get {
			return DisabledEngineOptions.Cardinality > 0 || HiddenEngineOptions.Cardinality > 0;
		} }
		bool HasUserOptionToggles { get {
			return DisabledUserOptions.Cardinality > 0 || HiddenUserOptions.Cardinality > 0;
		} }
		#endregion

//		public Megalo.Model.MegaloScriptModel EngineDefinition { get; private set; }
//		public Megalo.Proto.MegaloScriptDatabase MegaloDatabase { get { return EngineDefinition.Database; } }

		bool StringTableIsNotDefault { get {
			return BaseNameStringIndex.IsNotNone() || StringTable.HasStrings;
		} }
		void StringTableRevertToDefault()
		{
			BaseNameStringIndex = TypeExtensions.kNone;
			StringTable.Clear();
		}
#if false // TODO
		protected GameEngineMegaloVariant(EngineGame game,
			LocaleStringTableInfo stringTableInfo,
			LocaleStringTableInfo nameStringInfo, LocaleStringTableInfo descriptionStringInfo,
			LocaleStringTableInfo categoryStringInfo)
		{
			EngineDefinition = Megalo.Model.MegaloScriptModel.Create(game, this);

			PlayerTraits = new List<MegaloVariantPlayerTraitsBase>(MegaloDatabase.Limits.PlayerTraits.MaxCount);
			UserDefinedOptions = new List<MegaloVariantUserDefinedOption>(MegaloDatabase.Limits.UserDefinedOptions.MaxCount);
			StringTable = new LocaleStringTable(stringTableInfo);
			BaseNameStringIndex = TypeExtensions.kNone;
			NameString = new LocaleStringTable(nameStringInfo);
			DescriptionString = new LocaleStringTable(descriptionStringInfo);
			CategoryString = new LocaleStringTable(categoryStringInfo);
			EngineIconIndex = EngineCategory = TypeExtensions.kNone;
			MapPermissions = new MegaloVariantMapPermissions();
			PlayerRatingParameters = new MegaloVariantPlayerRatingParameters();

			DisabledEngineOptions = new Collections.BitSet(MegaloDatabase.Limits.GameEngineOptions.MaxCount, fixedLength: true);
			HiddenEngineOptions = new Collections.BitSet(MegaloDatabase.Limits.GameEngineOptions.MaxCount, fixedLength: true);
			DisabledUserOptions = new Collections.BitSet(MegaloDatabase.Limits.UserDefinedOptions.MaxCount, fixedLength: true);
			HiddenUserOptions = new Collections.BitSet(MegaloDatabase.Limits.UserDefinedOptions.MaxCount, fixedLength: true);
		}
#endif
		internal static GameEngineMegaloVariant Create(Engine.EngineBuildHandle gameBuild)
		{
#if false // TODO
			if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new Games.HaloReach.RuntimeData.Variants.GameEngineMegaloVariantHaloReach();
			else if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new Games.Halo4.RuntimeData.Variants.GameEngineMegaloVariantHalo4();
			else
#endif
				throw new KSoft.Debug.UnreachableException(gameBuild.ToDisplayString());
		}

		public virtual void ClearWeaponTunings() { }

		protected abstract bool VerifyEncodingVersion();

		#region IBitStreamSerializable Members
#if false // TODO
		internal void StreamStringTableIndexPointer(IO.BitStream s,		ref int stringIndex)
		{ s.StreamNoneable(ref stringIndex,	EngineDefinition.Database.Limits.VariantStrings.IndexBitLength); }
		internal void StreamStringTableIndexReference(IO.BitStream s,	ref int stringIndex)
		{ s.Stream(ref stringIndex,			EngineDefinition.Database.Limits.VariantStrings.IndexBitLength); }
		internal void StreamUserDefinedValuesCount(IO.BitStream s,		ref int count)
		{ s.Stream(ref count,				EngineDefinition.Database.Limits.UserDefinedOptionValues.CountBitLength); }
		internal void StreamUserDefinedValueIndex(IO.BitStream s,		ref int valueIndex)
		{ s.Stream(ref valueIndex,			EngineDefinition.Database.Limits.UserDefinedOptionValues.IndexBitLength); }
#else
		internal void StreamStringTableIndexPointer(IO.BitStream s,		ref int stringIndex)
		{  }
		internal void StreamStringTableIndexReference(IO.BitStream s,	ref int stringIndex)
		{  }
		internal void StreamUserDefinedValuesCount(IO.BitStream s,		ref int count)
		{  }
		internal void StreamUserDefinedValueIndex(IO.BitStream s,		ref int valueIndex)
		{  }
#endif
		protected void SerializeOptionToggles(IO.BitStream s)
		{
			DisabledEngineOptions.SerializeWords(s, Shell.EndianFormat.Little);
			HiddenEngineOptions.SerializeWords(s, Shell.EndianFormat.Little);
			DisabledUserOptions.SerializeWords(s, Shell.EndianFormat.Little);
			HiddenUserOptions.SerializeWords(s, Shell.EndianFormat.Little);
		}
		protected abstract void SerializeDescriptionLocaleStrings(IO.BitStream s);
		protected abstract void SerializeImpl(IO.BitStream s);

		public void Serialize(IO.BitStream s)
		{
			using (s.EnterOwnerBookmark(this))
			{
				s.Stream(ref mEncodingVersion);
				if (s.IsReading && !VerifyEncodingVersion())
					throw new IO.VersionMismatchException("Megalo encoding", (uint)mEncodingVersion);
#if false // TODO
				s.Stream(ref EngineVersion);				// global, not a c_game_engine_megalo_variant member
				s.StreamObject(BaseVariant);
				s.StreamElements(PlayerTraits,
					MegaloDatabase.Limits.PlayerTraits.CountBitLength,
					this, _this => _this.NewMegaloPlayerTraits());
				s.StreamElements(UserDefinedOptions,
					MegaloDatabase.Limits.UserDefinedOptions.CountBitLength);
				s.StreamObject(StringTable);
				StreamStringTableIndexPointer(s, ref BaseNameStringIndex);
				SerializeDescriptionLocaleStrings(s);
				s.StreamEnum(ref EngineIconIndex,
					MegaloDatabase.Limits.EngineCategories.IndexBitLength);
				s.StreamEnum(ref EngineCategory,
					MegaloDatabase.Limits.EngineCategories.IndexBitLength);
				s.StreamObject(MapPermissions);
				s.StreamObject(PlayerRatingParameters);
				s.Stream(ref ScoreToWinRound);
#endif
				SerializeImpl(s);
			}
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public GameEngineMegaloVariantTagElementStreamFlags TagElementStreamSerializeFlags;

		#region SerializeStringTableIndex
		static readonly Func<GameEngineMegaloVariant, string, int> StringTableEntryIdResolver =
			(ctxt, name) => name != null
				? ctxt.StringTable.FindNameIndex(name)
				: TypeExtensions.kNone;
		static readonly Func<GameEngineMegaloVariant, int, string> StringTableEntryNameResolver =
			(ctxt, id) => id.IsNotNone()
				? ctxt.StringTable[id].CodeName
				: null;
		internal void SerializeStringTableIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref int stringIndex)
			where TDoc : class
			where TCursor : class
		{
			if ((TagElementStreamSerializeFlags & GameEngineMegaloVariantTagElementStreamFlags.UseStringTableNames) != 0)
			{
				s.StreamAttributeIdAsString(attributeName, ref stringIndex,
					this, StringTableEntryIdResolver, StringTableEntryNameResolver);
			}
			else
				s.StreamAttribute(attributeName, ref stringIndex);
		}
		internal void SerializeStringTableIndexOpt<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref int stringIndex)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = false;

			if ((TagElementStreamSerializeFlags & GameEngineMegaloVariantTagElementStreamFlags.UseStringTableNames) != 0)
			{
				streamed = s.StreamAttributeOptIdAsString(attributeName, ref stringIndex,
					this, StringTableEntryIdResolver, StringTableEntryNameResolver, Predicates.IsNotNull);
			}
			else
			{
				streamed = s.StreamAttributeOpt(attributeName, ref stringIndex, Predicates.IsNotNone);
			}

			if (!streamed)
				stringIndex = TypeExtensions.kNone;
		}
		#endregion

		#region SerializeOptionToggles
		static int SerializeEngineOptionToggle<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BitSet bitset, int bitIndex, GameEngineMegaloVariant variant)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref bitIndex);

			return bitIndex;
		}
		static int SerializeUserOptionToggle<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BitSet bitset, int bitIndex, GameEngineMegaloVariant variant)
			where TDoc : class
			where TCursor : class
		{
#if false // TODO
			if (!variant.TagElementStreamSerializeFlags.UseUserOptionNames())
				s.StreamCursor(ref bitIndex);
			else if (s.IsReading)
			{
				string option_name = null;
				s.ReadCursor(ref option_name);
				bitIndex = variant.EngineDefinition.FromIndexName(
					Megalo.Proto.MegaloScriptValueIndexTarget.Option, option_name);
			}
			else if (s.IsWriting)
			{
				string option_name = variant.EngineDefinition.ToIndexName(
					Megalo.Proto.MegaloScriptValueIndexTarget.Option, bitIndex);
				s.WriteCursor(option_name);
			}
#endif
			return bitIndex;
		}
		void SerializeOptionToggles<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			#region EngineOptions
			using(var bm = s.EnterCursorBookmarkOpt("EngineOptions", this, _obj=>_obj.HasEngineOptionToggles)) if (bm.IsNotNull)
			{
				using (var bm2 = s.EnterCursorBookmarkOpt("Disabled", DisabledEngineOptions, Predicates.HasBits)) if (bm2.IsNotNull)
					DisabledEngineOptions.Serialize(s, "Option", this, SerializeEngineOptionToggle);
				using (var bm2 = s.EnterCursorBookmarkOpt("Hidden", HiddenEngineOptions, Predicates.HasBits)) if (bm2.IsNotNull)
					HiddenEngineOptions.Serialize(s, "Option", this, SerializeEngineOptionToggle);
			}
			#endregion
			#region UserOptions
			using(var bm = s.EnterCursorBookmarkOpt("UserOptions", this, _obj => _obj.HasUserOptionToggles)) if (bm.IsNotNull)
			{
				using (var bm2 = s.EnterCursorBookmarkOpt("Disabled", DisabledUserOptions, Predicates.HasBits)) if (bm2.IsNotNull)
					DisabledUserOptions.Serialize(s, "Option", this, SerializeUserOptionToggle);
				using (var bm2 = s.EnterCursorBookmarkOpt("Hidden", HiddenUserOptions, Predicates.HasBits)) if (bm2.IsNotNull)
					HiddenUserOptions.Serialize(s, "Option", this, SerializeUserOptionToggle);
			}
			#endregion
		}
		#endregion

		protected virtual void SerializeLocaleStrings<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("StringTable", this, _obj=>_obj.StringTableIsNotDefault)) if (bm.IsNotNull)
			{
				s.StreamObject(StringTable);

				SerializeStringTableIndexOpt(s, "baseNameIndex", ref BaseNameStringIndex);
			}
			else
				StringTableRevertToDefault();

			using (var bm = s.EnterCursorBookmarkOpt("NameString", NameString, Predicates.HasItems)) if (bm.IsNotNull)
				s.StreamObject(NameString);
			using (var bm = s.EnterCursorBookmarkOpt("DescString", DescriptionString, Predicates.HasItems)) if (bm.IsNotNull)
				s.StreamObject(DescriptionString);
			using (var bm = s.EnterCursorBookmarkOpt("CategoryString", CategoryString, Predicates.HasItems)) if (bm.IsNotNull)
				s.StreamObject(CategoryString);
		}
		protected virtual void SerializeImpl<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("SerializeFlags"))
				s.StreamCursorEnum(ref TagElementStreamSerializeFlags, true);

			s.StreamAttribute("encoding", ref mEncodingVersion, NumeralBase.Hex);
			s.StreamAttribute("version", ref EngineVersion, NumeralBase.Hex);
			// Must come first. Most of the other variant data contains string references
			SerializeLocaleStrings(s);

			using (var bm = s.EnterCursorBookmarkOpt("PlayerTraits", PlayerTraits, Predicates.HasItems)) if (bm.IsNotNull)
				s.StreamableElements("entry", PlayerTraits, this, _this => _this.NewMegaloPlayerTraits());
			using (var bm = s.EnterCursorBookmarkOpt("UserDefinedOptions", UserDefinedOptions, Predicates.HasItems)) if (bm.IsNotNull)
				s.StreamableElements("entry", UserDefinedOptions);

			if (!s.StreamAttributeOpt("engineIcon", ref EngineIconIndex, Predicates.IsNotNone))
				EngineIconIndex = TypeExtensions.kNone;
			if (!s.StreamAttributeOpt("engineCategory", ref EngineCategory, Predicates.IsNotNone))
				EngineCategory = TypeExtensions.kNone;

			using (var bm = s.EnterCursorBookmarkOpt("MapPermissions", MapPermissions, mp=>!mp.IsDefault)) if (bm.IsNotNull)
				s.StreamObject(MapPermissions);
			using (s.EnterCursorBookmark("PlayerRatingParams"))
				s.StreamObject(PlayerRatingParameters);
			s.StreamAttributeOpt("scoreToWinRound", ref ScoreToWinRound, Predicates.IsNotZero);

			SerializeOptionToggles(s);
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterOwnerBookmark(this))
			{
				using (s.EnterCursorBookmark("Base"))
					s.StreamObject(BaseVariant);

				using (s.EnterCursorBookmark("Megalo"))
					SerializeImpl(s);

				if (s.IsWriting && s.IgnoreWritePredicates) // HACK: IgnoreWritePredicates hack!
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine();
					Console.WriteLine("Always write 'default' data HACK: Skipping MegaloScript element (output will be incomplete)");
					Console.ResetColor();
					return;
				}
#if false // TODO
				using (s.EnterCursorBookmark("MegaloScript"))
					s.StreamObject(EngineDefinition);
#endif
			}
		}
		#endregion
	};
}