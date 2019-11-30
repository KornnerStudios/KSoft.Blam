using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.RuntimeData
{
	using ContentTypeBitStreamer = IO.EnumBitStreamer
		< ContentType
		, Int32
		, IO.EnumBitStreamerOptions.ShouldUseNoneSentinelEncoding
		>;
	using GameModeBitStreamer = IO.EnumBitStreamer<GameMode>;
	using GameEngineTypeBitStreamer = IO.EnumBitStreamer<GameEngineType>;

	using ContentTypeBinaryStreamer = IO.EnumBinaryStreamer<ContentType>;
	using GameModeBinaryStreamer = IO.EnumBinaryStreamer<GameMode>;
	using GameEngineTypeBinaryStreamer = IO.EnumBinaryStreamer<GameEngineType>;

	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class ContentHeader
		: IO.IBitStreamSerializable
		, IO.IEndianStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		const int kStringLength = 128;
		static readonly Memory.Strings.StringStorage kStringStorage = new Memory.Strings.StringStorage(
			Memory.Strings.StringStorageWidthType.Unicode, Memory.Strings.StringStorageType.CString, Shell.EndianFormat.Big, kStringLength);
		static readonly Text.StringStorageEncoding kStringEncoding = new Text.StringStorageEncoding(
			kStringStorage);

		public Engine.EngineBuildHandle GameBuild { get; private set; }

		public ContentType Type;
		public int FileLength; // total length of the file. in BLF cases, this would be the position following the _eof chunk payload
		public ulong unk08, unk10, unk18, unk20; // one of these are: owner id, share id, server file id
		public sbyte Activity;
		public GameMode Mode;
		public GameEngineType EngineType;
		protected int unk2C = TypeExtensions.kNone;	// ContentMiniMetadata.unk4?
		public int EngineCategoryIndex; // sbyte at runtime
		public ContentAuthor Creator { get; private set; }
		public ContentAuthor Modifier { get; private set; }
		public string Title, Description;
		protected int unk280 = TypeExtensions.kNone;
		public short HopperId = TypeExtensions.kNone;
		public sbyte CampaignId = TypeExtensions.kNone; // pretty sure this is actually unsigned, but we use -1 to know when we're touching it in non-campaign cases
		public GameDifficulty DifficultyLevel;
		public MetagameScoring GameScoring;
		public sbyte InsertionPoint = TypeExtensions.kNone; // pretty sure this is actually unsigned
		protected int unk2A4 = TypeExtensions.kNone;

		public int EngineIconIndex {
			get {
				Contract.Requires(Type == ContentType.GameVariant);
				return unk280;
			}
			set {
				Contract.Requires(Type == ContentType.GameVariant);
				unk280 = value;
			}
		}

		protected ContentHeader(Engine.EngineBuildHandle gameBuild)
		{
			GameBuild = gameBuild;
			Creator = new ContentAuthor();
			Modifier = new ContentAuthor();
		}

		internal static ContentHeader Create(Engine.EngineBuildHandle gameBuild)
		{
#if false // #TODO_BLAM_REFACTOR
			if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new Games.HaloReach.RuntimeData.ContentHeaderHaloReach();
			else if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new Games.Halo4.RuntimeData.ContentHeaderHalo4();
			else
#endif
			{
				throw new KSoft.Debug.UnreachableException(gameBuild.ToDisplayString());
			}
		}

		#region IBitStreamSerializable Members
		protected abstract void SerializeActivity(IO.BitStream s);
		protected abstract void SerializeGameSpecificData(IO.BitStream s);

		public void Serialize(IO.BitStream s)
		{
			s.Stream(ref Type, 4, ContentTypeBitStreamer.Instance);
			s.Stream(ref FileLength);
			s.Stream(ref unk08);
			s.Stream(ref unk10);
			s.Stream(ref unk18);
			s.Stream(ref unk20);
			SerializeActivity(s);
			s.Stream(ref Mode, 3, GameModeBitStreamer.Instance);
			s.Stream(ref EngineType, 3, GameEngineTypeBitStreamer.Instance);
			s.Stream(ref unk2C);
			s.Stream(ref EngineCategoryIndex, 8, signExtend:true);
			s.StreamObject(Creator);
			s.StreamObject(Modifier);
			s.Stream(ref Title, Memory.Strings.StringStorage.CStringUnicodeBigEndian, maxLength: kStringLength-1);
			s.Stream(ref Description, Memory.Strings.StringStorage.CStringUnicodeBigEndian, maxLength: kStringLength-1);

			if (Type == ContentType.Film || Type == ContentType.FilmClip)
				s.Stream(ref unk280);
			else if (Type == ContentType.GameVariant)
				s.Stream(ref unk280, 8, signExtend:true);

			if (Activity == 2)
				s.Stream(ref HopperId);

			SerializeGameSpecificData(s);
		}
		#endregion

		#region IEndianStreamSerializable Members
		protected abstract void SerializeGameSpecificData(IO.EndianStream s);

		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Type, ContentTypeBinaryStreamer.Instance);
			s.Pad24();
			s.Stream(ref FileLength);
			s.Stream(ref unk08);
			s.Stream(ref unk10);
			s.Stream(ref unk18);
			s.Stream(ref unk20);
			s.Stream(ref Activity);
			s.Stream(ref Mode, GameModeBinaryStreamer.Instance);
			s.Stream(ref EngineType, GameEngineTypeBinaryStreamer.Instance);
			s.Pad8();
			s.Stream(ref unk2C);
			s.StreamMethods(this,
				(_this, _s) => _this.EngineCategoryIndex = s.Reader.ReadSByte(),
				(_this, _s) => s.Writer.Write((sbyte)_this.EngineCategoryIndex));
			s.Pad24(); s.Pad32();
			s.Stream(Creator);
			s.Stream(Modifier);
			s.Stream(ref Title, kStringEncoding);
			s.Stream(ref Description, kStringEncoding);

			if (Type == ContentType.Film || Type == ContentType.FilmClip)
				s.Stream(ref unk280);
			else if (Type == ContentType.GameVariant)
			{
				s.StreamMethods(this,
					(_this, _s) => _this.unk280 = s.Reader.ReadSByte(),
					(_this, _s) => s.Writer.Write((sbyte)_this.unk280));
				s.Pad24();
			}
			else
				s.Pad32();
			s.Pad32(); s.Pad64();

			s.Stream(ref HopperId); s.Pad16();
			s.Pad32(); s.Pad64();

			SerializeGameSpecificData(s);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		protected abstract void SerializeActivity<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attrName, ref sbyte value)
			where TDoc : class
			where TCursor : class;
		protected abstract void SerializeGameSpecificData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnum("type", ref Type);
			SerializeActivity(s, "activity", ref Activity);
			s.StreamAttributeEnum("mode", ref Mode);
			s.StreamAttributeEnum("engine", ref EngineType);

			s.StreamAttributeOpt("fileLength", ref FileLength, Predicates.IsNotZero);

			using (s.EnterCursorBookmark("Unknown"))
			{
				s.StreamAttributeOpt("id0", ref unk08, Predicates.IsNotZero, NumeralBase.Hex);
				s.StreamAttributeOpt("id1", ref unk10, Predicates.IsNotZero, NumeralBase.Hex);
				s.StreamAttributeOpt("id2", ref unk18, Predicates.IsNotZero, NumeralBase.Hex);
				s.StreamAttributeOpt("unk20", ref unk20, Predicates.IsNotZero, NumeralBase.Hex);

				s.StreamAttributeOpt("unk2C", ref unk2C, Predicates.IsNotNone);
			}

			using (s.EnterCursorBookmark("Creator"))
				s.StreamObject(Creator);
			using (s.EnterCursorBookmark("Modifier"))
				s.StreamObject(Modifier);

			s.StreamElement("Title", ref Title);
			s.StreamElementOpt("Description", ref Description, Predicates.IsNotNullOrEmpty);

			if (Type == ContentType.Film || Type == ContentType.FilmClip)
				s.StreamElement("FilmLength_", ref unk280);
			else if (Type == ContentType.GameVariant)
				if (!s.StreamElementOpt("EngineIconIndex", ref unk280, Predicates.IsNotNone))
					EngineIconIndex = TypeExtensions.kNone;
			s.StreamAttributeOpt("EngineCategoryIndex", ref EngineCategoryIndex, Predicates.IsNotNone);

			if (Activity == 2)
				s.StreamElement("HopperID", ref HopperId);

			SerializeGameSpecificData(s);
		}
		#endregion
	};
}