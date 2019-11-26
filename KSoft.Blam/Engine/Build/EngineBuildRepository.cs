using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Engine
{
	public sealed class EngineBuildRepository
		: IO.ITagElementStringNameStreamable
	{
		/// <summary>Engine this repository defines builds for</summary>
		public BlamEngine Engine { get; private set; }
		/// <summary>The underlying general engine's Guid</summary>
		public Values.KGuid Guid { get; private set; }

		#region ValidTargetPlatforms
		Collections.BitSet mValidTargetPlatforms;
		/// <summary>Platforms which all builds in this repository can target</summary>
		public Collections.IReadOnlyBitSet ValidTargetPlatforms { get {
			if (mValidTargetPlatforms == null)
				return EngineRegistry.NullValidTargetPlatforms;

			return mValidTargetPlatforms;
		} }
		#endregion

		#region Branches
		public List<EngineBuildBranch> Branches { get; private set; }

		static int BranchIdResolver(EngineBuildRepository repo, string name)
		{
			int id = TypeExtensions.kNone;

			if (!string.IsNullOrEmpty(name))
			{
				id = repo.Branches.FindIndex(x => x.Name == name);

				if (id.IsNone())
					throw new KeyNotFoundException(string.Format("Engine {0} doesn't define a branch named {1}",
						repo.Engine.Name, name));
			}

			return id;
		}
		static readonly Func<EngineBuildRepository, string, int> BranchIdResolverSansKeyNotFoundException =
			(repo, name) => !string.IsNullOrEmpty(name)
				? repo.Branches.FindIndex(x => x.Name == name)
				: TypeExtensions.kNone;
		static readonly Func<EngineBuildRepository, int, string> BranchNameResolver =
			(repo, id) => id.IsNotNone()
				? repo.Branches[id].Name
				: null;
		#endregion

		public EngineBuildRepository()
		{
			Guid = Values.KGuid.Empty;
			Branches = new List<EngineBuildBranch>();
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var engine = KSoft.Debug.TypeCheck.CastReference<BlamEngine>(s.Owner);
			if (s.IsReading)
				Engine = engine;
			else
				Contract.Assert(engine == Engine);

			using (s.EnterCursorBookmark("Repository"))
			using (s.EnterUserDataBookmark(this))
			{
				s.StreamElement("Guid", this, obj => obj.Guid);

				EngineTargetPlatform.SerializeBitSet(s, ref mValidTargetPlatforms, "ValidTargetPlatforms");

				using (var bm = s.EnterCursorBookmarkOpt("Branches", Branches, Predicates.HasItems)) if (bm.IsNotNull)
					s.StreamableElements("Branch", Branches);
			}

			if (s.IsReading)
			{
				KSoft.Debug.ValueCheck.IsLessThanEqualTo("Too many registered engine branches in " + Engine.Name,
					EngineBuildBranch.kMaxCount, Branches.Count);

				Branches.TrimExcess();
			}
		}

		internal bool SerializeBranchId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref int branchId, bool isOptional = false)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = true;

			if (isOptional)
			{
				streamed = s.StreamAttributeOptIdAsString(attributeName, ref branchId, this,
					BranchIdResolver, BranchNameResolver, Predicates.IsNotNullOrEmpty);

				if (!streamed && s.IsReading)
					branchId = TypeExtensions.kNone;
			}
			else
			{
				s.StreamAttributeIdAsString(attributeName, ref branchId, this,
					BranchIdResolver, BranchNameResolver);
			}

			return streamed;
		}
		#endregion

		#region Index/Id interfaces
		internal void InitializeBuildHandles()
		{
			int engine_index = Engine.RootBuildHandle.EngineIndex;
			foreach (var branch in Branches)
			{
				int branch_index = BranchIdResolver(this, branch.Name);
				branch.InitializeBuildHandles(engine_index, branch_index);
			}
		}
		internal EngineBuildBranch ResolveWellKnownEngineBranch(string branchName)
		{
			int branch_index = BranchIdResolverSansKeyNotFoundException(this, branchName);

			return branch_index.IsNone()
				? null
				: Branches[branch_index];
		}
		#endregion
	};
}
/*
Repo
	-Halo1
		Branch
			-Halo1
			-HaloCE
			-HA10
			-Stubbs
			-Shadowrun
	-Halo2
	-Halo3
		Branch
			-Halo3
			-HaloOdst
	-HaloReach
		Branch
			-HaloReach
	-Halo4
		Branch
			-Halo4
*/