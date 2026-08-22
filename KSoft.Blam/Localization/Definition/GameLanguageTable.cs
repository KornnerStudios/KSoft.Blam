using System;

namespace KSoft.Blam.Localization
{
	/// <summary>
	/// Represents mappings between game-agnostic possibly-supported-languages and a game implementation's languages
	/// </summary>
	/// <remarks>
	/// EngineLanguage: a game-agnostic possibly-supported-language
	/// GameLanguage: a game implementation's language
	/// </remarks>
	public sealed class GameLanguageTable
		: IO.ITagElementStringNameStreamable
		, IEquatable<GameLanguageTable>
	{
		const string kErrorMessageNotInitialized =
			"GameLanguageTable not yet initialized";

		Engine.EngineBuildHandle mBuildHandle = Engine.EngineBuildHandle.None;
		Collections.BitVector32 mOptionalEngineLanguageFlags = new();
		Collections.BitVector32 mOptionalGameLanguageFlags = new();
		// An array of all registered languages and how they map to the build
		GameLanguageHandle[] mEngineLanguageTable;
		// All the IsSupported elements in mEngineLanguageTable, allowing us to index by game index
		GameLanguageHandle[] mGameLanguageTable;

		/// <summary>The handle for the build of the engine this table is associated with</summary>
		public Engine.EngineBuildHandle BuildHandle { get {
			if (mBuildHandle.IsNone)
			{
				throw new InvalidOperationException(kErrorMessageNotInitialized);
			}

			return mBuildHandle;
		} }

		/// <summary>Number of languages supported in this build of the engine</summary>
		public int GameLanguageCount { get {
			if (mGameLanguageTable == null)
			{
				throw new InvalidOperationException(kErrorMessageNotInitialized);
			}

			return mGameLanguageTable.Length;
		} }

		public GameLanguageHandle EnglishGameLangaugeHandle { get {
			if (mEngineLanguageTable == null)
			{
				throw new InvalidOperationException(kErrorMessageNotInitialized);
			}

			return mEngineLanguageTable[LanguageRegistry.EnglishIndex];
		} }

		public GameLanguageHandle GetEngineLanguage(int langIndex)
		{
			if (!LanguageRegistry.IsValidLanguageIndex(langIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(langIndex), langIndex,
					string.Format(Util.InvariantCultureInfo,
						"Language index must be NONE or in the range [0, {0}).",
						LanguageRegistry.NumberOfLanguages));
			}
			if (mEngineLanguageTable == null)
			{
				throw new InvalidOperationException(kErrorMessageNotInitialized);
			}

			return mEngineLanguageTable[langIndex];
		}
		public GameLanguageHandle GetGameLanguage(int gameIndex)
		{
			if (!IsValidGameIndex(gameIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(gameIndex), gameIndex,
					string.Format(Util.InvariantCultureInfo,
						"Game language index must be NONE or in the range [0, {0}).",
						GameLanguageCount));
			}

			return mGameLanguageTable[gameIndex];
		}

		public bool IsEngineLanguageOptional(int langIndex)
		{
			if (!LanguageRegistry.IsValidLanguageIndex(langIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(langIndex), langIndex,
					string.Format(Util.InvariantCultureInfo,
						"Language index must be NONE or in the range [0, {0}).",
						LanguageRegistry.NumberOfLanguages));
			}
			if (mEngineLanguageTable == null)
			{
				throw new InvalidOperationException(kErrorMessageNotInitialized);
			}

			return mOptionalEngineLanguageFlags[langIndex];
		}
		public bool IsGameLanguageOptional(int gameIndex)
		{
			if (!IsValidGameIndex(gameIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(gameIndex), gameIndex,
					string.Format(Util.InvariantCultureInfo,
						"Game language index must be NONE or in the range [0, {0}).",
						GameLanguageCount));
			}

			return mOptionalGameLanguageFlags[gameIndex];
		}

		#region Index interfaces
		[System.Diagnostics.DebuggerStepThrough]
		public bool IsValidGameIndex(int gameIndex)
			=> gameIndex.IsNoneOrPositive() && gameIndex < GameLanguageCount;

		public int LanguageIndexToGameIndex(int langIndex)
		{
			if (!LanguageRegistry.IsValidLanguageIndex(langIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(langIndex), langIndex,
					string.Format(Util.InvariantCultureInfo,
						"Language index must be NONE or in the range [0, {0}).",
						LanguageRegistry.NumberOfLanguages));
			}

			return GetEngineLanguage(langIndex).GameIndex;
		}
		public int LanguageIndexFromGameIndex(int gameIndex)
		{
			if (!IsValidGameIndex(gameIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(gameIndex), gameIndex,
					string.Format(Util.InvariantCultureInfo,
						"Game language index must be NONE or in the range [0, {0}).",
						GameLanguageCount));
			}

			return GetGameLanguage(gameIndex).LanguageIndex;
		}
		#endregion

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is GameLanguageTable objTable)
			{
				return objTable.Equals(this);
			}

