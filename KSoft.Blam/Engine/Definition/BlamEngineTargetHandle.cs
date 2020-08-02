using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Interop = System.Runtime.InteropServices;

namespace KSoft.Blam.Engine
{
	using BitFieldTraits = Bitwise.BitFieldTraits;

	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	[System.Diagnostics.DebuggerDisplay("Engine# = {Build.EngineIndex}, TargetPlatform# = {TargetPlatformIndex}, ResourceModel# = {ResourceModelIndex}")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public struct BlamEngineTargetHandle
		: IComparer<BlamEngineTargetHandle>, System.Collections.IComparer
		, IComparable<BlamEngineTargetHandle>, IComparable
		, IEquatable<BlamEngineTargetHandle>
	{
		#region Constants
		// nesting these into a static class makes them run before the struct's static ctor...
		// which, being a value type cctor, may not run when we want it
		static class Constants
		{
			public static readonly BitFieldTraits kResourceModelBitField =
				new BitFieldTraits(EngineRegistry.kResourceModelBitCount);
			public static readonly BitFieldTraits kTargetPlatformBitField =
				new BitFieldTraits(EngineTargetPlatform.kIndexBitCount, kResourceModelBitField);
			public static readonly BitFieldTraits kBuildBitField =
				new BitFieldTraits(EngineBuildHandle.BitCount, kTargetPlatformBitField);

			public static readonly BitFieldTraits kLastBitField =
				kBuildBitField;
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		/// <remarks>16 bits at last count</remarks>
		public static int BitCount { get { return Constants.kLastBitField.FieldsBitCount; } }
		public static uint Bitmask { get { return Constants.kLastBitField.FieldsBitmask.u32; } }

		public static readonly BlamEngineTargetHandle None = new BlamEngineTargetHandle();
		#endregion

		#region Internal Value
		[Interop.FieldOffset(0)] readonly uint mHandle;

		//internal uint Handle { get { return mHandle; } }

		static void InitializeHandle(out uint handle,
			EngineBuildHandle buildHandle, int platformIndex, int resourceModelIndex)
		{
			if (platformIndex.IsNone() && resourceModelIndex.IsNone())
				resourceModelIndex = 0;

			var encoder = new Bitwise.HandleBitEncoder();
			EngineRegistry.BitEncodeResourceModelIndex(ref encoder, resourceModelIndex);
			EngineTargetPlatform.BitEncodeIndex(ref encoder, platformIndex);
			encoder.Encode32(buildHandle.Handle, EngineBuildHandle.Bitmask);

			Contract.Assert(encoder.UsedBitCount == BlamEngineTargetHandle.BitCount);

			handle = encoder.GetHandle32();
		}
		#endregion

		#region Ctor
		public BlamEngineTargetHandle(EngineBuildHandle buildHandle, int platformIndex, int resourceModelIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(EngineTargetPlatform.IsValidIndex(platformIndex));
			Contract.Requires<ArgumentOutOfRangeException>(EngineRegistry.IsValidResourceModelIndex(resourceModelIndex) ||
				(platformIndex.IsNone() && resourceModelIndex.IsNone()));

			InitializeHandle(out mHandle, buildHandle, platformIndex, resourceModelIndex);
		}
		internal BlamEngineTargetHandle(uint handle, BitFieldTraits engineTargetField)
		{
			handle >>= engineTargetField.BitIndex;
			handle &= Bitmask;

			mHandle = handle;
		}

		/// <summary>
		/// Creates an engine target handle using the build handlew, but with the platform and resource model set to NONE
		/// </summary>
		/// <param name="buildHandle"></param>
		/// <returns></returns>
		public static BlamEngineTargetHandle FromBuildHandleOnly(EngineBuildHandle buildHandle)
		{
			int target_platform_index = TypeExtensions.kNone;
			int resource_model_index = TypeExtensions.kNone;

			return new BlamEngineTargetHandle(buildHandle, target_platform_index, resource_model_index);
		}
		#endregion

		#region Value properties
		[Contracts.Pure]
		public EngineBuildHandle Build { get {
			return new EngineBuildHandle(mHandle, Constants.kBuildBitField);
		} }
		[Contracts.Pure]
		public int TargetPlatformIndex { get {
			return EngineTargetPlatform.BitDecodeIndex(mHandle, Constants.kTargetPlatformBitField.BitIndex);
		} }
		[Contracts.Pure]
		public int ResourceModelIndex { get {
			return EngineRegistry.BitDecodeResourceModelIndex(mHandle, Constants.kResourceModelBitField.BitIndex);
		} }

		[Contracts.Pure]
		public EngineTargetPlatform TargetPlatform { get {
			var index = TargetPlatformIndex;

			return index.IsNotNone()
				? EngineRegistry.TargetPlatforms[index]
				: null;
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
			if (obj is BlamEngineTargetHandle)
				return this.mHandle == ((BlamEngineTargetHandle)obj).mHandle;

			return false;
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on its exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode()
		{
			return (int)mHandle;
		}
		/// <summary>Returns a string representation of this object</summary>
		/// <returns>"[Build\tTargetPlatform\tResourceModelIndex]"</returns>
		public override string ToString()
		{
			Contract.Ensures(Contract.Result<string>() != null);

			return string.Format(Util.InvariantCultureInfo,
				"[{0}\t{1}\t{2}]",
				Build.ToString(),
				TargetPlatform,
				ResourceModelIndex.ToString(Util.InvariantCultureInfo));
		}
		#endregion

		/// <summary>Creates a string of the build component name ids separated by periods</summary>
		/// <returns>Empty string if this <see cref="IsNone"/></returns>
		/// <remarks>If the <see cref="Branch"/>'s display name is the same as <see cref="Engine"/>, the former isn't included in the output</remarks>
		[Contracts.Pure]
		public string ToDisplayString()
		{
			Contract.Ensures(Contract.Result<string>() != null);

			if (IsNone)
				return TypeExtensions.kNoneDisplayString;

			var sb = new System.Text.StringBuilder(Build.ToDisplayString());
			int platform_index = TargetPlatformIndex;
			int rsrc_model_index = ResourceModelIndex;

			if (platform_index.IsNotNone())
			{
				var platform = EngineRegistry.TargetPlatforms[platform_index];
				sb.AppendFormat(Util.InvariantCultureInfo,
					".{0}", platform);

				#region ResourceModel
				if (rsrc_model_index.IsNotNone())
				{
					var rsrc_model = EngineRegistry.ResourceModels[rsrc_model_index];
					sb.AppendFormat(Util.InvariantCultureInfo,
						".{0}", rsrc_model);
				}
				#endregion
			}

			return sb.ToString();
		}

		#region IComparer<BlamEngineTargetHandle> Members
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(BlamEngineTargetHandle x, BlamEngineTargetHandle y)
		{
			return BlamEngineTargetHandle.StaticCompare(x, y);
		}
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		int System.Collections.IComparer.Compare(object x, object y)
		{
			BlamEngineTargetHandle _x; KSoft.Debug.TypeCheck.CastValue(x, out _x);
			BlamEngineTargetHandle _y; KSoft.Debug.TypeCheck.CastValue(y, out _y);

			return BlamEngineTargetHandle.StaticCompare(_x, _y);
		}
		#endregion

		#region IComparable<BlamEngineTargetHandle> Members
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(BlamEngineTargetHandle other)
		{
			return BlamEngineTargetHandle.StaticCompare(this, other);
		}
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		int IComparable.CompareTo(object obj)
		{
			BlamEngineTargetHandle _obj; KSoft.Debug.TypeCheck.CastValue(obj, out _obj);

			return BlamEngineTargetHandle.StaticCompare(this, _obj);
		}
		#endregion

		#region IEquatable<BlamEngineTargetHandle> Members
		/// <summary>See <see cref="IEquatable{T}.Equals"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(BlamEngineTargetHandle other)
		{
			return this.mHandle == other.mHandle;
		}
		#endregion

		#region Operators
		[Contracts.Pure]
		public static bool operator ==(BlamEngineTargetHandle lhs, BlamEngineTargetHandle rhs)	{ return lhs.mHandle == rhs.mHandle; }
		[Contracts.Pure]
		public static bool operator !=(BlamEngineTargetHandle lhs, BlamEngineTargetHandle rhs)	{ return lhs.mHandle != rhs.mHandle; }
		#endregion


		#region Util
		static int StaticCompare(BlamEngineTargetHandle lhs, BlamEngineTargetHandle rhs)
		{
			Contract.Assert(BlamEngineTargetHandle.BitCount < Bits.kInt32BitCount,
				"Handle bits needs to be <= 31 (ie, sans sign bit) in order for this implementation of CompareTo to reasonably work");

			int lhs_data = (int)lhs.mHandle;
			int rhs_data = (int)rhs.mHandle;
			int result = lhs_data - rhs_data;

			return result;
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public static void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			ref BlamEngineTargetHandle value)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			var build = reading
				? EngineBuildHandle.None
				: value.Build;
			var platform_index = reading
				? TypeExtensions.kNone
				: value.TargetPlatformIndex;
			var rsrc_model_index = reading
				? TypeExtensions.kNone
				: value.ResourceModelIndex;

			EngineBuildHandle.Serialize(s, ref build);
			if(!build.IsNone)
			{
				if (EngineTargetPlatform.SerializeId(s, "targetPlatform", ref platform_index, true))
				{
					EngineRegistry.SerializeResourceModelId(s, "resourceModel", ref rsrc_model_index, true);
				}
			}

			if (reading)
			{
				value = new BlamEngineTargetHandle(build, platform_index, rsrc_model_index);
			}
		}
		#endregion
	};
}
