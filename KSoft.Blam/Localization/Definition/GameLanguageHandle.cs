using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Interop = System.Runtime.InteropServices;

namespace KSoft.Blam.Localization
{
	/// <summary>Represents a mapping between a game-agnostic possibly-supported-language and a game implementation's language</summary>
	/// <remarks>
	/// EngineLanguage: a game-agnostic possibly-supported-language
	/// GameLanguage: a game implementation's language
	/// </remarks>
	[System.Reflection.Obfuscation(Exclude=false)]
	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	[System.Diagnostics.DebuggerDisplay("Game = {Game}, Lang = {Language}, Index = {GameIndex}, Supported = {IsSupported}")]
	public struct GameLanguageHandle
		: IComparer<GameLanguageHandle>, System.Collections.IComparer
		, IComparable<GameLanguageHandle>, IComparable
		, IEquatable<GameLanguageHandle>
	{
		#region Constants
		// nesting these into a static class makes them run before the struct's static ctor...
		// which, being a value type cctor, may not run when we want it
		static class Constants
		{
			// NOTE: It is assumed that the maximum value of GameIndex is the last last registered language
			// After all, this is suppose to map one to the other...

			public static readonly int kGameIndexShift =
				0;
			public static readonly int kGameIndexBitCount =
				LanguageRegistry.kLanguageIndexBitCount;
			public static readonly uint kGameIndexBitMask =
				Bits.BitCountToMask32(kGameIndexBitCount);

			public static readonly int kIsSupportedShift =
				kGameIndexShift + kGameIndexBitCount;
			public static readonly uint kIsSupportedMask =
				0x1;

			public static readonly int kLanguageIndexShift =
				kIsSupportedShift + Bits.kBooleanBitCount;

			public static readonly int kBuildShift =
				kLanguageIndexShift + LanguageRegistry.kLanguageIndexBitCount;

			public static readonly int kBitCount =
				kBuildShift + Engine.EngineBuildHandle.BitCount;
			public static readonly uint kBitmask = Bits.BitCountToMask32(kBitCount);
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		/// <remarks>22 bits at last count</remarks>
		public static int BitCount { get { return Constants.kBitCount; } }
		public static uint Bitmask { get { return Constants.kBitmask; } }

		public static readonly GameLanguageHandle None = new GameLanguageHandle();

		[Contracts.Pure]
		public static bool IsValidGameIndex(int index) { return LanguageRegistry.IsValidLanguageIndex(index); }
		#endregion

		#region Internal Value
		[Interop.FieldOffset(0)] readonly uint mHandle;

		//internal uint Handle { get { return mHandle; } }

		static void InitializeHandle(out uint handle,
			Engine.EngineBuildHandle buildHandle, int langIndex, int gameIndex)
		{
			uint is_supported = gameIndex.IsNotNone() ? 1U : 0U;

			var encoder = new Bitwise.HandleBitEncoder();
			encoder.EncodeNoneable32(gameIndex, Constants.kGameIndexBitMask);
			encoder.Encode32(is_supported, Constants.kIsSupportedMask);
			LanguageRegistry.BitEncodeLanguageIndex(ref encoder, langIndex);
			encoder.Encode32(buildHandle.Handle, Engine.EngineBuildHandle.Bitmask);

			Contract.Assert(encoder.UsedBitCount == GameLanguageHandle.BitCount);

			handle = encoder.GetHandle32();
		}
		#endregion

		#region Ctor
		internal GameLanguageHandle(Engine.EngineBuildHandle buildHandle, int langIndex, int gameIndex)
		{
			Contract.Requires(LanguageRegistry.IsValidLanguageIndex(langIndex));
			Contract.Requires(IsValidGameIndex(gameIndex));

			InitializeHandle(out mHandle, buildHandle, langIndex, gameIndex);
		}
		#endregion

		#region Value properties
		/// <summary>The handle to the game build this info specifically associates with</summary>
		[Contracts.Pure]
		public Engine.EngineBuildHandle Build { get {
			return new Engine.EngineBuildHandle(mHandle, Constants.kBuildShift);
		} }
		/// <summary>Index of a language registered in the <see cref="LanguageRegistry"/></summary>
		[Contracts.Pure]
		public int LanguageIndex { get {
			return LanguageRegistry.BitDecodeLanguageIndex(mHandle, Constants.kLanguageIndexShift);
		} }
		/// <summary>Is the language supported by <see cref="Build"/>?</summary>
		[Contracts.Pure]
		public bool IsSupported { get {
			return 1 == Bits.BitDecode(mHandle, Constants.kIsSupportedShift, Constants.kIsSupportedMask);
		} }
		/// <summary>Is the language unsupported by <see cref="Build"/>?</summary>
		[Contracts.Pure]
		public bool IsUnsupported { get {
			return 0 == Bits.BitDecode(mHandle, Constants.kIsSupportedShift, Constants.kIsSupportedMask);
		} }
		/// <summary>The index <see cref="LanguageIndex"/> maps to in <see cref="Build"/></summary>
		[Contracts.Pure]
		public int GameIndex { get {
			return Bits.BitDecodeNoneable(mHandle, Constants.kGameIndexShift, Constants.kGameIndexBitMask);
		} }

		[Contracts.Pure]
		public string LanguageName { get {
			Contract.Ensures(Contract.Result<string>() != null);

			int lang_index = LanguageIndex;

			return lang_index.IsNone()
				? LanguageRegistry.kNoneName
				: LanguageRegistry.LanguageNames[lang_index];
		} }
		#endregion

		[Contracts.Pure]
		public bool IsNone { get {
			// this only works because ALL bitfields are NONE encoded, meaning -1 values are encoded as 0
			return mHandle == 0;
		} }

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is GameLanguageHandle)
				return this.mHandle == ((GameLanguageHandle)obj).mHandle;

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
			Contract.Ensures(Contract.Result<string>() != null);

			return LanguageName;
		}
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
			GameLanguageHandle _x; KSoft.Debug.TypeCheck.CastValue(x, out _x);
			GameLanguageHandle _y; KSoft.Debug.TypeCheck.CastValue(y, out _y);

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
			GameLanguageHandle _obj; KSoft.Debug.TypeCheck.CastValue(obj, out _obj);

			return GameLanguageHandle.StaticCompare(this, _obj);
		}
		#endregion

		#region IEquatable<GameLanguageHandle> Members
		/// <summary>See <see cref="IEquatable{T}.Equals"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(GameLanguageHandle other)
		{
			return this.mHandle == other.mHandle;
		}
		#endregion

		#region Util
		static int StaticCompare(GameLanguageHandle lhs, GameLanguageHandle rhs)
		{
			Contract.Assert(GameLanguageHandle.BitCount < Bits.kInt32BitCount,
				"Handle bits needs to be <= 31 (ie, sans sign bit) in order for this implementation of CompareTo to reasonably work");

			int lhs_data = (int)lhs.mHandle;
			int rhs_data = (int)rhs.mHandle;
			int result = lhs_data - rhs_data;

			return result;
		}
		#endregion
	};
}