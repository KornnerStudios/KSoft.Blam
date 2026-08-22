using System;
using System.Collections.Generic;

using MegaloScriptTokenTypeHaloReach = KSoft.Blam.Games.HaloReach.Megalo.MegaloScriptTokenTypeHaloReach;
using MegaloScriptTokenTypeHalo4 = KSoft.Blam.Games.Halo4.Megalo.MegaloScriptTokenTypeHalo4;

namespace KSoft.Blam
{
	using MegaloModel = Blam.Megalo.Model;
	using MegaloProto = Blam.Megalo.Proto;

	public static partial class TypeExtensionsBlam
	{
		internal const int kTagStringLength = 31;
		internal static readonly Memory.Strings.StringStorage kTagStringStorage = new(
			Memory.Strings.StringStorageWidthType.Ascii, Memory.Strings.StringStorageType.CharArray, kTagStringLength+1);
		internal static readonly Text.StringStorageEncoding kTagStringEncoding = new(kTagStringStorage);

		#region Enum Bit Encoders
		public static class BitEncoders
		{
			// KSoft.Blam.Engine
			public static readonly EnumBitEncoder32<Engine.EngineGeneration>
				EngineGeneration = new();
			public static readonly EnumBitEncoder32<Engine.EngineProductionStage>
				EngineProductionStage = new();

			internal static readonly EnumBitEncoder32<Megalo.MegaloScriptVariableType>
				MegaloScriptVariableType = new();
			internal static readonly EnumBitEncoder32<Megalo.MegaloScriptVariableSet>
				MegaloScriptVariableSet = new();

			#region KSoft.Blam.Megalo.Model
			internal static readonly EnumBitEncoder32<MegaloModel.MegaloScriptModelObjectType>
				MegaloScriptModelObjectType = new();
			#endregion

			#region KSoft.Blam.Megalo.Proto
			internal static readonly EnumBitEncoder32<MegaloProto.MegaloScriptVarReferenceType>
				MegaloScriptVarReferenceType = new();

			internal static readonly EnumBitEncoder32<MegaloProto.MegaloScriptValueIndexTarget>
				MegaloScriptValueIndexTarget = new();
			internal static readonly EnumBitEncoder32<MegaloProto.MegaloScriptValueIndexTraits>
				MegaloScriptValueIndexTraits = new();

			internal static readonly EnumBitEncoder32<MegaloProto.MegaloScriptValueBaseType>
				MegaloScriptValueBaseType = new();

			internal static readonly EnumBitEncoder32<MegaloProto.MegaloScriptValueEnumTraits>
				MegaloScriptValueEnumTraits = new();
			#endregion
		};

		public static IEnumBitEncoder<uint> GetBitEncoder(this Engine.EngineGeneration _)
			=> BitEncoders.EngineGeneration;
		public static IEnumBitEncoder<uint> GetBitEncoder(this Engine.EngineProductionStage _)
			=> BitEncoders.EngineProductionStage;
		#endregion

		#region Blob
		public static int GetDataSize(this Blob.Transport.BlobTransportStreamAuthentication type)
		{
			return type switch
			{
				Blob.Transport.BlobTransportStreamAuthentication.None
				=> 0,
				Blob.Transport.BlobTransportStreamAuthentication.Crc
				=> sizeof(uint),
				Blob.Transport.BlobTransportStreamAuthentication.Hash or
				Blob.Transport.BlobTransportStreamAuthentication.Rsa
				=> 0x100,
				_ => throw new KSoft.Debug.UnreachableException(type.ToString()),
			};
		}
		#endregion

		internal const int IndexOfByPropertyNotFoundResult = int.MinValue;
		internal static int IndexOfByProperty<T, TProp>(this IList<T> list, TProp value, Func<T, TProp> property
			, Func<TProp, string> generateNotFoundMessage = null)
			where TProp : IEquatable<TProp>
		{
			ArgumentNullException.ThrowIfNull(property);

			for (int x = 0; x < list.Count; x++)
			{
				if (property(list[x]).Equals(value))
				{
					return x;
				}
			}

			if (generateNotFoundMessage == null)
			{
				return IndexOfByPropertyNotFoundResult;
			}
			else
			{
				throw new KeyNotFoundException(generateNotFoundMessage(value));
			}
		}

