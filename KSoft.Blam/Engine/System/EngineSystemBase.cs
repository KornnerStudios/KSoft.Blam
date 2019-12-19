using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Engine
{
	// Systems are kept around for all eternity unless their prototype says they require ref tracking.
	// - If they're not ref tracked, then their extern data will also persist in memory
	// - If they are ref tracked, then their externs are unloaded after the final reference is removed

	// #REVIEW_BLAM: need to rethink some of the underlying lifecycle logic...can't recall everything I was
	// brainstorming months ago when I last touched this
	// 2019 NOTE: 'months ago', circa 2014

	/// <summary>Base class for a specific system that an engine requires (eg, Blobs)</summary>
	public abstract class EngineSystemBase
		: IDisposable
	{
		static readonly TimeSpan kWaitForExternIOTimeout = TimeSpan.FromSeconds(30);

		/// <summary>The <see cref="BlamEngineSystem"/> used to describe this instance</summary>
		internal BlamEngineSystem Prototype { get; private set; }

		// #NOTE_BLAM: will always be null if !Prototype.SystemRequiresReferenceTracking
		Dictionary<EngineBuildHandle, int> mReferencesByBuildCounts;
		int mNonBuildSpecificReferences;
		bool mActiveInBlamEngine;
		EngineBuildHandle mRootBuildHandleBaseline;

		protected EngineSystemBase()
		{
		}
		/// <summary>Only call me if you are <see cref="EngineSystemAttribute.NewInstance"/></summary>
		/// <param name="prototype">The <see cref="BlamEngineSystem"/> used to describe this instance</param>
		internal void InitializeForNewInstance(BlamEngineSystem prototype)
		{
			Prototype = prototype;
			mActiveInBlamEngine = true;
			mRootBuildHandleBaseline = Prototype.Engine.RootBuildHandle;

			// ReSharper disable once InconsistentlySynchronizedField - method is construction, don't need a lock
			if (Prototype.SystemRequiresReferenceTracking)
				mReferencesByBuildCounts = new Dictionary<EngineBuildHandle, int>();
		}

		#region IDisposable Members
		protected virtual void Dispose(bool disposing)
		{
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		public BlamEngine Engine { get { return Prototype.Engine; } }
		public EngineBuildHandle RootBuildHandle { get { return mRootBuildHandleBaseline; } }

		#region Reference counting
		enum UpdateReferenceSideEffect
		{
			/// <summary>No real side effects; just ref counts updated</summary>
			None,
			/// <summary>A new build just got its first reference</summary>
			NewBuildReferenced,
			/// <summary>An existing build just closed its final reference</summary>
			OldBuildUnreferenced,
		}
		int UpdateReferencesByHandle(int amount, EngineBuildHandle handle, ref UpdateReferenceSideEffect sideEffect)
		{
			if (Prototype.SystemRequiresReferenceTracking)
			{
				if (!handle.IsNone)
				{
					int ref_count;
					if (!mReferencesByBuildCounts.TryGetValue(handle, out ref_count))
						mReferencesByBuildCounts[handle] = 0;

					int ref_count_update = ref_count + amount;
					if (sideEffect == UpdateReferenceSideEffect.None)
					{
						if (ref_count_update == 0)	// the update closed all existing references...
							sideEffect = UpdateReferenceSideEffect.OldBuildUnreferenced;
						else if (ref_count == 0)	// existing count is 0, this is the first reference
							sideEffect = UpdateReferenceSideEffect.NewBuildReferenced;
					}

					return mReferencesByBuildCounts[handle] = ref_count_update;
				}
			}
			else
			{
				int ref_count = Interlocked.CompareExchange(ref mNonBuildSpecificReferences, 0, 0);
				int ref_count_update = Interlocked.Add(ref mNonBuildSpecificReferences, amount);
				if (sideEffect == UpdateReferenceSideEffect.None)
				{
					if (ref_count_update == 0)	// the update closed all existing references...
						sideEffect = UpdateReferenceSideEffect.OldBuildUnreferenced;
					else if (ref_count == 0)	// existing count is 0, this is the first reference
						sideEffect = UpdateReferenceSideEffect.NewBuildReferenced;
				}

				return ref_count_update;
			}

			return 0;
		}
		UpdateReferenceSideEffect UpdateReferencesByBuild(int amount, EngineBuildHandle buildHandle)
		{
			var side_effect = UpdateReferenceSideEffect.None;

			if (Prototype.SystemRequiresReferenceTracking)
			{
				EngineBuildHandle engine, branch;
				buildHandle.ExtractHandles(out engine, out branch);

				int revisn_count, branch_count, engine_count;
				lock (mReferencesByBuildCounts)
				{
					revisn_count = UpdateReferencesByHandle(amount, buildHandle, ref side_effect);
					branch_count = UpdateReferencesByHandle(amount, branch, ref side_effect);
					engine_count = UpdateReferencesByHandle(amount, engine, ref side_effect);
				}

				// Validate that a RemoveReference hasn't made a count go negative (ie, there was an extra call)
				if (amount < 0)
				{
					if (revisn_count < 0 || branch_count < 0 || engine_count < 0)
					{
						throw new InvalidOperationException(string.Format(
							"Extra or bad RemoveReference call detected for {0} using the handle {1}",
							Prototype, buildHandle.ToDisplayString()));
					}
				}
			}
			else
			{
				int new_count = UpdateReferencesByHandle(amount, EngineBuildHandle.None, ref side_effect);

				// Validate that a RemoveReference hasn't made a count go negative (ie, there was an extra call)
				if (amount < 0)
				{
					if (new_count < 0)
					{
						throw new InvalidOperationException(string.Format(
							"Extra or bad RemoveReference call detected for {0}",
							Prototype));
					}
				}
			}

			return side_effect;
		}

		internal async Task AddReferenceAsync(EngineBuildHandle buildHandle)
		{
			Contract.Requires<ArgumentNullException>(!buildHandle.IsNone);
			Contract.Assert(mActiveInBlamEngine);

			// #REVIEW_BLAM: this isn't an optimal setup
			bool load_externs = true;
			var update_refs_side_effect = UpdateReferenceSideEffect.None;
			if (Prototype.SystemRequiresReferenceTracking)
			{
				load_externs = mReferencesByBuildCounts.Count == 0;
				update_refs_side_effect = UpdateReferencesByBuild(+1, buildHandle);
			}
			else
			{
				update_refs_side_effect = UpdateReferencesByBuild(+1, buildHandle);
				load_externs = update_refs_side_effect == UpdateReferenceSideEffect.NewBuildReferenced;
			}

			if (load_externs)
			{
				Contract.Assert(mExternIOTask == null);
				if (mExternIOTask != null)
				{
					throw new InvalidOperationException(string.Format(
						"Tried to perform an externs IO task while one was in flight under {0}",
						this.Prototype.Engine));
				}

				mExternIOTask = Task.Run((Action)LoadExternsBegin);
				await mExternIOTask;
				mExternIOTask = null;
			}

			// #REVIEW_BLAM: also don't do this if Prototype.SystemRequiresReferenceTracking==false?
			if (update_refs_side_effect == UpdateReferenceSideEffect.NewBuildReferenced)
				InitializeForNewBuildReference(buildHandle);
		}
		internal async Task RemoveReferenceAsync(EngineBuildHandle buildHandle)
		{
			Contract.Requires<ArgumentNullException>(!buildHandle.IsNone);

			bool unload_externs = !Prototype.SystemMetadata.KeepExternsLoaded;
			var update_refs_side_effect = UpdateReferenceSideEffect.None;
			if (Prototype.SystemRequiresReferenceTracking)
			{
				update_refs_side_effect = UpdateReferencesByBuild(-1, buildHandle);
				unload_externs = mReferencesByBuildCounts.Count == 0;

				// This will remove the EngineSystem from the active systems, meaning UnloadExterns will be safe
				// to call without locking or such as nothing can call LoadExterns on this object anymore
				if (unload_externs)
				{
					Engine.CloseSystem(this);
					mActiveInBlamEngine = false;
				}
			}
			else
			{
				update_refs_side_effect = UpdateReferencesByBuild(-1, buildHandle);
				unload_externs = update_refs_side_effect == UpdateReferenceSideEffect.OldBuildUnreferenced;

				// This will remove the EngineSystem from the active systems, meaning UnloadExterns will be safe
				// to call without locking or such as nothing can call LoadExterns on this object anymore
				if (unload_externs)
				{
					Engine.CloseSystem(this);
					mActiveInBlamEngine = false;
				}
			}

			if (unload_externs)
			{
				// mExternIOTask should previously be a Task for LoadExterns
				bool didntTimeout = WaitForExternsIO();
				Contract.Assert(didntTimeout);

				if (mExternIOTask != null)
				{
					throw new InvalidOperationException(string.Format(
						"Tried to perform an externs IO task while one was in flight under {0}",
						this.Prototype.Engine));
				}

				mExternIOTask = Task.Run((Action)UnloadExternsBegin);
				await mExternIOTask;
				mExternIOTask = null;
			}

			// #REVIEW_BLAM: also don't do this if Prototype.SystemRequiresReferenceTracking==false?
			// reminder: we're calling this AFTER externs are unloaded...be sure your dispose code handles this
			if (update_refs_side_effect == UpdateReferenceSideEffect.OldBuildUnreferenced)
				DisposeFromOldBuildReference(buildHandle);
		}

		/// <summary>Create a new reference to this system using an existing build handle</summary>
		/// <param name="buildHandle">The engine build which is in need of this system</param>
		/// <returns></returns>
		public EngineSystemReference NewReference(EngineBuildHandle buildHandle)
		{
			return new EngineSystemReference(this, buildHandle);
		}
		/// <summary>Create a new reference to this system using its <see cref="RootBuildHandle"/> (which may more specific than <see cref="Engine"/>'s root build handle)</summary>
		/// <returns></returns>
		public EngineSystemReference NewReference()
		{
			return new EngineSystemReference(this, this.RootBuildHandle);
		}
		#endregion

		/// <summary>Handle a specific build being referenced for the first time</summary>
		/// <param name="newBuildHandle">Handle that caused a build reference to be first created</param>
		protected virtual void InitializeForNewBuildReference(EngineBuildHandle newBuildHandle)
		{
		}
		/// <summary>Handle a specific build being unreferenced for the last time</summary>
		/// <param name="oldBuildHandle">Handle that caused a build reference to be disposed</param>
		protected virtual void DisposeFromOldBuildReference(EngineBuildHandle oldBuildHandle)
		{
		}

		#region Externs I/O
		Task mExternIOTask;

		/// <summary>Waits for any extern IO tasks to finish, returning false if the wait timed out after <see cref="kWaitForExternIOTimeout"/></summary>
		/// <returns>True if the task was successfully finished while waited on, or false if we timed out</returns>
		internal bool WaitForExternsIO()
		{
			bool success = true;

			if (mExternIOTask != null)
			{
				success = mExternIOTask.Wait(kWaitForExternIOTimeout);
				if (success)
					mExternIOTask = null;
			}

			return success;
		}

		void LoadExternsBegin()
		{
			using (var s = EngineRegistry.OpenEngineSystemTagElementStream(
				Engine, Prototype.SystemMetadata.Guid, Prototype.ExternsFile))
			{
				s.Owner = Prototype;
				SerializeExtern(s);
			}
		}
		void UnloadExternsBegin()
		{
			Contract.Requires(!Prototype.SystemMetadata.KeepExternsLoaded);

			UnloadExternsData();
		}

		protected virtual void UnloadExternsData()
		{
		}
		#endregion

		#region ITagElementStreamable<string> Members
		/// <summary></summary>
		/// <param name="s"></param>
		/// <remarks>
		/// <paramref name="s"/>'s UserData will be equal to <b>this</b> instance when called
		/// </remarks>
		protected virtual void SerializeExternBody<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
		}

		void SerializeExtern<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
/*			var prototype = KSoft.Debug.TypeCheck.CastReference<BlamEngineSystem>(s.Owner);
			if (s.IsReading)
				Prototype = prototype;
			else
				Contract.Assert(prototype == Prototype);*/

			EngineBuildHandle.Serialize(s, ref mRootBuildHandleBaseline);

			using (s.EnterUserDataBookmark(this))
			{
				SerializeExternBody(s);
			}

			if (s.IsReading)
			{
				int expected_engine_index = Engine.RootBuildHandle.EngineIndex;
				int actual_engine_index = RootBuildHandle.EngineIndex;
				KSoft.Debug.ValueCheck.AreEqual(
					string.Format("{0} definition for {1} uses wrong engine id",
						GetType().Name, Engine.Name),
					expected_engine_index, actual_engine_index);
			}
		}
		#endregion
	};
}