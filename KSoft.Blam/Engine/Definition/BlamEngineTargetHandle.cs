using System;
using System.Collections.Generic;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Blam.Engine
{
	using BitFieldTraits = Bitwise.BitFieldTraits;

	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	[System.Diagnostics.DebuggerDisplay("Engine# = {Build.EngineIndex}, TargetPlatform# = {TargetPlatformIndex}, ResourceModel# = {ResourceModelIndex}")]
	public readonly struct BlamEngineTargetHandle
		: IComparer<BlamEngineTargetHandle>, System.Collections.IComparer // #REMOVE_BLAM
		, IComparable<BlamEngineTargetHandle>, IComparable
		, IEquatable<BlamEngineTargetHandle>
	{
		#region Constants
		// nesting these into a static class makes them run before the struct's static ctor...
		// which, being a value type cctor, may not run when we want it
		static class Constants
		{
			public static readonly BitFieldTraits kResourceModelBitField =
				new(EngineRegistry.kResourceModelBitCount);
			public static readonly BitFieldTraits kTargetPlatformBitField =
				new(EngineTargetPlatform.kIndexBitCount, kResourceModelBitField);
			public static readonly BitFieldTraits kBuildBitField =
				new(EngineBuildHandle.BitCount, kTargetPlatformBitField);

			public static readonly BitFieldTraits kLastBitField =
				kBuildBitField;
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		/// <remarks>16 bits at last count</remarks>
		public static int BitCount => Constants.kLastBitField.FieldsBitCount;
		public static uint Bitmask => Constants.kLastBitField.FieldsBitmask.u32;

		public static readonly BlamEngineTargetHandle None = new();
		#endregion

		#region Internal Value
		[Interop.FieldOffset(0)] readonly uint mHandle;

		//internal uint Handle { get { return mHandle; } }

		static void InitializeHandle(out uint handle,
			EngineBuildHandle buildHandle, int platformIndex, int resourceModelIndex)
		{
			if (platformIndex.IsNone() && resourceModelIndex.IsNone())
			{
				resourceModelIndex = 0;
			}

			var encoder = new Bitwise.HandleBitEncoder();
			EngineRegistry.BitEncodeResourceModelIndex(ref encoder, resourceModelIndex);
			EngineTargetPlatform.BitEncodeIndex(ref encoder, platformIndex);
			encoder.Encode32(buildHandle.Handle, EngineBuildHandle.Bitmask);

			if (encoder.UsedBitCount != BlamEngineTargetHandle.BitCount)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Encoded engine target handle used {0} bits; expected {1}.",
					encoder.UsedBitCount, BlamEngineTargetHandle.BitCount));
			}

			handle = encoder.GetHandle32();
		}
		#endregion

		#region Ctor
		public BlamEngineTargetHandle(EngineBuildHandle buildHandle, int platformIndex, int resourceModelIndex)
		{
			if (!EngineTargetPlatform.IsValidIndex(platformIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(platformIndex));
			}
			if (!EngineRegistry.IsValidResourceModelIndex(resourceModelIndex) &&
				!(platformIndex.IsNone() && resourceModelIndex.IsNone()))
			{
				throw new ArgumentOutOfRangeException(nameof(resourceModelIndex));
			}

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
		public EngineBuildHandle Build => new(mHandle, Constants.kBuildBitField);
		public int TargetPlatformIndex => EngineTargetPlatform.BitDecodeIndex(mHandle, Constants.kTargetPlatformBitField.BitIndex);
		public int ResourceModelIndex => EngineRegistry.BitDecodeResourceModelIndex(mHandle, Constants.kResourceModelBitField.BitIndex);

		public EngineTargetPlatform TargetPlatform { get {
			int index = TargetPlatformIndex;

			return index.IsNotNone()
				? EngineRegistry.TargetPlatforms[index]
				: null;
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
			if (obj is BlamEngineTargetHandle objHandle)
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
		/// <summary>Returns a string representation of this object</summary>
		/// <returns>"[Build\tTargetPlatform\tResourceModelIndex]"</returns>
		public override string ToString()
		{
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
		public string ToDisplayString()
		{
			if (IsNone)
			{
				return TypeExtensions.kNoneDisplayString;
			}

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
			KSoft.Debug.TypeCheck.CastValue(x, out BlamEngineTargetHandle _x);
			KSoft.Debug.TypeCheck.CastValue(y, out BlamEngineTargetHandle _y);

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
			KSoft.Debug.TypeCheck.CastValue(obj, out BlamEngineTargetHandle _obj);

			return BlamEngineTargetHandle.StaticCompare(this, _obj);
		}
		#endregion

		#region IEquatable<BlamEngineTargetHandle> Members
		/// <summary>See <see cref="IEquatable{T}.Equals"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(BlamEngineTargetHandle other) => this.mHandle == other.mHandle;
		#endregion

		#region Operators
		public static bool operator==(BlamEngineTargetHandle lhs, BlamEngineTargetHandle rhs) => lhs.mHandle == rhs.mHandle;
		public static bool operator!=(BlamEngineTargetHandle lhs, BlamEngineTargetHandle rhs) => lhs.mHandle != rhs.mHandle;
		#endregion


		#region Util
		static int StaticCompare(BlamEngineTargetHandle lhs, BlamEngineTargetHandle rhs)
		{
			// #TODO figure out a a utility to do this generically for bit-encoded handles that can run
			// in the internal Constants class.
			if (BlamEngineTargetHandle.BitCount >= Bits.kInt32BitCount)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Handle bit count must be less than {0}; actual bit count is {1}.",
					Bits.kInt32BitCount, BlamEngineTargetHandle.BitCount));
			}

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
			int platform_index = reading
				? TypeExtensions.kNone
				: value.TargetPlatformIndex;
			int rsrc_model_index = reading
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
