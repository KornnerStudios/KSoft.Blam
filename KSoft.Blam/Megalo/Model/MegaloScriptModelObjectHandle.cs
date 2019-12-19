using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Interop = System.Runtime.InteropServices;

namespace KSoft.Blam.Megalo.Model
{
	using BitFieldTraits = Bitwise.BitFieldTraits;
	using BitEncoders = TypeExtensionsBlam.BitEncoders;

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Type = {Type}")]
	[Interop.StructLayout(Interop.LayoutKind.Explicit, Size = MegaloScriptModelObjectHandle.kSizeOf)]
	public struct MegaloScriptModelObjectHandle
		: System.Collections.IComparer, IComparer<MegaloScriptModelObjectHandle>
		, IEquatable<MegaloScriptModelObjectHandle>
	{
		#region Constants
		public const int kSizeOf = sizeof(uint);

		const int kBitCountId =				16;

		static class Constants
		{
			public static readonly BitFieldTraits kIdBitField =
				new BitFieldTraits(kBitCountId);
			public static readonly BitFieldTraits kTypeBitField =
				new BitFieldTraits(BitEncoders.MegaloScriptModelObjectType.BitCountTrait, kIdBitField);

			public static readonly BitFieldTraits kLastBitField =
				kTypeBitField;

			public static readonly int kMaxId =		(int)Bits.BitCountToMask32(kBitCountId);
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		/// <remarks>19 bits at last count</remarks>
		public static int BitCount { get { return Constants.kLastBitField.FieldsBitCount; } }
		public static uint Bitmask { get { return Constants.kLastBitField.FieldsBitmask.u32; } }

		/// <remarks>ONLY PUBLIC FOR USE IN CODE CONTRACTS</remarks>
		[Contracts.Pure] public static bool ValidateId(int id)	{ return id.IsNoneOrPositive() && id < Constants.kMaxId; }

		public static readonly MegaloScriptModelObjectHandle Null = new MegaloScriptModelObjectHandle(
			MegaloScriptModelObjectType.None);
		public static readonly MegaloScriptModelObjectHandle NullCondition = new MegaloScriptModelObjectHandle(
			MegaloScriptModelObjectType.Condition);
		#endregion

		#region Internal Value
		[Interop.FieldOffset(0)] readonly uint mHandle;

		//internal uint Handle { get { return mHandle; } }

		static void InitializeHandle(out uint handle, MegaloScriptModelObjectType type, int id)
		{
			var encoder = new Bitwise.HandleBitEncoder();
			encoder.EncodeNoneable32(id, Constants.kIdBitField);
			encoder.Encode32(type, BitEncoders.MegaloScriptModelObjectType);

			Contract.Assert(encoder.UsedBitCount == MegaloScriptModelObjectHandle.BitCount);

			handle = encoder.GetHandle32();
		}
		#endregion

		#region Ctor
		public MegaloScriptModelObjectHandle(MegaloScriptModelObjectType type, int id = TypeExtensions.kNone)
		{
			Contract.Requires(ValidateId(id));

			InitializeHandle(out mHandle, type, id);
		}
		#endregion

		#region Value properties
		public int Id							{ get { return Bits.BitDecodeNoneable(mHandle, Constants.kIdBitField); } }
		public MegaloScriptModelObjectType Type	{ get { return BitEncoders.MegaloScriptModelObjectType.BitDecode(mHandle, Constants.kTypeBitField.BitIndex); } }

		public bool IsNone						{ get { return Id.IsNone(); } }
		public bool IsNotNone					{ get { return Id.IsNotNone(); } }
		public bool IsNoneOrPositive			{ get { return Id.IsNoneOrPositive(); } }
		#endregion

		#region IComparer<MegaloScriptModelObjectHandle> Members
		int System.Collections.IComparer.Compare(object x, object y)
		{
			return Compare((MegaloScriptModelObjectHandle)x, (MegaloScriptModelObjectHandle)y);
		}

		public int Compare(MegaloScriptModelObjectHandle x, MegaloScriptModelObjectHandle y)
		{
			return (int)(x.mHandle - y.mHandle);
		}
		#endregion

		#region Overrides
		public override bool Equals(object obj)
		{
			if (obj is MegaloScriptModelObjectHandle)
				return this.mHandle == ((MegaloScriptModelObjectHandle)obj).mHandle;

			return false;
		}
		public override int GetHashCode()	{ return (int)mHandle; }
		#endregion

		#region Equality Members
		public bool Equals(MegaloScriptModelObjectHandle other) { return mHandle == other.mHandle; }
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
			switch (handle.Type)
			{
				case MegaloScriptModelObjectType.Condition:		return model.CreateCondition(id);
				case MegaloScriptModelObjectType.Action:		return model.CreateAction(id);
				case MegaloScriptModelObjectType.Trigger:		return model.CreateTrigger(id);
				case MegaloScriptModelObjectType.VirtualTrigger:return model.CreateVirtualTrigger(id);

				default: throw new KSoft.Debug.UnreachableException(handle.Type.ToString());
			}
		}
		static MegaloScriptModelObject CreateForWriteSansId(MegaloScriptModel model, MegaloScriptModelObjectType type)
		{
			switch (type)
			{
				case MegaloScriptModelObjectType.Condition:		return model.CreateCondition();
				case MegaloScriptModelObjectType.Action:		return model.CreateAction();
				case MegaloScriptModelObjectType.Trigger:		return model.CreateTrigger();
				case MegaloScriptModelObjectType.VirtualTrigger:return model.CreateVirtualTrigger();

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
		static bool CanEmbed(MegaloScriptModelObjectType type)
		{
			switch (type)
			{
				case MegaloScriptModelObjectType.Condition:
				case MegaloScriptModelObjectType.Action:
				case MegaloScriptModelObjectType.Trigger:
				case MegaloScriptModelObjectType.VirtualTrigger:
					return true;
				default: return false;
			}
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
				throw new System.IO.InvalidDataException(string.Format("{0}s can't be embedded (ID={1})",
					handle.Type, handle.Id));

			// Only try to recreate when we're reading and the script wasn't streamed without IDs
			// Else, Serialize would have already created
			var obj = reading && !streamed_sans_id ?
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
				s.StreamAttribute(MegaloScriptModelObject.kIdAttributeName, ref id);
			else if (reading)
				handle = CreateForWriteSansId(model, type).Handle;

			// handle will already be valid if the above case is hit
			if (reading && handle.Type == MegaloScriptModelObjectType.None)
				handle = new MegaloScriptModelObjectHandle(type, id);
		}
		#endregion

		/// <summary>Compare two handles (equality)</summary>
		/// <param name="lhs">left-hand value for comparison expression</param>
		/// <param name="rhs">right-hand value for comparison expression</param>
		/// <returns><paramref name="lhs"/> == <paramref name="rhs"/></returns>
		[Contracts.Pure]
		public static bool operator ==(MegaloScriptModelObjectHandle lhs, MegaloScriptModelObjectHandle rhs)
		{ return lhs.mHandle == rhs.mHandle; }
		/// <summary>Compare two handles (inequality)</summary>
		/// <param name="lhs">left-hand value for comparison expression</param>
		/// <param name="rhs">right-hand value for comparison expression</param>
		/// <returns><paramref name="lhs"/> != <paramref name="rhs"/></returns>
		/// <remarks>Ignores address size</remarks>
		[Contracts.Pure]
		public static bool operator !=(MegaloScriptModelObjectHandle lhs, MegaloScriptModelObjectHandle rhs)
		{ return lhs.mHandle != rhs.mHandle; }
	};
}