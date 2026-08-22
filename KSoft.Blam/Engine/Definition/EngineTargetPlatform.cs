using System;
using System.Collections.Generic;

namespace KSoft.Blam.Engine
{
	public sealed class EngineTargetPlatform
		: IO.ITagElementStringNameStreamable
	{
		#region Constants
		internal const int kMaxCount = 8 - 1; // 3 bits. per registry
		internal static readonly int kIndexBitCount;
		private static readonly uint kIndexBitMask = Bits.GetNoneableEncodingTraits(kMaxCount,
			out kIndexBitCount);
		#endregion

		/// <summary>Index this object appears in the global registry</summary>
		public int TargetPlatformIndex { get; internal set; } = TypeExtensions.kNone;

		public string Name { get; private set; } = string.Empty;

		Shell.Platform mPlatform = Shell.Platform.Undefined;
		public Shell.Platform Platform => mPlatform;

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is EngineTargetPlatform objPlatform)
			{
				return this.Name == objPlatform.Name;
			}

			return false;
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on its exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode() => Name.GetHashCode();
		/// <summary>Returns a string representation of this object</summary>
		/// <returns><see cref="Name"/></returns>
		public override string ToString()
		{
			return Name;
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("name", this, obj => obj.Name);
			IO.TagElementStreamDefaultSerializer.Serialize(s, ref mPlatform);
		}
		#endregion

		#region Bit encoding
		internal static void BitEncodeIndex(ref Bitwise.HandleBitEncoder encoder, int targetPlatformIndex)
		{
			if (!IsValidIndex(targetPlatformIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(targetPlatformIndex));
			}

			encoder.EncodeNoneable32(targetPlatformIndex, kIndexBitMask);
		}
		internal static int BitDecodeIndex(uint handle, int bitIndex)
		{
			int index = Bits.BitDecodeNoneable(handle, bitIndex, kIndexBitMask);

			if (!IsValidIndex(index))
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Decoded target platform index must be NONE or in the range [0, {0}); actual value is {1}.",
					EngineRegistry.TargetPlatforms.Count, index));
			}

			return index;
		}
		#endregion

		#region Index/Id interfaces
		public static bool IsValidIndex(int targetPlatformIndex)
			=> targetPlatformIndex.IsNoneOrPositive() && targetPlatformIndex < EngineRegistry.TargetPlatforms.Count;

		static int TargetPlatformIdResolver(object _null, string name)
		{
			int id = TypeExtensions.kNone;

			if (!string.IsNullOrEmpty(name))
			{
				id = EngineRegistry.TargetPlatforms.FindIndex(x => x.Name == name);

				if (id.IsNone())
				{
					throw new KeyNotFoundException(string.Format(Util.InvariantCultureInfo,
						"No target platform is registered with the name '{0}'",
						name));
				}
			}

			return id;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		static readonly Func<object, string, int> TargetPlatformIdResolverSansKeyNotFoundException =
			(_null, name) => !string.IsNullOrEmpty(name)
				? EngineRegistry.TargetPlatforms.FindIndex(x => x.Name == name)
				: TypeExtensions.kNone;
		static readonly Func<object, int, string> TargetPlatformNameResolver =
			(_null, id) => id.IsNotNone()
				? EngineRegistry.TargetPlatforms[id].Name
				: null;

		internal static bool SerializeId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref int targetPlatformId, bool isOptional = false)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = true;

			if (isOptional)
			{
				streamed = s.StreamAttributeOptIdAsString(attributeName, ref targetPlatformId, null,
					TargetPlatformIdResolver, TargetPlatformNameResolver, Predicates.IsNotNullOrEmpty);

				if (!streamed && s.IsReading)
				{
					targetPlatformId = TypeExtensions.kNone;
				}
			}
			else
			{
				s.StreamAttributeIdAsString(attributeName, ref targetPlatformId, null,
					TargetPlatformIdResolver, TargetPlatformNameResolver);
			}

			return streamed;
		}

		static int SerializeBitIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BitSet bitset, int bitIndex, object _null)
			where TDoc : class
			where TCursor : class
		{
			if (s.IsReading)
			{
				string platform_name = null;
				s.ReadCursor(ref platform_name);
				bitIndex = TargetPlatformIdResolver(null, platform_name);
			}
			else if (s.IsWriting)
			{
				string platform_name = TargetPlatformNameResolver(null, bitIndex);
				s.WriteCursor(platform_name);
			}

			return bitIndex;
		}
		internal static void SerializeBitSet<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			ref Collections.BitSet bitset, string setElementName, string bitElementName = "Platform")
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt(setElementName, bitset, Predicates.HasBits))
			{
				if (bm.IsNotNull)
				{
					if (s.IsReading)
					{
						bitset = new Collections.BitSet(EngineRegistry.TargetPlatforms.Count);
					}

					bitset.Serialize(s, bitElementName, (object)null, SerializeBitIndex);
				}
			}
		}
		#endregion
	};
}
