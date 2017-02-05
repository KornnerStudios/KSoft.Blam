using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.Engine
{
	public sealed class EngineBuildBranch
		: IO.ITagElementStringNameStreamable
	{
		#region Constants
		internal const int kMaxCount = 8 - 1; // 3 bits. per repository
		internal static readonly int kIndexBitCount;
		private static readonly uint kIndexBitMask;

		static EngineBuildBranch()
		{
			kIndexBitMask = Bits.GetNoneableEncodingTraits(kMaxCount,
				out kIndexBitCount);
		}
		#endregion

		public EngineBuildRepository Repository { get; private set; }

		public EngineBuildHandle BranchHandle { get; private set; }

		public string Name { get; private set; }
		public string ProjectName { get; private set; }

		#region ValidTargetPlatforms
		Collections.BitSet mValidTargetPlatforms;
		/// <summary>Platforms which all builds in this branch can target</summary>
		public Collections.IReadOnlyBitSet ValidTargetPlatforms { get {
			if (mValidTargetPlatforms == null)
				return Repository.ValidTargetPlatforms;

			return mValidTargetPlatforms;
		} }
		#endregion

		#region Revisions
		public List<EngineBuildRevision> Revisions { get; private set; }

		static int RevisionIdResolver(EngineBuildBranch branch, int version)
		{
			int id = TypeExtensions.kNone;

			if (version.IsNotNone())
			{
				id = branch.Revisions.FindIndex(x => x.Version == version);

				if (id.IsNone())
					throw new KeyNotFoundException(string.Format("Engine branch {0} doesn't define a revision for version #{1}",
						branch.Name, version));
			}

			return id;
		}
		static readonly Func<EngineBuildBranch, int/*version*/, int> RevisionIdResolverSansKeyNotFoundException =
			(branch, version) => version.IsNotNone()
				? branch.Revisions.FindIndex(x => x.Version == version)
				: TypeExtensions.kNone;
		static readonly Func<EngineBuildBranch, int, int/*version*/> RevisionNameResolver =
			(branch, id) => id.IsNotNone()
				? branch.Revisions[id].Version
				: TypeExtensions.kNone;
		#endregion

		public EngineBuildBranch()
		{
			BranchHandle = EngineBuildHandle.None;

			Name = ProjectName =
				"";

			Revisions = new List<EngineBuildRevision>();
		}

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return object.ReferenceEquals(this, obj);
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on its exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode()
		{
			Contract.Assert(!BranchHandle.IsNone,
				"Requested the hash code before the build data was fully initialized");

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
			var repo = KSoft.Debug.TypeCheck.CastReference<EngineBuildRepository>(s.UserData);
			if (s.IsReading)
				Repository = repo;
			else
				Contract.Assert(repo == Repository);

			using (s.EnterUserDataBookmark(this))
			{
				s.StreamAttribute("name", this, obj => obj.Name);
				s.StreamAttribute("project", this, obj => obj.ProjectName);

				EngineTargetPlatform.SerializeBitSet(s, ref mValidTargetPlatforms, "ValidTargetPlatforms");

				using (var bm = s.EnterCursorBookmarkOpt("Revisions", Revisions, Predicates.HasItems)) if (bm.IsNotNull)
					s.StreamableElements("Rev", Revisions);
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
					revisionId = TypeExtensions.kNone;
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
			Contract.Requires<ArgumentOutOfRangeException>(branchIndex.IsNoneOrPositive());

			encoder.EncodeNoneable32(branchIndex, kIndexBitMask);
		}
		internal static int BitDecodeIndex(uint handle, int bitIndex)
		{
			var index = Bits.BitDecodeNoneable(handle, bitIndex, kIndexBitMask);

			Contract.Assert(index.IsNoneOrPositive());
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
			foreach (var revisn in Revisions)
			{
				int revisn_index = RevisionIdResolver(this, revisn.Version);
				var handle = EngineBuildHandle.Create(engineIndex, branchIndex, revisn_index);
				revisn.InitializeBuildHandle(handle);
			}
		}
		#endregion
	};
}