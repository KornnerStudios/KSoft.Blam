using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam
{
	using MegaloModel = Blam.Megalo.Model;
	using MegaloProto = Blam.Megalo.Proto;

	public static partial class TypeExtensionsBlam
	{
		internal const int kTagStringLength = 31;
		internal static readonly Memory.Strings.StringStorage kTagStringStorage = new Memory.Strings.StringStorage(
			Memory.Strings.StringStorageWidthType.Ascii, Memory.Strings.StringStorageType.CharArray, kTagStringLength+1);
		internal static readonly Text.StringStorageEncoding kTagStringEncoding = new Text.StringStorageEncoding(
			kTagStringStorage);

		#region Enum Bit Encoders
		public static class BitEncoders
		{
			// KSoft.Blam.Engine
			public static readonly EnumBitEncoder32<Engine.EngineGeneration>
				EngineGeneration = new EnumBitEncoder32<Engine.EngineGeneration>();
			public static readonly EnumBitEncoder32<Engine.EngineProductionStage>
				EngineProductionStage = new EnumBitEncoder32<Engine.EngineProductionStage>();

			internal static readonly EnumBitEncoder32<Megalo.MegaloScriptVariableType>
				MegaloScriptVariableType = new EnumBitEncoder32<Megalo.MegaloScriptVariableType>();
			internal static readonly EnumBitEncoder32<Megalo.MegaloScriptVariableSet>
				MegaloScriptVariableSet = new EnumBitEncoder32<Megalo.MegaloScriptVariableSet>();

			#region KSoft.Blam.Megalo.Model
			internal static readonly EnumBitEncoder32<MegaloModel.MegaloScriptModelObjectType>
				MegaloScriptModelObjectType = new EnumBitEncoder32<MegaloModel.MegaloScriptModelObjectType>();
			#endregion

			#region KSoft.Blam.Megalo.Proto
			internal static readonly EnumBitEncoder32<MegaloProto.MegaloScriptVarReferenceType>
				MegaloScriptVarReferenceType = new EnumBitEncoder32<MegaloProto.MegaloScriptVarReferenceType>();

			internal static readonly EnumBitEncoder32<MegaloProto.MegaloScriptValueIndexTarget>
				MegaloScriptValueIndexTarget = new EnumBitEncoder32<MegaloProto.MegaloScriptValueIndexTarget>();
			internal static readonly EnumBitEncoder32<MegaloProto.MegaloScriptValueIndexTraits>
				MegaloScriptValueIndexTraits = new EnumBitEncoder32<MegaloProto.MegaloScriptValueIndexTraits>();

			internal static readonly EnumBitEncoder32<MegaloProto.MegaloScriptValueBaseType>
				MegaloScriptValueBaseType = new EnumBitEncoder32<MegaloProto.MegaloScriptValueBaseType>();

			internal static readonly EnumBitEncoder32<MegaloProto.MegaloScriptValueEnumTraits>
				MegaloScriptValueEnumTraits = new EnumBitEncoder32<MegaloProto.MegaloScriptValueEnumTraits>();
			#endregion
		};

		public static IEnumBitEncoder<uint> GetBitEncoder(this Engine.EngineGeneration value)
		{ return BitEncoders.EngineGeneration; }
		public static IEnumBitEncoder<uint> GetBitEncoder(this Engine.EngineProductionStage value)
		{ return BitEncoders.EngineProductionStage; }
		#endregion

		#region Blob
		public static int GetDataSize(this Blob.Transport.BlobTransportStreamAuthentication type)
		{
			switch (type)
			{
				case Blob.Transport.BlobTransportStreamAuthentication.None:
					return 0;

				case Blob.Transport.BlobTransportStreamAuthentication.Crc:
					return sizeof(uint);

				case Blob.Transport.BlobTransportStreamAuthentication.Hash:
				case Blob.Transport.BlobTransportStreamAuthentication.Rsa:
					return 0x100;

				default:
					throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
		#endregion

		internal const int IndexOfByPropertyNotFoundResult = int.MinValue;
		internal static int IndexOfByProperty<T, TProp>(this IList<T> list, TProp value, Func<T, TProp> property
			, Func<TProp, string> generateNotFoundMessage = null)
			where TProp : IEquatable<TProp>
		{
			Contract.Requires(property != null);

			for (int x = 0; x < list.Count; x++)
				if (property(list[x]).Equals(value))
					return x;

			if (generateNotFoundMessage == null)
				return IndexOfByPropertyNotFoundResult;
			else
				throw new KeyNotFoundException(generateNotFoundMessage(value));
		}

		#region IO.BitStream
		#region Single
		// interesting: http://stackoverflow.com/a/3542975/444977

		public static float DecodeSingle(uint rawBits, float min, float max, int bitCount, bool signed, bool unknown)
		{
			int r10 = 1 << bitCount;
			if (signed)
				r10 -= 1;

			float fp1;
			if (unknown)
			{
				if (rawBits == 0)
					fp1 = min;
				else if (rawBits == (r10 - 1))
					fp1 = max;
				else
				{
					uint r6 = rawBits - 1;
					float fp12 = max - min;
					int r8 = r10 - 2;

					float fp6 = (float)r6;
					float fp7 = (float)r8;

					float fp5 = fp12 / fp7;
					float fp4 = fp5 * 0.5f;
					float fp3 = (fp6 * fp5) + fp4;

					fp1 = fp3 + min;
				}
			}
			else
			{
				float fp12 = max - min;

				float fp5 = (float)rawBits;
				float fp8 = (float)r10;

				float fp6 = fp12 / fp8;
				float fp4 = fp6 * 0.5f;
				float fp3 = (fp5 * fp6) + fp4;

				fp1 = fp3 + min;
			}

			if (signed)
			{
				r10 -= 1;
				uint r8 = rawBits << 1;
				if (r8 == r10)
				{
					float fp13 = min + max;
					fp1 = (float)(fp13 * 0.5);
				}
			}

			return fp1;
		}
		public static uint EncodeSingle(float real, float min, float max, int bitCount, bool signed, bool unknown)
		{
			uint r11 = 1U << bitCount;
			if (signed)
				r11 -= 1;

			uint r10;
			if (unknown)
			{
				if (real == min)
					r11 = 0;
				else if (real == max)
					r11 -= 1;
				else
				{
					r11 -= 2;
					float fp0 = max - min;
					float fp13 = real - min;
					float fp9 = fp0 / (float)r11;
					float fp8 = fp13 / fp9;

					r10 = (uint)fp8;
					r10 += 1;
					if (r10 < 1)
						r10 = 1;

					if (r10 < r11)
						r11 = r10;
				}
			}
			else
			{
				float fp0 = max - min;
				float fp13 = real - min;
				float fp9 = fp0 / (float)r11;
				float fp8 = fp13 / fp9;
				r11 -= 1;

				r10 = (uint)fp8;

				uint r6 = (r10 >> 31); // get the sign bit
				uint r5 = r6 - 1;
				//int r6 = r10 < 0 ? 1 : 0;
				//int r5 = -1 + r6;
				r10 = r5 & r10;

				if (r10 < r11)
					r11 = r10;
			}

			return r11;
		}

		static uint Read(IO.BitStream s, out float real, float min, float max, int bitCount, bool signed, bool unknown)
		{
			uint raw_bits;
			s.Read(out raw_bits, bitCount);

			real = DecodeSingle(raw_bits, min, max, bitCount, signed, unknown);
			return raw_bits;
		}
		static void Write(IO.BitStream s, float real, float min, float max, int bitCount, bool signed, bool unknown)
		{
			uint raw_bits = EncodeSingle(real, min, max, bitCount, signed, unknown);

			s.Write(raw_bits, bitCount);
		}
		public static void Stream(this IO.BitStream s, ref float real, float min, float max, int bitCount, bool signed, bool unknown)
		{
			Contract.Requires(bitCount <= Bits.kInt32BitCount);

				 if (s.IsReading)	Read(s, out real, min, max, bitCount, signed, unknown);
			else if (s.IsWriting)	Write(s, real, min, max, bitCount, signed, unknown);
		}
		#endregion

		#region Stream index (Int32)
		/// <remarks>Used for indexes which *are* typically NONE (-1)</remarks>
		public static void StreamIndex(this IO.BitStream s, ref int value, int bitCount = Bits.kInt32BitCount)
		{
			Contract.Requires(bitCount <= Bits.kInt32BitCount);

			if (s.IsReading)
			{
				bool is_none = s.ReadBoolean();

				if (!is_none)
					s.Read(out value, bitCount);
				else
					value = TypeExtensions.kNone;
			}
			else if (s.IsWriting)
			{
				bool is_none = value.IsNone();
				s.Write(is_none);

				if (!is_none)
					s.Write(value, bitCount);
			}
		}
		/// <remarks>Used for indexes which *are not* typically NONE (-1)</remarks>
		public static void StreamIndexPos(this IO.BitStream s, ref int value, int bitCount = Bits.kInt32BitCount)
		{
			Contract.Requires(bitCount <= Bits.kInt32BitCount);

			if (s.IsReading)
			{
				bool not_none = s.ReadBoolean();

				if (not_none)
					s.Read(out value, bitCount);
				else
					value = TypeExtensions.kNone;
			}
			else if (s.IsWriting)
			{
				bool not_none = value.IsNotNone();
				s.Write(not_none);

				if (not_none)
					s.Write(value, bitCount);
			}
		}
		#endregion

		// #TODO_BLAM: T4
		#region Stream noneable
		/// <summary>Streams an integer which is >= -1, but when streamed out the value is added by 1 (so it will be >= 0)</summary>
		/// <param name="s"></param>
		/// <param name="value"></param>
		/// <param name="bitCount"></param>
		public static void StreamNoneable(this IO.BitStream s, ref sbyte value, int bitCount = Bits.kByteBitCount)
		{
			Contract.Requires(bitCount <= Bits.kByteBitCount);

			if (s.IsReading)
			{
				s.Read(out value, bitCount);
				value--;
			}
			else if (s.IsWriting)
			{
				Contract.Assert(value >= TypeExtensions.kNone);
				s.Write(value + 1, bitCount);
			}
		}
		/// <summary>Streams an integer which is >= -1, but when streamed out the value is added by 1 (so it will be >= 0)</summary>
		/// <param name="s"></param>
		/// <param name="value"></param>
		/// <param name="bitCount"></param>
		public static void StreamNoneable(this IO.BitStream s, ref short value, int bitCount = Bits.kInt16BitCount)
		{
			Contract.Requires(bitCount <= Bits.kInt16BitCount);

			if (s.IsReading)
			{
				s.Read(out value, bitCount);
				value--;
			}
			else if (s.IsWriting)
			{
				Contract.Assert(value >= TypeExtensions.kNone);
				s.Write(value + 1, bitCount);
			}
		}
		/// <summary>Streams an integer which is >= -1, but when streamed out the value is added by 1 (so it will be >= 0)</summary>
		/// <param name="s"></param>
		/// <param name="value"></param>
		/// <param name="bitCount"></param>
		public static void StreamNoneable(this IO.BitStream s, ref int value, int bitCount = Bits.kInt32BitCount)
		{
			Contract.Requires(bitCount <= Bits.kInt32BitCount);

			if (s.IsReading)
			{
				s.Read(out value, bitCount);
				value--;
			}
			else if (s.IsWriting)
			{
				Contract.Assert(value.IsNoneOrPositive());
				s.Write(value + 1, bitCount);
			}
		}
		#endregion
		#endregion

		#region None utils
		public static void StreamAttributeOptNoneOption<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s,
			string name, ref int value)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt(name, ref value, Predicates.IsNotNone) && s.IsReading)
				value = KSoft.TypeExtensions.kNone;
		}
		#endregion

		#region DefaultOption utils
		public const int kDefaultOption = -2;

		readonly static Predicate<int> kNotDefaultOption32 = x => x != kDefaultOption;

		public static void StreamAttributeOptDefaultOption<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s,
			string name, ref int value)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt(name, ref value, TypeExtensionsBlam.kNotDefaultOption32) && s.IsReading)
				value = TypeExtensionsBlam.kDefaultOption;
		}
		#endregion

		#region Unchanged utils
		public const int kUnchanged = -3;

		static readonly Predicate<sbyte> kNotUnchanged8 = x => x != kUnchanged;
		static readonly Predicate<int> kNotUnchanged32 = x => x != kUnchanged;

		public static void StreamAttributeOptUnchanged<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s,
			string name, ref sbyte value)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt(name, ref value, TypeExtensionsBlam.kNotUnchanged8) && s.IsReading)
				value = TypeExtensionsBlam.kUnchanged;
		}
		public static void StreamAttributeOptUnchanged<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s,
			string name, ref int value)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt(name, ref value, TypeExtensionsBlam.kNotUnchanged32) && s.IsReading)
				value = TypeExtensionsBlam.kUnchanged;
		}

		public static void StreamAttributeOptUnchangedZero<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s,
			string name, ref byte value)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt(name, ref value, Predicates.IsNotZero) && s.IsReading)
				value = 0;
		}
		public static void StreamAttributeOptUnchangedZero<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s,
			string name, ref int value)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt(name, ref value, Predicates.IsNotZero) && s.IsReading)
				value = 0;
		}
		#endregion

		#region Megalo
		[Contracts.Pure]
		internal static Megalo.MegaloScriptVariableType ToVariableType(
			this Megalo.MegaloScriptVariableReferenceType type)
		{
			switch (type)
			{
				case Megalo.MegaloScriptVariableReferenceType.	Custom:
					return Megalo.MegaloScriptVariableType.		Numeric;
				case Megalo.MegaloScriptVariableReferenceType.	Player:
					return Megalo.MegaloScriptVariableType.		Player;
				case Megalo.MegaloScriptVariableReferenceType.	Object:
					return Megalo.MegaloScriptVariableType.		Object;
				case Megalo.MegaloScriptVariableReferenceType.	Team:
					return Megalo.MegaloScriptVariableType.		Team;
				case Megalo.MegaloScriptVariableReferenceType.	Timer:
					return Megalo.MegaloScriptVariableType.		Timer;

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}

		[Contracts.Pure]
		internal static bool RequiresBitLength(this MegaloProto.MegaloScriptValueBaseType type)
		{
			switch (type)
			{
				case MegaloProto.MegaloScriptValueBaseType.Int:
				case MegaloProto.MegaloScriptValueBaseType.UInt:
				case MegaloProto.MegaloScriptValueBaseType.Enum:
					return true;

				default: return false;
			}
		}

		/// <summary>Is the target based in static (ie, tag) data?</summary>
		/// <param name="target"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool IsStaticData(this MegaloProto.MegaloScriptValueIndexTarget target)
		{
			switch (target)
			{
				case MegaloProto.MegaloScriptValueIndexTarget.ObjectType:
				case MegaloProto.MegaloScriptValueIndexTarget.Name:
				case MegaloProto.MegaloScriptValueIndexTarget.Sound:
				case MegaloProto.MegaloScriptValueIndexTarget.Incident:
				case MegaloProto.MegaloScriptValueIndexTarget.Icon:
				case MegaloProto.MegaloScriptValueIndexTarget.Medal:
				case MegaloProto.MegaloScriptValueIndexTarget.Ordnance:
					return true;

				default: return false;
			}
		}
		/// <summary>Is the target based in variant data?</summary>
		/// <param name="target"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool IsVariantData(this MegaloProto.MegaloScriptValueIndexTarget target)
		{
			switch (target)
			{
				case MegaloProto.MegaloScriptValueIndexTarget.LoadoutPalette:
				case MegaloProto.MegaloScriptValueIndexTarget.Option:
				case MegaloProto.MegaloScriptValueIndexTarget.String:
				case MegaloProto.MegaloScriptValueIndexTarget.PlayerTraits:
				case MegaloProto.MegaloScriptValueIndexTarget.Statistic:
				case MegaloProto.MegaloScriptValueIndexTarget.Widget:
				case MegaloProto.MegaloScriptValueIndexTarget.ObjectFilter:
				case MegaloProto.MegaloScriptValueIndexTarget.GameObjectFilter:
					return true;

				default: return false;
			}
		}
		/// <summary>Does the target have a human-friendly name?</summary>
		/// <param name="target"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool HasIndexName(this MegaloProto.MegaloScriptValueIndexTarget target)
		{
			switch (target)
			{
				case MegaloProto.MegaloScriptValueIndexTarget.ObjectType:
				case MegaloProto.MegaloScriptValueIndexTarget.Name:
				case MegaloProto.MegaloScriptValueIndexTarget.Sound:
				case MegaloProto.MegaloScriptValueIndexTarget.Incident:
//				case MegaloProto.MegaloScriptValueIndexTarget.Icon:
				case MegaloProto.MegaloScriptValueIndexTarget.Medal:
				case MegaloProto.MegaloScriptValueIndexTarget.Ordnance:

				case MegaloProto.MegaloScriptValueIndexTarget.Option:
				case MegaloProto.MegaloScriptValueIndexTarget.String:
				case MegaloProto.MegaloScriptValueIndexTarget.PlayerTraits:
				case MegaloProto.MegaloScriptValueIndexTarget.Statistic:
				case MegaloProto.MegaloScriptValueIndexTarget.Widget:
				case MegaloProto.MegaloScriptValueIndexTarget.ObjectFilter:
				case MegaloProto.MegaloScriptValueIndexTarget.GameObjectFilter:
					return true;

				default: return false;
			}
		}

		internal static bool UseConditionTypeNames(this MegaloModel.MegaloScriptModelTagElementStreamFlags flags)
		{
			const MegaloModel.MegaloScriptModelTagElementStreamFlags k_mask =
				MegaloModel.MegaloScriptModelTagElementStreamFlags.UseConditionTypeNames;
			return (flags & k_mask) == k_mask;
		}
		internal static bool UseActionTypeNames(this MegaloModel.MegaloScriptModelTagElementStreamFlags flags)
		{
			const MegaloModel.MegaloScriptModelTagElementStreamFlags k_mask =
				MegaloModel.MegaloScriptModelTagElementStreamFlags.UseActionTypeNames;
			return (flags & k_mask) == k_mask;
		}
		internal static bool EmbedObjects(this MegaloModel.MegaloScriptModelTagElementStreamFlags flags)
		{
			const MegaloModel.MegaloScriptModelTagElementStreamFlags k_mask =
				MegaloModel.MegaloScriptModelTagElementStreamFlags.EmbedObjects;
			return (flags & k_mask) == k_mask;
		}
		internal static bool EmbedObjectsWriteSansIds(this MegaloModel.MegaloScriptModelTagElementStreamFlags flags)
		{
			const MegaloModel.MegaloScriptModelTagElementStreamFlags k_mask =
				MegaloModel.MegaloScriptModelTagElementStreamFlags.EmbedObjects |
				MegaloModel.MegaloScriptModelTagElementStreamFlags.EmbedObjectsWriteSansIds;
			return (flags & k_mask) == k_mask;
		}
		internal static bool HasParamFlags(this MegaloModel.MegaloScriptModelTagElementStreamFlags flags)
		{
			const MegaloModel.MegaloScriptModelTagElementStreamFlags k_mask =
				MegaloModel.MegaloScriptModelTagElementStreamFlags.kParamsMask;

			return (flags & k_mask) != 0;
		}

		public static bool IsUpdatedOnGameTick(this Megalo.MegaloScriptTriggerType type)
		{
			return	type == Megalo.MegaloScriptTriggerType.Normal ||
					type == Megalo.MegaloScriptTriggerType.Local;
		}

		[Contracts.Pure]
		internal static Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach ToHaloReach(
			this Megalo.MegaloScriptTokenAbstractType type)
		{
			switch (type)
			{
				case Megalo.MegaloScriptTokenAbstractType.None:
					return Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach.None;
				case Megalo.MegaloScriptTokenAbstractType.Player:
					return Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach.AbsolutePlayerIndex;
				case Megalo.MegaloScriptTokenAbstractType.Team:
					return Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach.TeamDesignator;
				case Megalo.MegaloScriptTokenAbstractType.Object:
					return Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach.Object;
				case Megalo.MegaloScriptTokenAbstractType.Numeric:
					return Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach.Numeric;
				case Megalo.MegaloScriptTokenAbstractType.SignedNumeric:
					throw new NotSupportedException(type.ToString());
				case Megalo.MegaloScriptTokenAbstractType.Timer:
					return Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach.TimerSeconds;

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
		[Contracts.Pure]
		internal static Megalo.MegaloScriptTokenAbstractType ToAbstract(
			this Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach type)
		{
			switch (type)
			{
				case Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach.None:
					return Megalo.MegaloScriptTokenAbstractType.None;
				case Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach.AbsolutePlayerIndex:
					return Megalo.MegaloScriptTokenAbstractType.Player;
				case Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach.TeamDesignator:
					return Megalo.MegaloScriptTokenAbstractType.Team;
				case Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach.Object:
					return Megalo.MegaloScriptTokenAbstractType.Object;
				case Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach.Numeric:
					return Megalo.MegaloScriptTokenAbstractType.Numeric;
				case Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach.TimerSeconds:
					return Megalo.MegaloScriptTokenAbstractType.Timer;

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}

		[Contracts.Pure]
		internal static Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4 ToHalo4(
			this Megalo.MegaloScriptTokenAbstractType type)
		{
			switch (type)
			{
				case Megalo.MegaloScriptTokenAbstractType.None:
					return Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.None;
				case Megalo.MegaloScriptTokenAbstractType.Player:
					return Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.AbsolutePlayerIndex;
				case Megalo.MegaloScriptTokenAbstractType.Team:
					return Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.TeamDesignator;
				case Megalo.MegaloScriptTokenAbstractType.Object:
					return Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.Object;
				case Megalo.MegaloScriptTokenAbstractType.Numeric:
					return Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.Numeric;
				case Megalo.MegaloScriptTokenAbstractType.SignedNumeric:
					return Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.SignedNumeric;
				case Megalo.MegaloScriptTokenAbstractType.Timer:
					return Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.TimerSeconds;

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
		[Contracts.Pure]
		internal static Megalo.MegaloScriptTokenAbstractType ToAbstract(
			this Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4 type)
		{
			switch (type)
			{
				case Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.None:
					return Megalo.MegaloScriptTokenAbstractType.None;
				case Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.AbsolutePlayerIndex:
					return Megalo.MegaloScriptTokenAbstractType.Player;
				case Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.TeamDesignator:
					return Megalo.MegaloScriptTokenAbstractType.Team;
				case Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.Object:
					return Megalo.MegaloScriptTokenAbstractType.Object;
				case Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.Numeric:
					return Megalo.MegaloScriptTokenAbstractType.Numeric;
				case Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.SignedNumeric:
					return Megalo.MegaloScriptTokenAbstractType.SignedNumeric;
				case Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4.TimerSeconds:
					return Megalo.MegaloScriptTokenAbstractType.Timer;

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
		#endregion

		#region RuntimeData.Variants
		internal static bool UseUserOptionNames(this RuntimeData.Variants.GameEngineMegaloVariantTagElementStreamFlags flags)
		{
			const RuntimeData.Variants.GameEngineMegaloVariantTagElementStreamFlags k_mask =
				RuntimeData.Variants.GameEngineMegaloVariantTagElementStreamFlags.UseUserOptionNames;
			return (flags & k_mask) == k_mask;
		}
		#endregion
	};
}