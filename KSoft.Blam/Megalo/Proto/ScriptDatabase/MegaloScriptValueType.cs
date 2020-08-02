using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Interop = System.Runtime.InteropServices;

namespace KSoft.Blam.Megalo.Proto
{
	using BitFieldTraits = Bitwise.BitFieldTraits;
	using BitEncoders = TypeExtensionsBlam.BitEncoders;

	[System.Reflection.Obfuscation(Exclude=false)]
	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	[System.Diagnostics.DebuggerDisplay("Name = {NameIndex}, Base = {BaseType}, BitLength = {BitLength}")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct MegaloScriptValueType
		: IComparable<MegaloScriptValueType>, IComparable
		, IEquatable<MegaloScriptValueType>
		, IEqualityComparer<MegaloScriptValueType>
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
				new BitFieldTraits(kBitCountNameIndex);
			public static readonly BitFieldTraits kBaseTypeBitField =
				new BitFieldTraits(BitEncoders.MegaloScriptValueBaseType.BitCountTrait, kNameIndexBitField);
			public static readonly BitFieldTraits kBitLengthBitField =
				new BitFieldTraits(kBitCountBitLength, kBaseTypeBitField);
			public static readonly BitFieldTraits kTypeParamBitField =
				new BitFieldTraits(EncoderTraitsChoice.TypeParam.BitCountTrait, kBitLengthBitField);
			public static readonly BitFieldTraits kTypeTraitsBitField =
				new BitFieldTraits(EncoderTraitsChoice.TypeTraits.BitCountTrait, kTypeParamBitField);

			public static readonly BitFieldTraits kLastBitField =
				kTypeTraitsBitField;
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		/// <remarks>26 bits at last count</remarks>
		public static int BitCount { get { return Constants.kLastBitField.FieldsBitCount; } }
		public static uint Bitmask { get { return Constants.kLastBitField.FieldsBitmask.u32; } }

		/// <remarks>ONLY PUBLIC FOR USE IN CODE CONTRACTS</remarks>
		[Contracts.Pure] public static bool ValidateNameIndex(int index)	{ return index >= 0 && index <= kMaxNameIndex; }
		/// <remarks>ONLY PUBLIC FOR USE IN CODE CONTRACTS</remarks>
		[Contracts.Pure] public static bool ValidateBitLength(int length)	{ return length >= 0 && length <= kMaxBitLength; }
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

			Contract.Assert(encoder.UsedBitCount == MegaloScriptValueType.BitCount);

			handle = encoder.GetHandle32();
		}
		#endregion

		#region Ctor
		public MegaloScriptValueType(int nameIndex, MegaloScriptValueBaseType baseType, int bitLength,
			uint typeParam = 0, uint typeTraits = 0) : this()
		{
			Contract.Requires(ValidateNameIndex(nameIndex));
			Contract.Requires(	baseType != MegaloScriptValueBaseType.Enum &&
								baseType != MegaloScriptValueBaseType.Index &&
								baseType != MegaloScriptValueBaseType.VarReference);
			Contract.Requires(ValidateBitLength(bitLength));

			InitializeHandle(out mHandle, nameIndex, baseType, bitLength, typeParam, typeTraits);
		}
		#region MegaloScriptValueBaseType.Enum
		public MegaloScriptValueType(int nameIndex, int bitLength,
			int enumIndex, MegaloScriptValueEnumTraits enumTraits) : this()
		{
			Contract.Requires(ValidateNameIndex(nameIndex));
			Contract.Requires(ValidateBitLength(bitLength));

			InitializeHandle(out mHandle, nameIndex, MegaloScriptValueBaseType.Enum, bitLength, (uint)enumIndex, (uint)enumTraits);
		}
		#endregion
		#region MegaloScriptValueBaseType.Index
		public MegaloScriptValueType(int nameIndex, int bitLength,
			MegaloScriptValueIndexTarget indexTarget, MegaloScriptValueIndexTraits indexTraits) : this()
		{
			Contract.Requires(ValidateNameIndex(nameIndex));
			Contract.Requires(ValidateBitLength(bitLength));

			InitializeHandle(out mHandle, nameIndex, MegaloScriptValueBaseType.Index, bitLength, (uint)indexTarget, (uint)indexTraits);
		}
		#endregion
		#region MegaloScriptValueBaseType.Var
		public MegaloScriptValueType(int nameIndex, int bitLength,
			MegaloScriptVariableType varType, MegaloScriptVariableSet varSet) : this()
		{
			Contract.Requires(ValidateNameIndex(nameIndex));
			Contract.Requires(ValidateBitLength(bitLength));

			InitializeHandle(out mHandle, nameIndex, MegaloScriptValueBaseType.Var, bitLength, (uint)varType, (uint)varSet);
		}
		#endregion
		#region MegaloScriptValueBaseType.VarReference
		public MegaloScriptValueType(int nameIndex,
			MegaloScriptVarReferenceType varRefType, MegaloScriptValueBaseType baseType = MegaloScriptValueBaseType.VarReference) : this()
		{
			Contract.Requires(ValidateNameIndex(nameIndex));

			InitializeHandle(out mHandle, nameIndex, baseType, 0, (uint)varRefType);
		}
		#endregion
		#endregion