		#region IO.BitStream
		#region Single
		// interesting: http://stackoverflow.com/a/3542975/444977

		static void ThrowIfBitCountExceeds(int bitCount, int maxBitCount)
		{
			if (bitCount > maxBitCount)
			{
				throw new ArgumentOutOfRangeException(nameof(bitCount), bitCount,
					string.Format(Util.InvariantCultureInfo,
						"Bit count must be <= {0}; actual value is {1}.",
						maxBitCount, bitCount));
			}
		}

		public static float DecodeSingle(uint rawBits, float min, float max, int bitCount, bool isSigned, bool unknown)
		{
			int r10 = 1 << bitCount;
			if (isSigned)
			{
				r10 -= 1;
			}

			float fp1;
			if (unknown)
			{
				if (rawBits == 0)
				{
					fp1 = min;
				}
				else if (rawBits == (r10 - 1))
				{
					fp1 = max;
				}
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

			if (isSigned)
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
		public static uint EncodeSingle(float real, float min, float max, int bitCount, bool isSigned, bool unknown)
		{
			uint r11 = 1U << bitCount;
			if (isSigned)
			{
				r11 -= 1;
			}

			uint r10;
			if (unknown)
			{
				if (real == min)
				{
					r11 = 0;
				}
				else if (real == max)
				{
					r11 -= 1;
				}
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
					{
						r10 = 1;
					}

					if (r10 < r11)
					{
						r11 = r10;
					}
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
				{
					r11 = r10;
				}
			}

			return r11;
		}

		static uint Read(IO.BitStream s, out float real, float min, float max, int bitCount, bool isSigned, bool unknown)
		{
			s.Read(out uint raw_bits, bitCount);

			real = DecodeSingle(raw_bits, min, max, bitCount, isSigned, unknown);
			return raw_bits;
		}
		static void Write(IO.BitStream s, float real, float min, float max, int bitCount, bool isSigned, bool unknown)
		{
			uint raw_bits = EncodeSingle(real, min, max, bitCount, isSigned, unknown);

			s.Write(raw_bits, bitCount);
		}
		public static void Stream(this IO.BitStream s, ref float real, float min, float max, int bitCount, bool isSigned, bool unknown)
		{
			ThrowIfBitCountExceeds(bitCount, Bits.kInt32BitCount);

				 if (s.IsReading)	{ Read(s, out real, min, max, bitCount, isSigned, unknown); }
			else if (s.IsWriting)	{ Write(s, real, min, max, bitCount, isSigned, unknown); }
		}
		#endregion

		#region Stream index (Int32)
		/// <remarks>Used for indexes which *are* typically NONE (-1)</remarks>
		public static void StreamIndex(this IO.BitStream s, ref int value, int bitCount = Bits.kInt32BitCount)
		{
			ThrowIfBitCountExceeds(bitCount, Bits.kInt32BitCount);

			if (s.IsReading)
			{
				bool is_none = s.ReadBoolean();

				if (!is_none)
				{
					s.Read(out value, bitCount);
				}
				else
				{
					value = TypeExtensions.kNone;
				}
			}
			else if (s.IsWriting)
			{
				bool is_none = value.IsNone();
				s.Write(is_none);

				if (!is_none)
				{
					s.Write(value, bitCount);
				}
			}
		}
		/// <remarks>Used for indexes which *are not* typically NONE (-1)</remarks>
		public static void StreamIndexPos(this IO.BitStream s, ref int value, int bitCount = Bits.kInt32BitCount)
		{
			ThrowIfBitCountExceeds(bitCount, Bits.kInt32BitCount);

			if (s.IsReading)
			{
				bool not_none = s.ReadBoolean();

				if (not_none)
				{
					s.Read(out value, bitCount);
				}
				else
				{
					value = TypeExtensions.kNone;
				}
			}
			else if (s.IsWriting)
			{
				bool not_none = value.IsNotNone();
				s.Write(not_none);

				if (not_none)
				{
					s.Write(value, bitCount);
				}
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
			ThrowIfBitCountExceeds(bitCount, Bits.kByteBitCount);

			if (s.IsReading)
			{
				s.Read(out value, bitCount);
				value--;
			}
			else if (s.IsWriting)
			{
				if (value < TypeExtensions.kNone)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value,
						string.Format(Util.InvariantCultureInfo,
							"Value must be >= NONE ({0}); actual value is {1}.",
							TypeExtensions.kNone, value));
				}
				s.Write(value + 1, bitCount);
			}
		}
		/// <summary>Streams an integer which is >= -1, but when streamed out the value is added by 1 (so it will be >= 0)</summary>
		/// <param name="s"></param>
		/// <param name="value"></param>
		/// <param name="bitCount"></param>
		public static void StreamNoneable(this IO.BitStream s, ref short value, int bitCount = Bits.kInt16BitCount)
		{
			ThrowIfBitCountExceeds(bitCount, Bits.kInt16BitCount);

			if (s.IsReading)
			{
				s.Read(out value, bitCount);
				value--;
			}
			else if (s.IsWriting)
			{
				if (value < TypeExtensions.kNone)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value,
						string.Format(Util.InvariantCultureInfo,
							"Value must be >= NONE ({0}); actual value is {1}.",
							TypeExtensions.kNone, value));
				}
				s.Write(value + 1, bitCount);
			}
		}
		/// <summary>Streams an integer which is >= -1, but when streamed out the value is added by 1 (so it will be >= 0)</summary>
		/// <param name="s"></param>
		/// <param name="value"></param>
		/// <param name="bitCount"></param>
		public static void StreamNoneable(this IO.BitStream s, ref int value, int bitCount = Bits.kInt32BitCount)
		{
			ThrowIfBitCountExceeds(bitCount, Bits.kInt32BitCount);

			if (s.IsReading)
			{
				s.Read(out value, bitCount);
				value--;
			}
			else if (s.IsWriting)
			{
				if (!value.IsNoneOrPositive())
				{
					throw new ArgumentOutOfRangeException(nameof(value), value,
						string.Format(Util.InvariantCultureInfo,
							"Value must be NONE or non-negative; actual value is {0}.",
							value));
				}
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
			{
				value = KSoft.TypeExtensions.kNone;
			}
		}
		#endregion

		#region DefaultOption utils
		public const int kDefaultOption = -2;

		public const int kUsualDefaultRespawnTimeInSeconds = 5;

		readonly static Predicate<int> kNotDefaultOption32 = x => x != kDefaultOption;

		public static void StreamAttributeOptDefaultOption<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s,
			string name, ref int value)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt(name, ref value, TypeExtensionsBlam.kNotDefaultOption32) && s.IsReading)
			{
				value = TypeExtensionsBlam.kDefaultOption;
			}
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
			{
				value = TypeExtensionsBlam.kUnchanged;
			}
		}
		public static void StreamAttributeOptUnchanged<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s,
			string name, ref int value)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt(name, ref value, TypeExtensionsBlam.kNotUnchanged32) && s.IsReading)
			{
				value = TypeExtensionsBlam.kUnchanged;
			}
		}

		public static void StreamAttributeOptUnchangedZero<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s,
			string name, ref byte value)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt(name, ref value, Predicates.IsNotZero) && s.IsReading)
			{
				value = 0;
			}
		}
		public static void StreamAttributeOptUnchangedZero<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s,
			string name, ref int value)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt(name, ref value, Predicates.IsNotZero) && s.IsReading)
			{
				value = 0;
			}
		}
		#endregion

		#region Megalo
		internal static Megalo.MegaloScriptVariableType ToVariableType(
			this Megalo.MegaloScriptVariableReferenceType type)
		{
			return type switch
			{
				Megalo.MegaloScriptVariableReferenceType.Custom => Megalo.MegaloScriptVariableType.Numeric,
				Megalo.MegaloScriptVariableReferenceType.Player => Megalo.MegaloScriptVariableType.Player,
				Megalo.MegaloScriptVariableReferenceType.Object => Megalo.MegaloScriptVariableType.Object,
				Megalo.MegaloScriptVariableReferenceType.Team => Megalo.MegaloScriptVariableType.Team,
				Megalo.MegaloScriptVariableReferenceType.Timer => Megalo.MegaloScriptVariableType.Timer,
				_ => throw new KSoft.Debug.UnreachableException(type.ToString()),
			};
		}

		internal static bool RequiresBitLength(this MegaloProto.MegaloScriptValueBaseType type)
		{
			return type switch
			{
				MegaloProto.MegaloScriptValueBaseType.Int or
				MegaloProto.MegaloScriptValueBaseType.UInt or
				MegaloProto.MegaloScriptValueBaseType.Enum
				=> true,
				_ => false,
			};
		}

		/// <summary>Is the target based in static (ie, tag) data?</summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool IsStaticData(this MegaloProto.MegaloScriptValueIndexTarget target)
		{
			return target switch
			{
				MegaloProto.MegaloScriptValueIndexTarget.ObjectType or
				MegaloProto.MegaloScriptValueIndexTarget.Name or
				MegaloProto.MegaloScriptValueIndexTarget.Sound or
				MegaloProto.MegaloScriptValueIndexTarget.Incident or
				MegaloProto.MegaloScriptValueIndexTarget.HudWidgetIcon or
				MegaloProto.MegaloScriptValueIndexTarget.GameEngineIcon or
				MegaloProto.MegaloScriptValueIndexTarget.Medal or
				MegaloProto.MegaloScriptValueIndexTarget.Ordnance
				=> true,
				_ => false,
			};
		}
		/// <summary>Is the target based in variant data?</summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool IsVariantData(this MegaloProto.MegaloScriptValueIndexTarget target)
		{
			return target switch
			{
				MegaloProto.MegaloScriptValueIndexTarget.LoadoutPalette or
				MegaloProto.MegaloScriptValueIndexTarget.Option or
				MegaloProto.MegaloScriptValueIndexTarget.String or
				MegaloProto.MegaloScriptValueIndexTarget.PlayerTraits or
				MegaloProto.MegaloScriptValueIndexTarget.Statistic or
				MegaloProto.MegaloScriptValueIndexTarget.Widget or
				MegaloProto.MegaloScriptValueIndexTarget.ObjectFilter or
				MegaloProto.MegaloScriptValueIndexTarget.GameObjectFilter
				=> true,
				_ => false,
			};
		}
		/// <summary>Does the target have a human-friendly name?</summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool HasIndexName(this MegaloProto.MegaloScriptValueIndexTarget target)
		{
			return target switch
			{
				MegaloProto.MegaloScriptValueIndexTarget.ObjectType or
				MegaloProto.MegaloScriptValueIndexTarget.Name or
				MegaloProto.MegaloScriptValueIndexTarget.Sound or
				MegaloProto.MegaloScriptValueIndexTarget.Incident or
				MegaloProto.MegaloScriptValueIndexTarget.HudWidgetIcon or
				MegaloProto.MegaloScriptValueIndexTarget.GameEngineIcon or
				MegaloProto.MegaloScriptValueIndexTarget.Medal or
				MegaloProto.MegaloScriptValueIndexTarget.Ordnance or

				MegaloProto.MegaloScriptValueIndexTarget.Option or
				MegaloProto.MegaloScriptValueIndexTarget.String or
				MegaloProto.MegaloScriptValueIndexTarget.PlayerTraits or
				MegaloProto.MegaloScriptValueIndexTarget.Statistic or
				MegaloProto.MegaloScriptValueIndexTarget.Widget or
				MegaloProto.MegaloScriptValueIndexTarget.ObjectFilter or
				MegaloProto.MegaloScriptValueIndexTarget.GameObjectFilter
				=> true,
				_ => false,
			};
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

		internal static MegaloScriptTokenTypeHaloReach ToHaloReach(
			this Megalo.MegaloScriptTokenAbstractType type)
		{
			return type switch
			{
				Megalo.MegaloScriptTokenAbstractType.None => MegaloScriptTokenTypeHaloReach.None,
				Megalo.MegaloScriptTokenAbstractType.Player => MegaloScriptTokenTypeHaloReach.AbsolutePlayerIndex,
				Megalo.MegaloScriptTokenAbstractType.Team => MegaloScriptTokenTypeHaloReach.TeamDesignator,
				Megalo.MegaloScriptTokenAbstractType.Object => MegaloScriptTokenTypeHaloReach.Object,
				Megalo.MegaloScriptTokenAbstractType.Numeric => MegaloScriptTokenTypeHaloReach.Numeric,
				Megalo.MegaloScriptTokenAbstractType.SignedNumeric
				=> throw new NotSupportedException(type.ToString()),
				Megalo.MegaloScriptTokenAbstractType.Timer => MegaloScriptTokenTypeHaloReach.TimerSeconds,
				_ => throw new KSoft.Debug.UnreachableException(type.ToString()),
			};
		}
		internal static Megalo.MegaloScriptTokenAbstractType ToAbstract(
			this MegaloScriptTokenTypeHaloReach type)
		{
			return type switch
			{
				MegaloScriptTokenTypeHaloReach.None => Megalo.MegaloScriptTokenAbstractType.None,
				MegaloScriptTokenTypeHaloReach.AbsolutePlayerIndex => Megalo.MegaloScriptTokenAbstractType.Player,
				MegaloScriptTokenTypeHaloReach.TeamDesignator => Megalo.MegaloScriptTokenAbstractType.Team,
				MegaloScriptTokenTypeHaloReach.Object => Megalo.MegaloScriptTokenAbstractType.Object,
				MegaloScriptTokenTypeHaloReach.Numeric => Megalo.MegaloScriptTokenAbstractType.Numeric,
				MegaloScriptTokenTypeHaloReach.TimerSeconds => Megalo.MegaloScriptTokenAbstractType.Timer,
				_ => throw new KSoft.Debug.UnreachableException(type.ToString()),
			};
		}

		internal static MegaloScriptTokenTypeHalo4 ToHalo4(
			this Megalo.MegaloScriptTokenAbstractType type)
		{
			return type switch
			{
				Megalo.MegaloScriptTokenAbstractType.None => MegaloScriptTokenTypeHalo4.None,
				Megalo.MegaloScriptTokenAbstractType.Player => MegaloScriptTokenTypeHalo4.AbsolutePlayerIndex,
				Megalo.MegaloScriptTokenAbstractType.Team => MegaloScriptTokenTypeHalo4.TeamDesignator,
				Megalo.MegaloScriptTokenAbstractType.Object => MegaloScriptTokenTypeHalo4.Object,
				Megalo.MegaloScriptTokenAbstractType.Numeric => MegaloScriptTokenTypeHalo4.Numeric,
				Megalo.MegaloScriptTokenAbstractType.SignedNumeric => MegaloScriptTokenTypeHalo4.SignedNumeric,
				Megalo.MegaloScriptTokenAbstractType.Timer => MegaloScriptTokenTypeHalo4.TimerSeconds,
				_ => throw new KSoft.Debug.UnreachableException(type.ToString()),
			};
		}
		internal static Megalo.MegaloScriptTokenAbstractType ToAbstract(
			this MegaloScriptTokenTypeHalo4 type)
		{
			return type switch
			{
				MegaloScriptTokenTypeHalo4.None => Megalo.MegaloScriptTokenAbstractType.None,
				MegaloScriptTokenTypeHalo4.AbsolutePlayerIndex => Megalo.MegaloScriptTokenAbstractType.Player,
				MegaloScriptTokenTypeHalo4.TeamDesignator => Megalo.MegaloScriptTokenAbstractType.Team,
				MegaloScriptTokenTypeHalo4.Object => Megalo.MegaloScriptTokenAbstractType.Object,
				MegaloScriptTokenTypeHalo4.Numeric => Megalo.MegaloScriptTokenAbstractType.Numeric,
				MegaloScriptTokenTypeHalo4.SignedNumeric => Megalo.MegaloScriptTokenAbstractType.SignedNumeric,
				MegaloScriptTokenTypeHalo4.TimerSeconds => Megalo.MegaloScriptTokenAbstractType.Timer,
				_ => throw new KSoft.Debug.UnreachableException(type.ToString()),
			};
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
