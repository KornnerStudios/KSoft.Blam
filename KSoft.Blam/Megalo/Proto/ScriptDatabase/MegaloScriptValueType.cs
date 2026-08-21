using System;
using System.Collections.Generic;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Blam.Megalo.Proto
{
	using BitFieldTraits = Bitwise.BitFieldTraits;
	using BitEncoders = TypeExtensionsBlam.BitEncoders;

	[System.Reflection.Obfuscation(Exclude=false)]
	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	[System.Diagnostics.DebuggerDisplay("Name = {NameIndex}, Base = {BaseType}, BitLength = {BitLength}")]
	public struct MegaloScriptValueType
		: IComparable<MegaloScriptValueType>, IComparable
		, IEquatable<MegaloScriptValueType>
		, IEqualityComparer<MegaloScriptValueType> // #REMOVE_BLAM
	{
		#region Constants
		static class EncoderTraitsChoice
		{
			public static readonly IEnumBitEncoder<uint> TypeParam;
			public static readonly IEnumBitEncoder<uint> TypeTraits;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
			static EncoderTraitsChoice()
			{
				TypeParam = Util.MaxChoice(
					BitEncoders.MegaloScriptVarReferenceType as IEnumBitEncoder<uint>,
					BitEncoders.MegaloScriptValueIndexTarget as IEnumBitEncoder<uint>,
					x => x.BitCountTrait);
				TypeParam = Util.MaxChoice(
					TypeParam,
					BitEncoders.MegaloScriptVariableType as IEnumBitEncoder<uint>,
					x => x.BitCountTrait);

				TypeTraits = Util.MaxChoice(
					BitEncoders.MegaloScriptValueIndexTraits as IEnumBitEncoder<uint>,
					BitEncoders.MegaloScriptValueEnumTraits as IEnumBitEncoder<uint>,
					x => x.BitCountTrait);
				TypeTraits = Util.MaxChoice(
					TypeTraits,
					BitEncoders.MegaloScriptVariableSet as IEnumBitEncoder<uint>,
					x => x.BitCountTrait);
			}
		};

		const int kBitCountNameIndex =				7;
		const int kMaxNameIndex =					127;

		// If we wanted to save a bit, we could internally store the bit length as base zero
		const int kBitCountBitLength =				7;
		const uint kMaxBitLength =					64;

		// nesting these into a static class makes them run before the struct's static ctor...
		// which, being a value type cctor, may not run when we want it
		static class Constants
		{
			public static readonly BitFieldTraits kNameIndexBitField =
				new(kBitCountNameIndex);
			public static readonly BitFieldTraits kBaseTypeBitField =
				new(BitEncoders.MegaloScriptValueBaseType.BitCountTrait, kNameIndexBitField);
			public static readonly BitFieldTraits kBitLengthBitField =
				new(kBitCountBitLength, kBaseTypeBitField);
			public static readonly BitFieldTraits kTypeParamBitField =
				new(EncoderTraitsChoice.TypeParam.BitCountTrait, kBitLengthBitField);
			public static readonly BitFieldTraits kTypeTraitsBitField =
				new(EncoderTraitsChoice.TypeTraits.BitCountTrait, kTypeParamBitField);

			public static readonly BitFieldTraits kLastBitField =
				kTypeTraitsBitField;
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		/// <remarks>26 bits at last count</remarks>
		public static int BitCount => Constants.kLastBitField.FieldsBitCount;
		public static uint Bitmask => Constants.kLastBitField.FieldsBitmask.u32;

		public static bool ValidateNameIndex(int index) => index >= 0 && index <= kMaxNameIndex;
		public static bool ValidateBitLength(int length) => length >= 0 && length <= kMaxBitLength;
		#endregion

		#region Internal Value
		// #TODO_BLAM: Reorder so that MSB->LSB goes BaseType->TypeParam->TypeTraits->BitLength->NameIndex
		// NameIndex
		// BaseType : MegaloScriptValueBaseType
		// BitLength
		// TypeParameter : MegaloScriptVarReferenceType, MegaloScriptValueIndexTarget
		// TypeTraits : MegaloScriptValueIndexTraits
		[Interop.FieldOffset(0)] readonly uint mHandle;

		//internal uint Handle { get { return mHandle; } }

		static void InitializeHandle(out uint handle, int nameIndex, MegaloScriptValueBaseType baseType, int bitLength,
			uint typeParam = 0, uint typeTraits = 0)
		{
			var encoder = new Bitwise.HandleBitEncoder();
			encoder.Encode32((uint)nameIndex, Constants.kNameIndexBitField);
			encoder.Encode32(baseType, BitEncoders.MegaloScriptValueBaseType);
			encoder.Encode32((uint)bitLength, Constants.kBitLengthBitField);
			encoder.Encode32(typeParam, Constants.kTypeParamBitField);
			encoder.Encode32(typeTraits, Constants.kTypeTraitsBitField);

			if (encoder.UsedBitCount != MegaloScriptValueType.BitCount)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Encoded value-type handle used {0} bits, expected {1}.",
					encoder.UsedBitCount,
					MegaloScriptValueType.BitCount));
			}

			handle = encoder.GetHandle32();
		}

		static void ThrowIfInvalidNameIndex(int nameIndex)
		{
			if (!ValidateNameIndex(nameIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(nameIndex), nameIndex,
					string.Format(Util.InvariantCultureInfo, "Name index must be between 0 and {0}.", kMaxNameIndex));
			}
		}

		static void ThrowIfInvalidBitLength(int bitLength)
		{
			if (!ValidateBitLength(bitLength))
			{
				throw new ArgumentOutOfRangeException(nameof(bitLength), bitLength,
					string.Format(Util.InvariantCultureInfo, "Bit length must be between 0 and {0}.", kMaxBitLength));
			}
		}

		static void ThrowIfDisallowedGeneralBaseType(MegaloScriptValueBaseType baseType)
		{
			if (baseType == MegaloScriptValueBaseType.Enum ||
				baseType == MegaloScriptValueBaseType.Index ||
				baseType == MegaloScriptValueBaseType.VarReference)
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Base type {0} must use its specialized constructor.",
					baseType), nameof(baseType));
			}
		}
		#endregion

		#region Ctor
		public MegaloScriptValueType(int nameIndex, MegaloScriptValueBaseType baseType, int bitLength,
			uint typeParam = 0, uint typeTraits = 0) : this()
		{
			ThrowIfInvalidNameIndex(nameIndex);
			ThrowIfDisallowedGeneralBaseType(baseType);
			ThrowIfInvalidBitLength(bitLength);

			InitializeHandle(out mHandle, nameIndex, baseType, bitLength, typeParam, typeTraits);
		}
		#region MegaloScriptValueBaseType.Enum
		public MegaloScriptValueType(int nameIndex, int bitLength,
			int enumIndex, MegaloScriptValueEnumTraits enumTraits) : this()
		{
			ThrowIfInvalidNameIndex(nameIndex);
			ThrowIfInvalidBitLength(bitLength);

			InitializeHandle(out mHandle, nameIndex, MegaloScriptValueBaseType.Enum, bitLength, (uint)enumIndex, (uint)enumTraits);
		}
		#endregion
		#region MegaloScriptValueBaseType.Index
		public MegaloScriptValueType(int nameIndex, int bitLength,
			MegaloScriptValueIndexTarget indexTarget, MegaloScriptValueIndexTraits indexTraits) : this()
		{
			ThrowIfInvalidNameIndex(nameIndex);
			ThrowIfInvalidBitLength(bitLength);

			InitializeHandle(out mHandle, nameIndex, MegaloScriptValueBaseType.Index, bitLength, (uint)indexTarget, (uint)indexTraits);
		}
		#endregion
		#region MegaloScriptValueBaseType.Var
		public MegaloScriptValueType(int nameIndex, int bitLength,
			MegaloScriptVariableType varType, MegaloScriptVariableSet varSet) : this()
		{
			ThrowIfInvalidNameIndex(nameIndex);
			ThrowIfInvalidBitLength(bitLength);

			InitializeHandle(out mHandle, nameIndex, MegaloScriptValueBaseType.Var, bitLength, (uint)varType, (uint)varSet);
		}
		#endregion
		#region MegaloScriptValueBaseType.VarReference
		public MegaloScriptValueType(int nameIndex,
			MegaloScriptVarReferenceType varRefType, MegaloScriptValueBaseType baseType = MegaloScriptValueBaseType.VarReference) : this()
		{
			ThrowIfInvalidNameIndex(nameIndex);

			InitializeHandle(out mHandle, nameIndex, baseType, 0, (uint)varRefType);
		}
		#endregion
		#endregion

		#region Value properties
		public readonly int NameIndex =>		(int)Bits.BitDecode(mHandle, Constants.kNameIndexBitField);
		public readonly MegaloScriptValueBaseType BaseType => BitEncoders.MegaloScriptValueBaseType.BitDecode(mHandle, Constants.kBaseTypeBitField.BitIndex);
		public readonly int BitLength =>		(int)Bits.BitDecode(mHandle, Constants.kBitLengthBitField);
		public readonly uint TypeParam =>		Bits.BitDecode(mHandle, Constants.kTypeParamBitField);
		public readonly uint TypeTraits =>		Bits.BitDecode(mHandle, Constants.kTypeTraitsBitField);

		#region Type Parameter/Traits interfaces
		/// <summary><see cref="MegaloScriptValueBaseType.Single"/></summary>
		public readonly int EncodingIndex =>	(int)TypeParam - 1;

		/// <summary><see cref="MegaloScriptValueBaseType.Point3d"/></summary>
		public readonly bool PointIsSigned =>	TypeTraits != 0;

		#region MegaloScriptValueBaseType.Enum and Flags
		/// <summary><see cref="MegaloScriptValueBaseType.Enum"/> and <see cref="MegaloScriptValueBaseType.Flags"/></summary>
		public readonly int EnumIndex =>							(int)TypeParam;
		/// <summary><see cref="MegaloScriptValueBaseType.Enum"/> and <see cref="MegaloScriptValueBaseType.Flags"/></summary>
		public readonly MegaloScriptValueEnumTraits EnumTraits =>	BitEncoders.MegaloScriptValueEnumTraits.BitDecode(mHandle, Constants.kTypeTraitsBitField.BitIndex);
		#endregion
		#region MegaloScriptValueBaseType.Index
		/// <summary><see cref="MegaloScriptValueBaseType.Index"/></summary>
		public readonly MegaloScriptValueIndexTarget IndexTarget => BitEncoders.MegaloScriptValueIndexTarget.BitDecode(mHandle, Constants.kTypeParamBitField.BitIndex);
		/// <summary><see cref="MegaloScriptValueBaseType.Index"/></summary>
		public readonly MegaloScriptValueIndexTraits IndexTraits => BitEncoders.MegaloScriptValueIndexTraits.BitDecode(mHandle, Constants.kTypeTraitsBitField.BitIndex);
		#endregion
		#region MegaloScriptValueBaseType.Var
		/// <summary><see cref="MegaloScriptValueBaseType.Var"/></summary>
		public readonly MegaloScriptVariableType VarType =>	BitEncoders.MegaloScriptVariableType.BitDecode(mHandle, Constants.kTypeParamBitField.BitIndex);
		/// <summary><see cref="MegaloScriptValueBaseType.Var"/></summary>
		public readonly MegaloScriptVariableSet VarSet =>	BitEncoders.MegaloScriptVariableSet.BitDecode(mHandle, Constants.kTypeTraitsBitField.BitIndex);
		#endregion

		/// <summary><see cref="MegaloScriptValueBaseType.VarReference"/></summary>
		public readonly MegaloScriptVarReferenceType VarReference => BitEncoders.MegaloScriptVarReferenceType.BitDecode(mHandle, Constants.kTypeParamBitField.BitIndex);

		/// <summary><see cref="MegaloScriptValueBaseType.Tokens"/></summary>
		public readonly int MaxTokens =>		(int)TypeParam;
		#endregion
		#endregion

		#region Overrides
		public override readonly bool Equals(object obj)
		{
			if (obj is MegaloScriptValueType objValueType)
			{
				return this.mHandle == objValueType.mHandle;
			}

			return false;
		}
		public override readonly int GetHashCode() => (int)mHandle;
		#endregion

		#region Operators
		public static bool operator ==(MegaloScriptValueType lhs, MegaloScriptValueType rhs) => lhs.mHandle == rhs.mHandle;
		public static bool operator !=(MegaloScriptValueType lhs, MegaloScriptValueType rhs) => lhs.mHandle != rhs.mHandle;
		#endregion

		#region IComparable<MegaloScriptValueType> Members
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public readonly int CompareTo(MegaloScriptValueType other)
		{
			return MegaloScriptValueType.StaticCompare(this, other);
		}
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		readonly int IComparable.CompareTo(object obj)
		{
			KSoft.Debug.TypeCheck.CastValue(obj, out MegaloScriptValueType _obj);

			return MegaloScriptValueType.StaticCompare(this, _obj);
		}
		#endregion

		#region Equality Members
		public readonly bool Equals(MegaloScriptValueType x, MegaloScriptValueType y) => x.mHandle == y.mHandle;

		public readonly bool Equals(MegaloScriptValueType other) => Equals(this, other);

		public readonly int GetHashCode(MegaloScriptValueType obj) => obj.GetHashCode();
		#endregion

		#region Util
		static int StaticCompare(MegaloScriptValueType lhs, MegaloScriptValueType rhs)
		{
			// #TODO figure out a a utility to do this generically for bit-encoded handles that can run
			// in the internal Constants class.
			if (MegaloScriptValueType.BitCount >= Bits.kInt32BitCount)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Handle bit count is {0}, expected less than {1} for signed CompareTo.",
					MegaloScriptValueType.BitCount,
					Bits.kInt32BitCount));
			}

			int lhs_data = (int)lhs.mHandle;
			int rhs_data = (int)rhs.mHandle;
			int result = lhs_data - rhs_data;

			return result;
		}
		#endregion
	};
}
