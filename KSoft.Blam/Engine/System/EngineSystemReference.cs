using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.Engine
{
	/// <summary>Represents a lightweight reference to a <see cref="EngineSystemBase"/></summary>
	/// <remarks>Should be declared in a using() statement, or as an object member that implements IDisposable (and disposes this)</remarks>
	public struct EngineSystemReference
		: IDisposable
	{
		public static readonly EngineSystemReference None = new EngineSystemReference(true);

		EngineSystemBase mSystem;
		readonly EngineBuildHandle mBuildHandle;

		/// <summary>The system that is referenced</summary>
		public EngineSystemBase System { get { return mSystem; } }

		#region Ctor
		EngineSystemReference(bool dummy)
		{
			mSystem = null;
			mBuildHandle = EngineBuildHandle.None;
		}
		internal EngineSystemReference(EngineSystemBase system, EngineBuildHandle buildHandle)
		{
			Contract.Assume(system != null);
			Contract.Assume(!buildHandle.IsNone);

			mSystem = system;
			mBuildHandle = buildHandle;

			system.AddReference(buildHandle);
		}
		#endregion

		/// <summary>Has this reference not yet been disposed of?</summary>
		[Contracts.Pure]
		public bool IsValid { get {
			return mSystem != null;
		} }

		#region IDisposable Members
		public void Dispose()
		{
			if (mSystem != null)
			{
				mSystem.RemoveReference(mBuildHandle);
				mSystem = null;
			}
		}
		#endregion

		/// <summary>Access the reference's underlying <see cref="System"/>. Only use in temporary copies and calls!</summary>
		/// <param name="reference"></param>
		/// <returns></returns>
		public static implicit operator EngineSystemBase(EngineSystemReference reference)
		{
			return reference.System;
		}
	};

	/// <summary>Represents a lightweight reference to a specific <see cref="EngineSystemBase"/> implementation</summary>
	/// <remarks>Should be declared in a using() statement, or as an object member that implements IDisposable (and disposes this)</remarks>
	public struct EngineSystemReference<T>
		: IDisposable
		where T : EngineSystemBase
	{
		public static readonly EngineSystemReference<T> None = new EngineSystemReference<T>(true);

		T mSystem;
		readonly EngineBuildHandle mBuildHandle;

		/// <summary>The system that is referenced</summary>
		public T System { get { return mSystem; } }

		#region Ctor
		EngineSystemReference(bool dummy)
		{
			mSystem = null;
			mBuildHandle = EngineBuildHandle.None;
		}
		internal EngineSystemReference(T system, EngineBuildHandle buildHandle)
		{
			Contract.Assume(system != null);
			Contract.Assume(!buildHandle.IsNone);

			mSystem = system;
			mBuildHandle = buildHandle;

			system.AddReference(buildHandle);
		}
		#endregion

		/// <summary>Has this reference not yet been disposed of?</summary>
		[Contracts.Pure]
		public bool IsValid { get {
			return mSystem != null;
		} }

		#region IDisposable Members
		public void Dispose()
		{
			if (mSystem != null)
			{
				mSystem.RemoveReference(mBuildHandle);
				mSystem = null;
			}
		}
		#endregion

		/// <summary>Access the reference's underlying <see cref="System"/>. Only use in temporary copies and calls!</summary>
		/// <param name="reference"></param>
		/// <returns></returns>
		public static implicit operator T(EngineSystemReference<T> reference)
		{
			return reference.System;
		}
	};
}