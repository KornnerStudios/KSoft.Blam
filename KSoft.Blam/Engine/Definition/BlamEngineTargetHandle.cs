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
	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	[System.Diagnostics.DebuggerDisplay("Engine# = {Build.EngineIndex}, TargetPlatform# = {TargetPlatformIndex}, ResourceModel# = {ResourceModelIndex}")]
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
			public static readonly int kResourceModelShift =
				0;

			public static readonly int kTargetPlatformShift =
				kResourceModelShift + EngineRegistry.kResourceModelBitCount;

			public static readonly int kBuildShift =
				kTargetPlatformShift + EngineBuildBranch.kIndexBitCount;

			public static readonly int kBitCount =
				kBuildShift + BlamEngine.kIndexBitCount;
			public static readonly uint kBitmask = Bits.BitCountToMask32(kBitCount);
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		/// <remarks>16 bits at last count</remarks>
		public static int BitCount { get { return Constants.kBitCount; } }
		public static uint Bitmask { get { return Constants.kBitmask; } }

		public static readonly BlamEngineTargetHandle None = new BlamEngineTargetHandle();
		#endregion

		#region Internal Value
		[Interop.FieldOffset(0)] readonly uint mHandle;

		internal uint Handle { get { return mHandle; } }

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
		internal BlamEngineTargetHandle(uint handle, int startBitIndex)
		{
			handle >>= startBitIndex;
			handle &= Constants.kBitmask;

			mHandle = handle;
		}
		#endregion

		#region Value properties
		[Contracts.Pure]
		public EngineBuildHandle Build { get {
			return new EngineBuildHandle(mHandle, Constants.kBuildShift);
		} }
		[Contracts.Pure]
		public int TargetPlatformIndex { get {
			return EngineTargetPlatform.BitDecodeIndex(mHandle, Constants.kTargetPlatformShift);
		} }
		[Contracts.Pure]
		public int ResourceModelIndex { get {
			return EngineRegistry.BitDecodeResourceModelIndex(mHandle, Constants.kResourceModelShift);
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

			return string.Format("[{0}\t{1}\t{2}]",
				Build.ToString(),
				TargetPlatform,
				ResourceModelIndex.ToString());
		}
		#endregion

		/// <summary>Creates a string of the build component name ids separated by periods</summary>
		/// <returns>Empty string if this <see cref="IsNone"/></returns>
		/// <remarks>If the <see cref="Branch"/>'s display name is the same as <see cref="Engine"/>, the former isn't included in the output</remarks>
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
				sb.AppendFormat(".{0}", platform.ToString());

				#region ResourceModel
				if (rsrc_model_index.IsNotNone())
				{
					var rsrc_model = EngineRegistry.ResourceModels[rsrc_model_index];
					sb.AppendFormat(".{0}", rsrc_model);
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