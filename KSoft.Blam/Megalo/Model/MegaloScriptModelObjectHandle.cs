using System;
using System.Collections.Generic;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Blam.Megalo.Model
{
	using BitFieldTraits = Bitwise.BitFieldTraits;
	using BitEncoders = TypeExtensionsBlam.BitEncoders;

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Type = {Type}")]
	[Interop.StructLayout(Interop.LayoutKind.Explicit, Size = MegaloScriptModelObjectHandle.kSizeOf)]
	public struct MegaloScriptModelObjectHandle
		: System.Collections.IComparer, IComparer<MegaloScriptModelObjectHandle> // #REMOVE_BLAM
		, IComparable<MegaloScriptModelObjectHandle>, IComparable
		, IEquatable<MegaloScriptModelObjectHandle>
	{
		#region Constants
		public const int kSizeOf = sizeof(uint);

		const int kBitCountId =				16;

		static class Constants
		{
			public static readonly BitFieldTraits kIdBitField =
				new(kBitCountId);
			public static readonly BitFieldTraits kTypeBitField =
				new(BitEncoders.MegaloScriptModelObjectType.BitCountTrait, kIdBitField);

			public static readonly BitFieldTraits kLastBitField =
				kTypeBitField;

			public static readonly int kMaxId =		(int)Bits.BitCountToMask32(kBitCountId);
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		/// <remarks>19 bits at last count</remarks>
		public static int BitCount => Constants.kLastBitField.FieldsBitCount;
		public static uint Bitmask => Constants.kLastBitField.FieldsBitmask.u32;

		public static bool ValidateId(int id) => id.IsNoneOrPositive() && id < Constants.kMaxId;

		public static readonly MegaloScriptModelObjectHandle Null = new(MegaloScriptModelObjectType.None);
		public static readonly MegaloScriptModelObjectHandle NullCondition = new(MegaloScriptModelObjectType.Condition);
		#endregion

		#region Internal Value
		[Interop.FieldOffset(0)] readonly uint mHandle;

		//internal uint Handle { get { return mHandle; } }

		static void InitializeHandle(out uint handle, MegaloScriptModelObjectType type, int id)
		{
			var encoder = new Bitwise.HandleBitEncoder();
			encoder.EncodeNoneable32(id, Constants.kIdBitField);
			encoder.Encode32(type, BitEncoders.MegaloScriptModelObjectType);

			if (encoder.UsedBitCount != MegaloScriptModelObjectHandle.BitCount)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Encoded handle used {0} bits; expected {1}.",
					encoder.UsedBitCount, MegaloScriptModelObjectHandle.BitCount));
			}

			handle = encoder.GetHandle32();
		}
		#endregion

		#region Ctor
		public MegaloScriptModelObjectHandle(MegaloScriptModelObjectType type, int id = TypeExtensions.kNone)
		{
			if (!ValidateId(id))
			{
				throw new ArgumentOutOfRangeException(nameof(id), id,
					string.Format(Util.InvariantCultureInfo,
						"Object handle id must be NONE or in the range [0, {0}).", Constants.kMaxId));
			}

			InitializeHandle(out mHandle, type, id);
		}
		#endregion

		#region Value properties
		public readonly int Id =>							Bits.BitDecodeNoneable(mHandle, Constants.kIdBitField);
		public readonly MegaloScriptModelObjectType Type =>	BitEncoders.MegaloScriptModelObjectType.BitDecode(mHandle, Constants.kTypeBitField.BitIndex);

		public readonly bool IsNone =>						Id.IsNone();
		public readonly bool IsNotNone =>					Id.IsNotNone();
		public readonly bool IsNoneOrPositive =>			Id.IsNoneOrPositive();
		#endregion

		#region IComparer<MegaloScriptModelObjectHandle> Members
		readonly int System.Collections.IComparer.Compare(object x, object y)
		{
			return Compare((MegaloScriptModelObjectHandle)x, (MegaloScriptModelObjectHandle)y);
		}

		public readonly int Compare(MegaloScriptModelObjectHandle x, MegaloScriptModelObjectHandle y)
		{
			return (int)(x.mHandle - y.mHandle);
		}
		#endregion

		#region IComparable<MegaloScriptModelObjectHandle> Members
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public readonly int CompareTo(MegaloScriptModelObjectHandle other) => MegaloScriptModelObjectHandle.StaticCompare(this, other);
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		readonly int IComparable.CompareTo(object obj)
		{
			KSoft.Debug.TypeCheck.CastValue(obj, out MegaloScriptModelObjectHandle _obj);

			return MegaloScriptModelObjectHandle.StaticCompare(this, _obj);
		}

		static int StaticCompare(MegaloScriptModelObjectHandle lhs, MegaloScriptModelObjectHandle rhs)
		{
			// #TODO figure out a a utility to do this generically for bit-encoded handles that can run
			// in the internal Constants class.
			if (MegaloScriptModelObjectHandle.BitCount >= Bits.kInt32BitCount)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Handle bit count must be less than {0}; actual bit count is {1}.",
					Bits.kInt32BitCount, MegaloScriptModelObjectHandle.BitCount));
			}

			int lhs_data = (int)lhs.mHandle;
			int rhs_data = (int)rhs.mHandle;
			int result = lhs_data - rhs_data;

			return result;
		}
		#endregion

		#region Overrides
		public override readonly bool Equals(object obj)
		{
			if (obj is MegaloScriptModelObjectHandle objHandle)
			{
				return this.mHandle == objHandle.mHandle;
			}

			return false;
		}
		public override readonly int GetHashCode() => (int)mHandle;
		#endregion

		#region Equality Members
		public readonly bool Equals(MegaloScriptModelObjectHandle other) => mHandle == other.mHandle;
		#endregion

		#region ITagElementStringNameStreamable Members
		/// <summary>Create the identiy of an existing object based on an its handle</summary>
		/// <param name="model"></param>
		/// <param name="handle"></param>
		/// <returns></returns>
		/// <remarks>Not all object types support this operation</remarks>
		static MegaloScriptModelObject Recreate(MegaloScriptModel model, MegaloScriptModelObjectHandle handle)
		{
			int id = handle.Id;
			return handle.Type switch
			{
				MegaloScriptModelObjectType.Condition =>		model.CreateCondition(id),
				MegaloScriptModelObjectType.Action =>			model.CreateAction(id),
				MegaloScriptModelObjectType.Trigger =>			model.CreateTrigger(id),
				MegaloScriptModelObjectType.VirtualTrigger =>	model.CreateVirtualTrigger(id),

				_ => throw new KSoft.Debug.UnreachableException(handle.Type.ToString()),
			};
		}
		static MegaloScriptModelObject CreateForWriteSansId(MegaloScriptModel model, MegaloScriptModelObjectType type)
		{
			return type switch
			{
				MegaloScriptModelObjectType.Condition =>		model.CreateCondition(),
				MegaloScriptModelObjectType.Action =>			model.CreateAction(),
				MegaloScriptModelObjectType.Trigger =>			model.CreateTrigger(),
				MegaloScriptModelObjectType.VirtualTrigger =>	model.CreateVirtualTrigger(),

				_ => throw new KSoft.Debug.UnreachableException(type.ToString()),
			};
		}
		static bool CanEmbed(MegaloScriptModelObjectType type)
		{
			return type switch
			{
				MegaloScriptModelObjectType.Condition or
				MegaloScriptModelObjectType.Action or
				MegaloScriptModelObjectType.Trigger or
				MegaloScriptModelObjectType.VirtualTrigger
				=> true,
				_ => false,
			};
		}

		internal static void SerializeForEmbed<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, MegaloScriptModel model,
			ref MegaloScriptModelObjectHandle handle)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;
			bool streamed_sans_id = model.TagElementStreamSerializeFlags.EmbedObjectsWriteSansIds();
			Serialize(s, model, ref handle);
			// #REVIEW_BLAM: ThrowReadException?
			if (!CanEmbed(handle.Type))
			{
				throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"{0}s can't be embedded (ID={1})",
					handle.Type, handle.Id));
			}

			// Only try to recreate when we're reading and the script wasn't streamed without IDs
			// Else, Serialize would have already created
			MegaloScriptModelObject obj = reading && !streamed_sans_id ?
				Recreate(model, handle)
				: model[handle];
			obj.Serialize(model, s);
		}
		internal static void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, MegaloScriptModel model,
			ref MegaloScriptModelObjectHandle handle)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;
			var type = reading
				? MegaloScriptModelObjectType.None
				: handle.Type;
			int id = reading
				? TypeExtensions.kNone
				: handle.Id;

			s.StreamAttributeEnum("type", ref type);

			if (!model.TagElementStreamSerializeFlags.EmbedObjectsWriteSansIds())
			{
				s.StreamAttribute(MegaloScriptModelObject.kIdAttributeName, ref id);
			}
			else if (reading)
			{
				handle = CreateForWriteSansId(model, type).Handle;
			}

			// handle will already be valid if the above case is hit
			if (reading && handle.Type == MegaloScriptModelObjectType.None)
			{
				handle = new MegaloScriptModelObjectHandle(type, id);
			}
		}
		#endregion

		/// <summary>Compare two handles (equality)</summary>
		/// <param name="lhs">left-hand value for comparison expression</param>
		/// <param name="rhs">right-hand value for comparison expression</param>
		/// <returns><paramref name="lhs"/> == <paramref name="rhs"/></returns>
		public static bool operator ==(MegaloScriptModelObjectHandle lhs, MegaloScriptModelObjectHandle rhs) => lhs.mHandle == rhs.mHandle;
		/// <summary>Compare two handles (inequality)</summary>
		/// <param name="lhs">left-hand value for comparison expression</param>
		/// <param name="rhs">right-hand value for comparison expression</param>
		/// <returns><paramref name="lhs"/> != <paramref name="rhs"/></returns>
		/// <remarks>Ignores address size</remarks>
		public static bool operator !=(MegaloScriptModelObjectHandle lhs, MegaloScriptModelObjectHandle rhs) => lhs.mHandle != rhs.mHandle;
	};
}
