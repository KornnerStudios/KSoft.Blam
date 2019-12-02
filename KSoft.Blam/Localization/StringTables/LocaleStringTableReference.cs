using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Localization.StringTables
{
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("CodeName = {CodeName}")]
	public partial class LocaleStringTableReference
		: IO.ITagElementStringNameStreamable
	{
		#region CodeName
		string mCodeName;
		public string CodeName {
			get { Contract.Ensures(Contract.Result<string>() != null);
				return mCodeName;
			} set { Contract.Requires<ArgumentNullException>(value != null);
				mCodeName = value;
				NotifyPropertyChanged(kCodeNameChanged);
		} }
		#endregion

		readonly GameLanguageTable mLanguageTable;
		readonly string[] mLanguageStrings;

		#region LanguageOffsets
		// Only valid when reading/writing. Set to null once strings have been read, or the offsets have been written
		int[] mLanguageOffsets;

		void LanguageOffsetsInitialize()
		{
			mLanguageOffsets = new int[mLanguageStrings.Length];

			for (int x = 0; x < mLanguageOffsets.Length; x++)
				mLanguageOffsets[x] = TypeExtensions.kNone;
		}
		void LanguageOffsetsDispose()
		{
			mLanguageOffsets = null;
		}
		#endregion

		#region Language interfaces
		string GetImpl(int langIndex)
		{
			var engine_lang = mLanguageTable.GetEngineLanguage(langIndex);
			Contract.Assert(engine_lang.IsSupported);

			return mLanguageStrings[engine_lang.GameIndex];
		}
		void SetImpl(int langIndex, string value)
		{
			var engine_lang = mLanguageTable.GetEngineLanguage(langIndex);
			Contract.Assert(engine_lang.IsSupported);

			mLanguageStrings[engine_lang.GameIndex] = value;

			NotifyPropertyChanged(LanguageRegistry.GetLanguageChangedEventArgs(langIndex));
		}

		public string English {
			get { return GetImpl(LanguageRegistry.EnglishIndex); }
			set { SetImpl(LanguageRegistry.EnglishIndex, value); }
		}
		#endregion

		internal bool JustEnglish { get {
			Contract.Requires(English != null);
			string english = English;

			bool just_english = true; // either all strings match the english one
			bool rest_are_null = true;// or the rest are null and only the english one is set
			LanguageRegistry.CodeExpectsEnglishFirst();
			for (int x = 1; x < mLanguageStrings.Length; x++)
			{
				rest_are_null &= mLanguageStrings[x] == null;

				if (mLanguageStrings[x] != null && !StringComparer.Ordinal.Equals(mLanguageStrings[x], english))
					just_english = false;
			}

			return just_english || rest_are_null;
		} }

		public LocaleStringTableReference(GameLanguageTable languageTable)
		{
			mCodeName = "";

			mLanguageTable = languageTable;

			mLanguageStrings = new string[languageTable.GameLanguageCount];
		}

		internal void ReadLanguageStrings(IO.EndianReader buffer, uint langBitmask = uint.MaxValue)
		{
			Contract.Assert(LanguageRegistry.NumberOfLanguages <= Bits.kInt32BitCount,
				"langBitmask is too small to actually be a language bitvector");
			Contract.Assert(mLanguageOffsets != null);

			for (int x = 0; x < mLanguageOffsets.Length; x++)
			{
				int offset = mLanguageOffsets[x];
				if (offset.IsNone())
					continue;
				else if (!Bitwise.Flags.Test(langBitmask, 1U << x))
					continue;

				buffer.Seek(offset);
				mLanguageStrings[x] = buffer.ReadString(Memory.Strings.StringStorage.CStringUtf8);
			}

			LanguageOffsetsDispose();
		}
		internal void WriteLanguageString(Memory.Strings.StringMemoryPool pool)
		{
			LanguageOffsetsInitialize();

			if (JustEnglish)
			{
				var ptr = pool.Add(mLanguageStrings[0]);
				for (int x = 0, offset = (int)ptr.u32; x < mLanguageOffsets.Length; x++)
					mLanguageOffsets[x] = offset;
			}
			else
			{
				for (int x = 0; x < mLanguageStrings.Length; x++)
				{
					string str = mLanguageStrings[x];

					var ptr = str != null
						? pool.Add(str)
						: Values.PtrHandle.InvalidHandle32;
					mLanguageOffsets[x] = (int)ptr.u32;
				}
			}
		}

		#region IBitStreamSerializable Members
		public void Serialize(IO.BitStream s, int kIndexBitCount)
		{
			if (s.IsReading && mLanguageOffsets == null)
				LanguageOffsetsInitialize();

			for (int x = 0; x < mLanguageOffsets.Length; x++)
				s.StreamIndexPos(ref mLanguageOffsets[x], kIndexBitCount);

			if (s.IsWriting)
				LanguageOffsetsDispose();
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		void SerializeLanguages<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool writing = s.IsWriting;

			for (int x = 0; x < mLanguageStrings.Length; x++)
			{
				if (writing && mLanguageStrings[x] == null)
					continue;

				using (var bm = s.EnterCursorBookmarkOpt(mLanguageTable.GetGameLanguage(x).LanguageName)) if (bm.IsNotNull)
					s.StreamCursor(ref mLanguageStrings[x]);
				else
				{
					if (x == LanguageRegistry.EnglishIndex)
						s.ThrowReadException(new System.IO.InvalidDataException(
							"Tried to serialize multilingual string with no English translation"));

					// If ChineseSimple is empty, engine will fall back to ChineseTraditional (for HaloReach and H4 at least)
					if (x != LanguageRegistry.ChineseSimpleIndex)
					{
						// default to english if there's no element for this lang
						mLanguageStrings[x] = mLanguageStrings[LanguageRegistry.EnglishIndex];
					}
				}
			}
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt("name", ref mCodeName, Predicates.IsNotNullOrEmpty))
				mCodeName = "";

			using (var bm = s.EnterCursorBookmarkOpt("String", this, obj=>obj.JustEnglish)) if (bm.IsNotNull)
			{
				LanguageRegistry.CodeExpectsEnglishFirst();
				int k_english_index = LanguageRegistry.EnglishIndex;

				s.StreamCursor(ref mLanguageStrings[k_english_index]);
				if (s.IsReading)
				{
					// empty cursor text returns null, at least in the XML implementation
					if (mLanguageStrings[k_english_index] == null)
						mLanguageStrings[k_english_index] = "";

					for (int x = 1; x < mLanguageStrings.Length; x++)
						mLanguageStrings[x] = mLanguageStrings[k_english_index];
				}
			}
			else
				SerializeLanguages(s);
		}
		#endregion
	};
}