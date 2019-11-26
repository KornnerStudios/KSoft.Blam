using System;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Localization
{
	/// <summary>Represents mappings between game-agnostic possibly-supported-languages and a game implementation's languages</summary>
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

		Engine.EngineBuildHandle mBuildHandle;
		// An array of all registered languages and how they map to the build
		GameLanguageHandle[] mEngineLanguageTable;
		// All the IsSupported elements in mEngineLanguageTable, allowing us to index by game index
		GameLanguageHandle[] mGameLanguageTable;

		public GameLanguageTable()
		{
			mBuildHandle = Engine.EngineBuildHandle.None;
		}

		/// <summary>The handle for the build of the engine this table is associated with</summary>
		public Engine.EngineBuildHandle BuildHandle { get {
			Contract.Assert(!mBuildHandle.IsNone, kErrorMessageNotInitialized);

			return mBuildHandle;
		} }

		/// <summary>Number of languages supported in this build of the engine</summary>
		public int GameLanguageCount { get {
			Contract.Assert(mGameLanguageTable != null, kErrorMessageNotInitialized);

			return mGameLanguageTable.Length;
		} }

		[Contracts.Pure]
		public GameLanguageHandle GetEngineLanguage(int langIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(LanguageRegistry.IsValidLanguageIndex(langIndex));
			Contract.Assert(mEngineLanguageTable != null, kErrorMessageNotInitialized);

			return mEngineLanguageTable[langIndex];
		}
		[Contracts.Pure]
		public GameLanguageHandle GetGameLanguage(int gameIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(IsValidGameIndex(gameIndex));
			Contract.Assert(mGameLanguageTable != null, kErrorMessageNotInitialized);

			return mGameLanguageTable[gameIndex];
		}

		#region Index interfaces
		[Contracts.Pure]
		[System.Diagnostics.DebuggerStepThrough]
		public bool IsValidGameIndex(int gameIndex)
		{
			return gameIndex.IsNoneOrPositive() && gameIndex < GameLanguageCount;
		}

		[Contracts.Pure]
		public int LanguageIndexToGameIndex(int langIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(LanguageRegistry.IsValidLanguageIndex(langIndex));

			return GetEngineLanguage(langIndex).GameIndex;
		}
		[Contracts.Pure]
		public int LanguageIndexFromGameIndex(int gameIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(IsValidGameIndex(gameIndex));
			Contract.Assert(mGameLanguageTable != null, kErrorMessageNotInitialized);

			return GetGameLanguage(gameIndex).LanguageIndex;
		}
		#endregion

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is GameLanguageTable)
				return ((GameLanguageTable)obj).Equals(this);

			return false;
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on its exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode()
		{
			return mBuildHandle.GetHashCode();
		}
		/// <summary><see cref="Engine.EngineBuildHandle.ToString()"/></summary>
		/// <returns></returns>
		public override string ToString()
		{
			Contract.Ensures(Contract.Result<string>() != null);

			return mBuildHandle.ToString();
		}
		#endregion

		#region IEquatable<GameLanguageTable> Members
		/// <summary>See <see cref="IEquatable{T}.Equals"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(GameLanguageTable other)
		{
			return this.mBuildHandle.Equals(other.mBuildHandle);
		}
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
		}
		bool InitializeGameLanguageTableFromEngineTable(int gameLangCount)
		{
			mGameLanguageTable = new GameLanguageHandle[gameLangCount];
			bool game_indexes_valid = true;

			foreach (var lang in mEngineLanguageTable)
			{
				if (lang.IsUnsupported)
					continue;

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
		{
			return index < 0 || !GameLanguageHandle.IsValidGameIndex(index);
		}
		void ReadEngineLanguageTable<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			const string kElementNameEntry = "E";
			const string kAttributeNameGameIndex = "id";
			const string kAttributeNameLangIndex = "lang";

			// number of languages for this build of the engine
			int lang_count = 0;

			foreach (var e in s.ElementsByName(kElementNameEntry))
			{
				int game_index = TypeExtensions.kNone;
				int lang_index = TypeExtensions.kNone;

				s.ReadAttribute(kAttributeNameGameIndex, ref game_index, NumeralBase.Decimal);
				LanguageRegistry.SerializeLanguageId(s, kAttributeNameLangIndex, ref lang_index);

				if (IsInvalidGameIndexFromStream(game_index) || lang_index.IsNone())
					s.ThrowReadException(new System.IO.InvalidDataException("Invalid table entry data"));

				mEngineLanguageTable[lang_index] = new GameLanguageHandle(mBuildHandle, lang_index, game_index);

				lang_count++;
			}

			if(lang_count == 0)
				s.ThrowReadException(new System.IO.InvalidDataException("Table has no entries"));

			if (!InitializeGameLanguageTableFromEngineTable(lang_count))
				s.ThrowReadException(new System.IO.InvalidDataException("Invalid game index data"));
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
				InitializeEngineLanguageTableWithBuildHandle();

			using (s.EnterCursorBookmark("Entries"))
			{
				if (reading)
					ReadEngineLanguageTable(s);
				else
					Contract.Assert(false, "Writing not supported");
			}
		}
		#endregion
	};
}