			return false;
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on its exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode() => mBuildHandle.GetHashCode();
		/// <summary><see cref="Engine.EngineBuildHandle.ToString()"/></summary>
		/// <returns></returns>
		public override string ToString()
		{
			return mBuildHandle.ToString();
		}
		#endregion

		#region IEquatable<GameLanguageTable> Members
		/// <summary>See <see cref="IEquatable{T}.Equals"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(GameLanguageTable other) => this.mBuildHandle.Equals(other.mBuildHandle);
		#endregion

		#region Initialization
		void InitializeEngineLanguageTableWithBuildHandle()
		{
			mEngineLanguageTable = new GameLanguageHandle[LanguageRegistry.NumberOfLanguages];

			for (int langIndex = 0; langIndex < mEngineLanguageTable.Length; langIndex++)
			{
				mEngineLanguageTable[langIndex] =
					new GameLanguageHandle(mBuildHandle, langIndex, TypeExtensions.kNone);
			}

			if (LanguageRegistry.NumberOfLanguages > Bits.kInt32BitCount)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Language count must fit in a 32-bit bitvector; actual count is {0}.",
					LanguageRegistry.NumberOfLanguages));
			}
		}
		bool InitializeGameLanguageTableFromEngineTable(int gameLangCount)
		{
			mGameLanguageTable = new GameLanguageHandle[gameLangCount];
			bool game_indexes_valid = true;

			foreach (var lang in mEngineLanguageTable)
			{
				if (lang.IsUnsupported)
				{
					continue;
				}

				int game_index = lang.GameIndex;
				if (game_index >= gameLangCount)
				{
					game_indexes_valid = false;
					break;
				}

				mGameLanguageTable[game_index] = lang;
			}

			return game_indexes_valid;
		}
		#endregion

		#region ITagElementStreamable<string> Members
		static bool IsInvalidGameIndexFromStream(int index)
			=> index < 0 || !GameLanguageHandle.IsValidGameIndex(index);
		void ReadEngineLanguageTable<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			const string kElementNameEntry = "E";
			const string kAttributeNameGameIndex = "id";
			const string kAttributeNameLangIndex = "lang";
			const string kAttributeNameOptional = "optional";

			// number of languages for this build of the engine
			int lang_count = 0;

			foreach (var e in s.ElementsByName(kElementNameEntry))
			{
				using (s.EnterCursorBookmark(e))
				{
					int game_index = TypeExtensions.kNone;
					int lang_index = TypeExtensions.kNone;
					bool is_optional = false;

					s.ReadAttribute(kAttributeNameGameIndex, ref game_index, NumeralBase.Decimal);
					LanguageRegistry.SerializeLanguageId(s, kAttributeNameLangIndex, ref lang_index);
					s.ReadAttributeOpt(kAttributeNameOptional, ref is_optional);

					if (IsInvalidGameIndexFromStream(game_index) || lang_index.IsNone())
					{
						s.ThrowReadException(new System.IO.InvalidDataException("Invalid table entry data"));
					}

					mEngineLanguageTable[lang_index] = new GameLanguageHandle(mBuildHandle, lang_index, game_index);
					if (is_optional)
					{
						mOptionalEngineLanguageFlags[lang_index] = true;
						mOptionalGameLanguageFlags[game_index] = true;
					}

					lang_count++;
				}
			}

			if (lang_count == 0)
			{
				s.ThrowReadException(new System.IO.InvalidDataException("Table has no entries"));
			}

			if (!InitializeGameLanguageTableFromEngineTable(lang_count))
			{
				s.ThrowReadException(new System.IO.InvalidDataException("Invalid game index data"));
			}
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			var system = KSoft.Debug.TypeCheck.CastReference<LanguageSystem>(s.UserData);

			Engine.EngineBuildHandle.SerializeWithBaseline(s, system.Engine.RootBuildHandle,
				ref mBuildHandle);

			if (reading)
			{
				InitializeEngineLanguageTableWithBuildHandle();
			}

			using (s.EnterCursorBookmark("Entries"))
			{
				if (reading)
				{
					ReadEngineLanguageTable(s);
				}
				else
				{
					throw new KSoft.Debug.UnreachableException("Writing not supported");
				}
			}
		}
		#endregion

		public EnumeratorWrapper<GameLanguageHandle> EngineLanguageHandles => new(mEngineLanguageTable);
		public EnumeratorWrapper<GameLanguageHandle> SupportedLanguageHandles => new(mGameLanguageTable);
	};
}
