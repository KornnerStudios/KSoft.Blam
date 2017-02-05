using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.Engine
{
	/// <summary>Base class for a specific system that an engine requires (eg, Blobs)</summary>
	public abstract class EngineSystemBase
	{
		/// <summary>The <see cref="BlamEngineSystem"/> used to describe this instance</summary>
		internal BlamEngineSystem Prototype { get; private set; }

		// NOTE: will always be null if !Prototype.SystemRequiresReferenceTracking
		Dictionary<EngineBuildHandle, int> mReferencesByBuildCounts;

		protected EngineSystemBase()
		{
		}

		public BlamEngine Engine { get { return Prototype.Engine; } }

		#region Reference counting
		int UpdateReferencesByHandle(int amount, EngineBuildHandle handle)
		{
			if (!handle.IsNone)
				return mReferencesByBuildCounts[handle] += amount;

			return 0;
		}
		void UpdateReferencesByBuild(int amount, EngineBuildHandle buildHandle)
		{
			EngineBuildHandle engine, branch;
			buildHandle.ExtractHandles(out engine, out branch);

			int revisn_count, branch_count, engine_count;
			lock (mReferencesByBuildCounts)
			{
				revisn_count = UpdateReferencesByHandle(amount, buildHandle);
				branch_count = UpdateReferencesByHandle(amount, branch);
				engine_count = UpdateReferencesByHandle(amount, engine);
			}

			// Validate that a RemoveReference hasn't made a count go negative (ie, there was an extra call)
			if (amount < 0)
			{
				if (revisn_count < 0 || branch_count < 0 || engine_count < 0)
					throw new InvalidOperationException(string.Format(
						"Extra or bad RemoveReference call detected for {0} using the handle {1}"));
			}
		}
		void InitializeReferencesByBuildCounts()
		{
			if (Prototype.SystemRequiresReferenceTracking)
				mReferencesByBuildCounts = new Dictionary<EngineBuildHandle, int>();
		}

		internal void AddReference(EngineBuildHandle buildHandle)
		{
			Contract.Requires<ArgumentNullException>(!buildHandle.IsNone);

			bool load_externs = true;
			if (Prototype.SystemRequiresReferenceTracking)
			{
				bool no_refs_before_update = mReferencesByBuildCounts.Count == 0;
				UpdateReferencesByBuild(+1, buildHandle);
				load_externs = !no_refs_before_update;
			}

			if (load_externs)
				LoadExternsBegin();
		}
		internal void RemoveReference(EngineBuildHandle buildHandle)
		{
			Contract.Requires<ArgumentNullException>(!buildHandle.IsNone);

			bool unload_externs = false;
			if (Prototype.SystemRequiresReferenceTracking)
			{
				UpdateReferencesByBuild(-1, buildHandle);
				unload_externs = mReferencesByBuildCounts.Count == 0;
			}

			if (unload_externs)
				UnloadExternsData();
		}

		/// <summary>Create a new reference to this system using an existing build handle</summary>
		/// <param name="buildHandle">The engine build which is in need of this system</param>
		/// <returns></returns>
		public EngineSystemReference NewReference(EngineBuildHandle buildHandle)
		{
			return new EngineSystemReference(this, buildHandle);
		}
		/// <summary>Create a new reference to this system using <see cref="Engine"/>'s root build handle</summary>
		/// <returns></returns>
		public EngineSystemReference NewReference()
		{
			return new EngineSystemReference(this, Engine.RootBuildHandle);
		}
		#endregion

		void LoadExternsBegin()
		{
		}
		protected virtual void UnloadExternsData()
		{
			Contract.Requires(!Prototype.SystemMetadata.KeepExternsLoaded);
		}

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

		internal void SerializeExtern<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var prototype = KSoft.Debug.TypeCheck.CastReference<BlamEngineSystem>(s.Owner);
			if (s.IsReading)
				Prototype = prototype;
			else
				Contract.Assert(prototype == Prototype);

			var build_handle = Engine.RootBuildHandle;
			EngineBuildHandle.Serialize(s, ref build_handle);

			using (s.EnterUserDataBookmark(this))
			{
				SerializeExternBody(s);
			}

			if (s.IsReading)
			{
				int expected_engine_index = Engine.RootBuildHandle.EngineIndex;
				int actual_engine_index = build_handle.EngineIndex;
				KSoft.Debug.ValueCheck.AreEqual(
					string.Format("{0} definition for {1} uses wrong engine id",
						GetType().Name, Engine.Name),
					expected_engine_index, actual_engine_index);

				InitializeReferencesByBuildCounts();
			}
		}
		#endregion
	};
}