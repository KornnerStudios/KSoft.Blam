using System;
using System.Collections.Generic;
using System.IO;

namespace KSoft.Blam.Localization
{
	/// <summary>Tracks all global-level language information and objects</summary>
	public static partial class LanguageRegistry
	{
		const string kErrorMessageNotInitialized =
			"LanguageRegistry not yet initialized";

		const string kRegistryFilePath = @"Localization\LanguageRegistry.xml";
		internal const string kNoneName = TypeExtensions.kNoneDisplayString;
		const string kEnglishName = "English";
		const string kChineseSimpleName = "ChineseSimp";

		#region Languages
		internal const int kMaxLanguages = 32 - 1; // 5 bits
		internal static readonly int kLanguageIndexBitCount = Bits.GetMaxEnumBits(kMaxLanguages);
		private static readonly uint kLanguageIndexBitMask = Bits.BitCountToMask32(kLanguageIndexBitCount);

		static List<string> gLanguageNames = null!;
		/// <summary>Names of all the registered languages</summary>
		public static IReadOnlyList<string> LanguageNames { get {
			if (gLanguageNames == null)
			{
				throw new InvalidOperationException(kErrorMessageNotInitialized);
			}

			return gLanguageNames;
		} }
		/// <summary>Number of languages that have been registered</summary>
		public static int NumberOfLanguages { get {
			if (gLanguageNames == null)
			{
				throw new InvalidOperationException(kErrorMessageNotInitialized);
			}

			return gLanguageNames.Count;
		} }

		[System.Diagnostics.DebuggerStepThrough]
		public static bool IsValidLanguageIndex(int languageIndex)
			=> languageIndex.IsNoneOrPositive() && languageIndex < NumberOfLanguages;

		static int LanguageIdResolver(object? _null, string name)
		{
			int id = TypeExtensions.kNone;

			if (name != kNoneName)
			{
				id = LanguageNames.FindIndex(name.Equals);

				if (id.IsNone())
				{
					throw new KeyNotFoundException(string.Format(Util.InvariantCultureInfo,
						"No language is registered with the name '{0}'",
						name));
				}
			}

			return id;
		}
		static readonly Func<object?, string, int> LanguageIdResolverSansKeyNotFoundException =
			(_null, name) => name != kNoneName
				? LanguageNames.FindIndex(name.Equals)
				: TypeExtensions.kNone;
		static readonly Func<object?, int, string> LanguageNameResolver =
			(_null, id) => id.IsNotNone()
				? LanguageNames[id]
				: kNoneName;
		#endregion

		#region Well known languages
		/// <summary>The index which the English language is registered at</summary>
		public static int EnglishIndex { get; private set; }
		/// <summary>The index which the Chinese (Simple) language is registered at</summary>
		/// <remarks>only 'well known' for <see cref="StringTables.LocaleStringTableReference"/></remarks>
		internal static int ChineseSimpleIndex { get; private set; }

		/// <summary>Call before code which expects English to be the first registered language</summary>
		internal static void CodeExpectsEnglishFirst()
		{
			if (LanguageRegistry.EnglishIndex != 0)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Caller code assumes English is first; actual English index is {0}.",
					LanguageRegistry.EnglishIndex));
			}
		}
		#endregion

		#region Initialization
		static void InitializeLanguageNames()
		{
			using (var s = OpenRegistryTagElementStream())
			{
				SerializeLanguages(s);
			}

			KSoft.Debug.ValueCheck.IsGreaterThanEqualTo("LanguageRegistry: No registered languages",
				1, gLanguageNames.Count);

			KSoft.Debug.ValueCheck.IsLessThanEqualTo("LanguageRegistry: Too many registered languages",
				kMaxLanguages, gLanguageNames.Count);

			KSoft.Debug.ValueCheck.IsDistinct("LanguageRegistry: Duplicate languages registered",
				"name", gLanguageNames);

			gLanguageNames.TrimExcess();
		}

		static int ResolveWellKnownLanguage(string name)
		{
			int index = LanguageIdResolverSansKeyNotFoundException(null, name);

			if (index.IsNone())
			{
				throw new InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"{0} doesn't define the required language {1}",
					kRegistryFilePath, name));
			}

			return index;
		}
		static void ResolveWellKnownLanguages()
		{
			EnglishIndex = ResolveWellKnownLanguage(kEnglishName);
			ChineseSimpleIndex = ResolveWellKnownLanguage(kChineseSimpleName);
		}

		public static void Initialize()
		{
			if (gLanguageNames != null)
			{
				throw new InvalidOperationException("Tried to initialize the language registry more than once");
			}

			gLanguageNames = new List<string>(kMaxLanguages);

			InitializeLanguageNames();
			ResolveWellKnownLanguages();
		}
		public static void Dispose()
		{
			gLanguageNames = null!;

			EnglishIndex = TypeExtensions.kNone;
		}
		#endregion

		#region Bit encoding
		internal static void BitEncodeLanguageIndex(ref Bitwise.HandleBitEncoder encoder, int languageIndex)
		{
			if (!IsValidLanguageIndex(languageIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(languageIndex), languageIndex,
					string.Format(Util.InvariantCultureInfo,
						"Language index must be NONE or in the range [0, {0}).",
						NumberOfLanguages));
			}

			encoder.EncodeNoneable32(languageIndex, kLanguageIndexBitMask);
		}
		internal static int BitDecodeLanguageIndex(uint handle, int bitIndex)
		{
			int index = Bits.BitDecodeNoneable(handle, bitIndex, kLanguageIndexBitMask);

			if (!IsValidLanguageIndex(index))
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Decoded language index must be NONE or in the range [0, {0}); actual value is {1}.",
					NumberOfLanguages, index));
			}

			return index;
		}
		#endregion

		#region ITagElementStreamable<string> Members
		static dynamic OpenRegistryTagElementStream(FileAccess streamMode = FileAccess.Read)
		{
			if (!File.Exists(kRegistryFilePath))
			{
				throw new FileNotFoundException(
					"Can't initialize the LanguageRegistry, need the following file: " + kRegistryFilePath,
					kRegistryFilePath);
			}

			var stream = IO.TagElementStreamFactory.Open(kRegistryFilePath);
			stream.StreamMode = streamMode;

			return stream;
		}

		static void SerializeLanguages<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("Languages"))
			{
				s.StreamElements("Language", gLanguageNames);
			}
		}

		internal static bool SerializeLanguageId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref int languageId, bool isOptional = false)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = true;

			if (isOptional)
			{
				streamed = s.StreamAttributeOptIdAsString(attributeName, ref languageId, null,
					LanguageIdResolver, LanguageNameResolver, Predicates.IsNotNullOrEmpty);

				if (!streamed && s.IsReading)
				{
					languageId = TypeExtensions.kNone;
				}
			}
			else
			{
				s.StreamAttributeIdAsString(attributeName, ref languageId, null,
					LanguageIdResolver, LanguageNameResolver);
			}

			return streamed;
		}
		#endregion
	};
}
