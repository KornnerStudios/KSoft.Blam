using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.RuntimeData
{
	using GameEngineTypeBitStreamer = IO.EnumBitStreamer<GameEngineType>;

	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class ContentMiniMetadata
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		static readonly Text.RadixEncoding kRadixEncoding = new Text.RadixEncoding(
			"abcdefghijklmnopqrstuvwxyz012345", Shell.EndianFormat.Big, true);
		static readonly Memory.Strings.StringStorage kAuthorStorage = new Memory.Strings.StringStorage(Memory.Strings.StringStorageWidthType.Ascii, Memory.Strings.StringStorageType.CharArray,
			fixedLength: kAuthorStorageLength);
		static readonly Text.StringStorageEncoding kAuthorEncoding = new Text.StringStorageEncoding(kAuthorStorage);

		const int kBitStreamSizeInBytes = 0x80;
		const int kEncodedPortionLength = 0x2A; // characters
		const int kContainerNameLength = 1 + kEncodedPortionLength;
		const int kAuthorStorageLength = 13; // 16 at runtime

		//public EngineGame Game { get; private set; }
		public Engine.EngineBuildHandle BuildHandle { get; private set; }

		public ContentMiniMetadataType Type;
		public DateTime Timestamp;
		public byte SessionSalt;

		public GameEngineType EngineType;
		public int unk4 = TypeExtensions.kNone;
		public int MegaloCategoryIndex = TypeExtensions.kNone;
		public int EngineIconIndex = TypeExtensions.kNone;
		public bool unkA, unkB;
		public string Author;

		protected ContentMiniMetadata(Engine.EngineBuildHandle buildHandle)
		{
			BuildHandle = buildHandle;
		}

		public static ContentMiniMetadata Create(Engine.EngineBuildHandle gameBuild)
		{
			if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHaloReach))
				return new Games.HaloReach.RuntimeData.ContentMiniMetadataHaloReach(gameBuild);

			if (gameBuild.IsWithinSameBranch(Engine.EngineRegistry.EngineBranchHalo4))
				return new Games.Halo4.RuntimeData.ContentMiniMetadataHalo4(gameBuild);

			throw new KSoft.Debug.UnreachableException(gameBuild.ToDisplayString());
		}

		#region IO.IBitStreamSerializable
		protected abstract void SerializeActivity(IO.BitStream s);
		public void Serialize(IO.BitStream s)
		{
			var engine_category_limits = Megalo.Proto.MegaloProtoSystem.GetScriptDatabaseAsync(BuildHandle).Result.Limits.EngineCategories;

			s.Stream(ref Timestamp, 36);
			s.Stream(ref SessionSalt);
			SerializeActivity(s);
			s.Stream(ref EngineType, 3, GameEngineTypeBitStreamer.Instance);
			s.Stream(ref unk4);
			s.StreamNoneable(ref MegaloCategoryIndex, engine_category_limits.IndexBitLength);
			s.StreamNoneable(ref EngineIconIndex, engine_category_limits.IndexBitLength);
			s.Stream(ref unkA);
			s.Stream(ref unkB);
			s.Stream(ref Author, kAuthorEncoding);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		protected abstract void SerializeActivity<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var build = s.IsWriting ? BuildHandle : Engine.EngineBuildHandle.None;
			using(s.EnterCursorBookmark("Game"))
				Engine.EngineBuildHandle.Serialize(s, ref build);
			if (s.IsReading)
			{
				// #TODO_BLAM: validate build handle?
				BuildHandle = build;
			}

			s.StreamElementEnum("Content", ref Type);

			s.StreamElement("TimeStamp", ref Timestamp);
			s.WriteComment(this, _this => _this.Timestamp.ToString(System.Globalization.CultureInfo.InvariantCulture));

			s.StreamElement("SessionSalt", ref SessionSalt, NumeralBase.Hex);
			SerializeActivity(s);
			s.StreamElementEnum("Engine", ref EngineType);
			s.StreamAttributeOpt("unk4_", ref unk4, Predicates.IsNotNone);
			s.StreamAttributeOpt("megaloCategory", ref MegaloCategoryIndex, Predicates.IsNotNone);
			s.StreamAttributeOpt("engineIcon", ref EngineIconIndex, Predicates.IsNotNone);
			s.StreamAttribute("unkA_", ref unkA);
			s.StreamAttribute("unkB_", ref unkB);
			s.StreamElement("Author", ref Author);
		}
		#endregion

		public static bool ContainerNameIsValid(string containerName)
		{
			bool valid = !string.IsNullOrWhiteSpace(containerName) && containerName.Length <= kEncodedPortionLength+1;

			if (valid && containerName.Length > 0)
			{
				var type = (ContentMiniMetadataType)containerName[0];

				valid = type.IsValid();
			}

			return valid;
		}

		public string Encode()
		{
			string encoded_porition;
			using (var ms = new System.IO.MemoryStream(kBitStreamSizeInBytes))
			using (var bs = new IO.BitStream(ms))
			{
				bs.StreamMode = System.IO.FileAccess.Write;
				Serialize(bs);
				bs.Flush();

				byte[] bits = ms.ToArray();
				encoded_porition = kRadixEncoding.Encode(bits);
			}

			Contract.Assert(encoded_porition.Length <= kEncodedPortionLength);

			return Type.ToEncodingPrefix() + encoded_porition;
		}
		public static ContentMiniMetadata Decode(Engine.EngineBuildHandle buildHandle, string containerName)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(containerName));
			Contract.Requires<ArgumentException>(containerName.Length <= kEncodedPortionLength+1);

			var result = Create(buildHandle);

			result.Type = (ContentMiniMetadataType)containerName[0];
			if (!result.Type.IsValid())
				throw new ArgumentException("Unrecognized content type: " + containerName[0]);

			string encoded_porition = containerName.Substring(1);

			byte[] bits = kRadixEncoding.Decode(encoded_porition);
			using (var ms = new System.IO.MemoryStream(bits))
			using (var bs = new IO.BitStream(ms))
			{
				bs.StreamMode = System.IO.FileAccess.Read;

				result.Serialize(bs);
			}

			return result;
		}
	};
}