		#region Value properties
		public int NameIndex						{ get { return (int)Bits.BitDecode(mHandle, Constants.kNameIndexBitField); } }
		public MegaloScriptValueBaseType BaseType	{ get { return BitEncoders.MegaloScriptValueBaseType.BitDecode(mHandle, Constants.kBaseTypeBitField.BitIndex); } }
		public int BitLength						{ get { return (int)Bits.BitDecode(mHandle, Constants.kBitLengthBitField); } }
		public uint TypeParam						{ get { return Bits.BitDecode(mHandle, Constants.kTypeParamBitField); } }
		public uint TypeTraits						{ get { return Bits.BitDecode(mHandle, Constants.kTypeTraitsBitField); } }

		#region Type Parameter/Traits interfaces
		/// <summary><see cref="MegaloScriptValueBaseType.Single"/></summary>
		public int EncodingIndex						{ get { return (int)TypeParam - 1; } }

		/// <summary><see cref="MegaloScriptValueBaseType.Point3d"/></summary>
		public bool PointIsSigned						{ get { return TypeTraits != 0; } }

		#region MegaloScriptValueBaseType.Enum and Flags
		/// <summary><see cref="MegaloScriptValueBaseType.Enum"/> and <see cref="MegaloScriptValueBaseType.Flags"/></summary>
		public int EnumIndex							{ get { return (int)TypeParam; } }
		/// <summary><see cref="MegaloScriptValueBaseType.Enum"/> and <see cref="MegaloScriptValueBaseType.Flags"/></summary>
		public MegaloScriptValueEnumTraits EnumTraits	{ get { return BitEncoders.MegaloScriptValueEnumTraits.BitDecode(mHandle, Constants.kTypeTraitsBitField.BitIndex); } }
		#endregion
		#region MegaloScriptValueBaseType.Index
		/// <summary><see cref="MegaloScriptValueBaseType.Index"/></summary>
		public MegaloScriptValueIndexTarget IndexTarget { get { return BitEncoders.MegaloScriptValueIndexTarget.BitDecode(mHandle, Constants.kTypeParamBitField.BitIndex); } }
		/// <summary><see cref="MegaloScriptValueBaseType.Index"/></summary>
		public MegaloScriptValueIndexTraits IndexTraits	{ get { return BitEncoders.MegaloScriptValueIndexTraits.BitDecode(mHandle, Constants.kTypeTraitsBitField.BitIndex); } }
		#endregion
		#region MegaloScriptValueBaseType.Var
		/// <summary><see cref="MegaloScriptValueBaseType.Var"/></summary>
		public MegaloScriptVariableType VarType			{ get { return BitEncoders.MegaloScriptVariableType.BitDecode(mHandle, Constants.kTypeParamBitField.BitIndex); } }
		/// <summary><see cref="MegaloScriptValueBaseType.Var"/></summary>
		public MegaloScriptVariableSet VarSet			{ get { return BitEncoders.MegaloScriptVariableSet.BitDecode(mHandle, Constants.kTypeTraitsBitField.BitIndex); } }
		#endregion

		/// <summary><see cref="MegaloScriptValueBaseType.VarReference"/></summary>
		public MegaloScriptVarReferenceType VarReference{ get { return BitEncoders.MegaloScriptVarReferenceType.BitDecode(mHandle, Constants.kTypeParamBitField.BitIndex); } }

		/// <summary><see cref="MegaloScriptValueBaseType.Tokens"/></summary>
		public int MaxTokens							{ get { return (int)TypeParam; } }
		#endregion
		#endregion

		#region Overrides
		public override bool Equals(object obj)
		{
			if (obj is MegaloScriptValueType)
				return this.mHandle == ((MegaloScriptValueType)obj).mHandle;

			return false;
		}
		public override int GetHashCode() { return (int)mHandle; }
		#endregion

		#region Operators
		[Contracts.Pure]
		public static bool operator ==(MegaloScriptValueType lhs, MegaloScriptValueType rhs)	{ return lhs.mHandle == rhs.mHandle; }
		[Contracts.Pure]
		public static bool operator !=(MegaloScriptValueType lhs, MegaloScriptValueType rhs)	{ return lhs.mHandle != rhs.mHandle; }
		#endregion

		#region IComparable<MegaloScriptValueType> Members
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(MegaloScriptValueType other)
		{
			return MegaloScriptValueType.StaticCompare(this, other);
		}
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		int IComparable.CompareTo(object obj)
		{
			MegaloScriptValueType _obj; KSoft.Debug.TypeCheck.CastValue(obj, out _obj);

			return MegaloScriptValueType.StaticCompare(this, _obj);
		}
		#endregion

		#region Equality Members
		public bool Equals(MegaloScriptValueType x, MegaloScriptValueType y) { return x.mHandle == y.mHandle; }

		public bool Equals(MegaloScriptValueType other) { return Equals(this, other); }

		public int GetHashCode(MegaloScriptValueType obj) { return obj.GetHashCode(); }
		#endregion

		#region Util
		static int StaticCompare(MegaloScriptValueType lhs, MegaloScriptValueType rhs)
		{
			Contract.Assert(MegaloScriptValueType.BitCount < Bits.kInt32BitCount,
				"Handle bits needs to be <= 31 (ie, sans sign bit) in order for this implementation of CompareTo to reasonably work");

			int lhs_data = (int)lhs.mHandle;
			int rhs_data = (int)rhs.mHandle;
			int result = lhs_data - rhs_data;

			return result;
		}
		#endregion
	};
}
