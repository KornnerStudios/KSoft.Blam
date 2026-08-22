using System;
using System.Collections.Generic;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Blam.Localization
{
	using BitFieldTraits = Bitwise.BitFieldTraits;

	/// <summary>
	/// Represents a mapping between a game-agnostic possibly-supported-language and a game implementation's language
	/// </summary>
	/// <remarks>
	/// EngineLanguage: a game-agnostic possibly-supported-language
	/// GameLanguage: a game implementation's language
	/// </remarks>
	[System.Reflection.Obfuscation(Exclude=false)]
	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	[System.Diagnostics.DebuggerDisplay(
		"Game = {Game}, Lang = {Language}, Index = {GameIndex}, Supported = {IsSupported}")]
	public readonly struct GameLanguageHandle
		: IComparer<GameLanguageHandle>, System.Collections.IComparer // #REMOVE_BLAM
		, IComparable<GameLanguageHandle>, IComparable
		, IEquatable<GameLanguageHandle>
	{
		#region Constants
		// nesting these into a static class makes them run before the struct's static ctor...
		// which, being a value type cctor, may not run when we want it
		static class Constants
		{
			// #NOTE_BLAM: It is assumed that the maximum value of GameIndex is the last last registered language
			// After all, this is suppose to map one to the other...

			public static readonly BitFieldTraits kGameIndexBitField =
				new(LanguageRegistry.kLanguageIndexBitCount);
			public static readonly BitFieldTraits kIsSupportedBitField =
				new(Bits.kBooleanBitCount, kGameIndexBitField);
			public static readonly BitFieldTraits kLanguageIndexBitField =
				new(LanguageRegistry.kLanguageIndexBitCount, kIsSupportedBitField);
			public static readonly BitFieldTraits kBuildBitField =
				new(Engine.EngineBuildHandle.BitCount, kLanguageIndexBitField);

			public static readonly BitFieldTraits kLastBitField =
				kBuildBitField;
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		/// <remarks>22 bits at last count</remarks>
		public static int BitCount => Constants.kLastBitField.FieldsBitCount;
		public static uint Bitmask => Constants.kLastBitField.FieldsBitmask.u32;

		public static readonly GameLanguageHandle None = new();

		public static bool IsValidGameIndex(int index) => LanguageRegistry.IsValidLanguageIndex(index);
		#endregion

		#region Internal Value
		[Interop.FieldOffset(0)] readonly uint mHandle;

		//internal uint Handle { get { return mHandle; } }

		static void InitializeHandle(out uint handle,
			Engine.EngineBuildHandle buildHandle, int langIndex, int gameIndex)
		{
			uint is_supported = gameIndex.IsNotNone() ? 1U : 0U;

			var encoder = new Bitwise.HandleBitEncoder();
			encoder.EncodeNoneable32(gameIndex, Constants.kGameIndexBitField);
			encoder.Encode32(is_supported, Constants.kIsSupportedBitField);
			LanguageRegistry.BitEncodeLanguageIndex(ref encoder, langIndex);
			encoder.Encode32(buildHandle.Handle, Constants.kBuildBitField);

			if (encoder.UsedBitCount != GameLanguageHandle.BitCount)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Encoded game language handle used {0} bits; expected {1}.",
					encoder.UsedBitCount, GameLanguageHandle.BitCount));
			}

			handle = encoder.GetHandle32();
		}
		#endregion

		#region Ctor
		internal GameLanguageHandle(Engine.EngineBuildHandle buildHandle, int langIndex, int gameIndex)
		{
			if (!LanguageRegistry.IsValidLanguageIndex(langIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(langIndex), langIndex,
					string.Format(Util.InvariantCultureInfo,
						"Language index must be NONE or in the range [0, {0}).",
						LanguageRegistry.NumberOfLanguages));
			}
			if (!IsValidGameIndex(gameIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(gameIndex), gameIndex,
					string.Format(Util.InvariantCultureInfo,
						"Game language index must be NONE or in the range [0, {0}).",
						LanguageRegistry.NumberOfLanguages));
			}

			InitializeHandle(out mHandle, buildHandle, langIndex, gameIndex);
		}
		#endregion

		#region Value properties
		/// <summary>The handle to the game build this info specifically associates with</summary>
		public Engine.EngineBuildHandle Build => new(mHandle, Constants.kBuildBitField);
		/// <summary>Index of a language registered in the <see cref="LanguageRegistry"/></summary>
		public int LanguageIndex =>
			LanguageRegistry.BitDecodeLanguageIndex(mHandle, Constants.kLanguageIndexBitField.BitIndex);
		/// <summary>Is the language supported by <see cref="Build"/>?</summary>
		public bool IsSupported => 1 == Bits.BitDecode(mHandle, Constants.kIsSupportedBitField);
		/// <summary>Is the language unsupported by <see cref="Build"/>?</summary>
		public bool IsUnsupported => 0 == Bits.BitDecode(mHandle, Constants.kIsSupportedBitField);
		/// <summary>The index <see cref="LanguageIndex"/> maps to in <see cref="Build"/></summary>
		public int GameIndex => Bits.BitDecodeNoneable(mHandle, Constants.kGameIndexBitField);

		public string LanguageName { get {
			int lang_index = LanguageIndex;

			return lang_index.IsNone()
				? LanguageRegistry.kNoneName
				: LanguageRegistry.LanguageNames[lang_index];
		} }
		#endregion

		public bool IsNone =>
			// this only works because ALL bitfields are NONE encoded, meaning -1 values are encoded as 0
			mHandle == 0;

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is GameLanguageHandle objHandle)
			{
				return this.mHandle == objHandle.mHandle;
			}

			return false;
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on its exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode()
		{
			return (int)mHandle;
		}
		/// <summary>Returns <see cref="LanguageName"/></summary>
		/// <returns></returns>
		public override string ToString()
		{
			return LanguageName;
		}
		#endregion

		#region Operators
		public static bool operator==(GameLanguageHandle lhs, GameLanguageHandle rhs) => lhs.mHandle == rhs.mHandle;
		public static bool operator!=(GameLanguageHandle lhs, GameLanguageHandle rhs) => lhs.mHandle != rhs.mHandle;
		#endregion

		#region IComparer<GameLanguageHandle> Members
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(GameLanguageHandle x, GameLanguageHandle y)
		{
			return GameLanguageHandle.StaticCompare(x, y);
		}
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		int System.Collections.IComparer.Compare(object x, object y)
		{
			KSoft.Debug.TypeCheck.CastValue(x, out GameLanguageHandle _x);
			KSoft.Debug.TypeCheck.CastValue(y, out GameLanguageHandle _y);

			return GameLanguageHandle.StaticCompare(_x, _y);
		}
		#endregion

		#region IComparable<GameLanguageHandle> Members
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(GameLanguageHandle other)
		{
			return GameLanguageHandle.StaticCompare(this, other);
		}
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		int IComparable.CompareTo(object obj)
		{
			KSoft.Debug.TypeCheck.CastValue(obj, out GameLanguageHandle _obj);

			return GameLanguageHandle.StaticCompare(this, _obj);
		}
		#endregion

		#region IEquatable<GameLanguageHandle> Members
		/// <summary>See <see cref="IEquatable{T}.Equals"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(GameLanguageHandle other) => this.mHandle == other.mHandle;
		#endregion

		#region Util
		static int StaticCompare(GameLanguageHandle lhs, GameLanguageHandle rhs)
		{
			// #TODO figure out a a utility to do this generically for bit-encoded handles that can run
			// in the internal Constants class.
			if (GameLanguageHandle.BitCount >= Bits.kInt32BitCount)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Handle bit count must be less than {0}; actual bit count is {1}.",
					Bits.kInt32BitCount, GameLanguageHandle.BitCount));
			}

			int lhs_data = (int)lhs.mHandle;
			int rhs_data = (int)rhs.mHandle;
			int result = lhs_data - rhs_data;

			return result;
		}
		#endregion
	};
}
