using System;
using System.Collections.Generic;

namespace KSoft.Blam.Engine
{
	public sealed class EngineBuildBranch
		: IO.ITagElementStringNameStreamable
	{
		#region Constants
		internal const int kMaxCount = 8 - 1; // 3 bits. per repository
		internal static readonly int kIndexBitCount;
		private static readonly uint kIndexBitMask = Bits.GetNoneableEncodingTraits(kMaxCount,
			out kIndexBitCount);
		#endregion

		public EngineBuildRepository Repository { get; private set; } = null!;

		public EngineBuildHandle BranchHandle { get; private set; } = EngineBuildHandle.None;

		public string Name { get; private set; } = string.Empty;
		public string ProjectName { get; private set; } = string.Empty;

		#region ValidTargetPlatforms
		Collections.BitSet? mValidTargetPlatforms;
		/// <summary>Platforms which all builds in this branch can target</summary>
		public Collections.IReadOnlyBitSet ValidTargetPlatforms { get {
			if (mValidTargetPlatforms == null)
			{
				return Repository.ValidTargetPlatforms;
			}

			return mValidTargetPlatforms;
		} }
		#endregion

		#region Revisions
		public List<EngineBuildRevision> Revisions { get; private set; } = new();

		static int RevisionIdResolver(EngineBuildBranch branch, int version)
		{
			int id = TypeExtensions.kNone;

			if (version.IsNotNone())
			{
				id = branch.Revisions.FindIndex(x => x.Version == version);

				if (id.IsNone())
				{
					throw new KeyNotFoundException(string.Format(Util.InvariantCultureInfo,
						"Engine branch {0} doesn't define a revision for version #{1}",
						branch.Name, version));
				}
			}

			return id;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		static readonly Func<EngineBuildBranch, int/*version*/, int> RevisionIdResolverSansKeyNotFoundException =
			(branch, version) => version.IsNotNone()
				? branch.Revisions.FindIndex(x => x.Version == version)
				: TypeExtensions.kNone;
		static readonly Func<EngineBuildBranch, int, int/*version*/> RevisionNameResolver =
			(branch, id) => id.IsNotNone()
				? branch.Revisions[id].Version
				: TypeExtensions.kNone;
		#endregion

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object? obj)
		{
			return object.ReferenceEquals(this, obj);
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on its exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode()
		{
			if (BranchHandle.IsNone)
			{
				throw new InvalidOperationException(
					"Requested the hash code before the branch build data was fully initialized.");
			}

			return BranchHandle.GetHashCode();
		}
		/// <summary>Returns <see cref="Name"/></summary>
		/// <returns></returns>
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
			var repo = KSoft.Debug.TypeCheck.CastReference<EngineBuildRepository>(s.UserData!);
			if (s.IsReading)
			{
				Repository = repo;
			}
			else
			{
				if (!object.ReferenceEquals(repo, Repository))
				{
					throw new InvalidOperationException("Branch repository context does not match the serialized repository.");
				}
			}

			using (s.EnterUserDataBookmark(this))
			{
				s.StreamAttribute("name", this, obj => obj.Name);
				s.StreamAttribute("project", this, obj => obj.ProjectName);

				EngineTargetPlatform.SerializeBitSet(s, ref mValidTargetPlatforms, "ValidTargetPlatforms");

				using (var bm = s.EnterCursorBookmarkOpt("Revisions", Revisions, Predicates.HasItems))
				{
					if (bm.IsNotNull)
					{
						s.StreamableElements("Rev", Revisions);
					}
				}
			}

			if (s.IsReading)
			{
				KSoft.Debug.ValueCheck.IsLessThanEqualTo("Too many registered branch revisions in " + Name,
					EngineBuildRevision.kMaxCount, Revisions.Count);

				Revisions.TrimExcess();
			}
		}

		internal bool SerializeRevisionId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref int revisionId, bool isOptional = false)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = true;

			if (isOptional)
			{
				streamed = s.StreamAttributeOptIdAsInt32(attributeName, ref revisionId, this,
					RevisionIdResolver, RevisionNameResolver, Predicates.IsNotNone);

				if (!streamed && s.IsReading)
				{
					revisionId = TypeExtensions.kNone;
				}
			}
			else
			{
				s.StreamAttributeIdAsInt32(attributeName, ref revisionId, this,
					RevisionIdResolver, RevisionNameResolver);
			}

			return streamed;
		}
		#endregion

		#region Bit encoding
		internal static void BitEncodeIndex(ref Bitwise.HandleBitEncoder encoder, int branchIndex)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(branchIndex, TypeExtensions.kNone);

			encoder.EncodeNoneable32(branchIndex, kIndexBitMask);
		}
		internal static int BitDecodeIndex(uint handle, int bitIndex)
		{
			int index = Bits.BitDecodeNoneable(handle, bitIndex, kIndexBitMask);

			if (!index.IsNoneOrPositive())
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Decoded branch index must be NONE or non-negative; actual value is {0}.", index));
			}

			return index;
		}
		#endregion

		#region Index/Id interfaces
		/// <summary>Initialize the <see cref="BranchHandle"/> and the handles for all <see cref="Revisions"/></summary>
		/// <param name="engineIndex"></param>
		/// <param name="branchIndex"></param>
		internal void InitializeBuildHandles(int engineIndex, int branchIndex)
		{
			BranchHandle = EngineBuildHandle.Create(engineIndex, branchIndex);
			foreach (EngineBuildRevision revision in Revisions)
			{
				int revisn_index = RevisionIdResolver(this, revision.Version);
				var handle = EngineBuildHandle.Create(engineIndex, branchIndex, revisn_index);
				revision.InitializeBuildHandle(handle);
			}
		}
		#endregion
	};
}
