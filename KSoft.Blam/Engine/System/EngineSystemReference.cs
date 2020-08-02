using System;
using System.Diagnostics.CodeAnalysis;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Engine
{
	/// <summary>Represents a lightweight reference to a <see cref="EngineSystemBase"/></summary>
	/// <remarks>Should be declared in a using() statement, or as an object member that implements IDisposable (and disposes this)</remarks>
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct EngineSystemReference
		: IDisposable
	{
		public static readonly EngineSystemReference None = new EngineSystemReference(true);

		EngineSystemBase mSystem;
		readonly EngineBuildHandle mBuildHandle;

		/// <summary>The system that is referenced</summary>
		public EngineSystemBase System { get {
			if (IsValid)
			{
				bool didntTimeout = mSystem.WaitForExternsIO();
				Contract.Assert(didntTimeout);
			}

			return mSystem;
		} }

		#region Ctor
		EngineSystemReference(bool dummy)
		{
			Util.MarkUnusedVariable(ref dummy);

			mSystem = null;
			mBuildHandle = EngineBuildHandle.None;
		}
		internal EngineSystemReference(EngineSystemBase system, EngineBuildHandle buildHandle)
		{
			Contract.Assume(system != null);
			Contract.Assume(!buildHandle.IsNone);

			mSystem = system;
			mBuildHandle = buildHandle;

#pragma warning disable 4014
			system.AddReferenceAsync(buildHandle);
#pragma warning restore 4014
		}
		#endregion

		/// <summary>Has this reference not yet been disposed of?</summary>
		[Contracts.Pure]
		public bool IsValid { get {
			return mSystem != null;
		} }
		public bool IsNotValid { get { return !IsValid; } }

		#region IDisposable Members
		public void Dispose()
		{
			if (mSystem != null)
			{
#pragma warning disable 4014
				mSystem.RemoveReferenceAsync(mBuildHandle);
#pragma warning restore 4014
				mSystem = null;
			}
		}
		#endregion

		/// <summary>Access the reference's underlying <see cref="System"/>. Only use in temporary copies and calls!</summary>
		/// <returns></returns>
		public EngineSystemBase ToSystem() => this.System;
		/// <summary>Access the reference's underlying <see cref="System"/>. Only use in temporary copies and calls!</summary>
		/// <param name="reference"></param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static implicit operator EngineSystemBase(EngineSystemReference reference) =>
			reference.ToSystem();
	};

	/// <summary>Represents a lightweight reference to a specific <see cref="EngineSystemBase"/> implementation</summary>
	/// <remarks>Should be declared in a using() statement, or as an object member that implements IDisposable (and disposes this)</remarks>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct EngineSystemReference<T>
		: IDisposable
		where T : EngineSystemBase
	{
		public static readonly EngineSystemReference<T> None = new EngineSystemReference<T>(true);

		T mSystem;
		readonly EngineBuildHandle mBuildHandle;

		/// <summary>The system that is referenced</summary>
		public T System { get {
			if (IsValid)
			{
				bool didntTimeout = mSystem.WaitForExternsIO();
				Contract.Assert(didntTimeout);
			}

			return mSystem;
		} }

		#region Ctor
		EngineSystemReference(bool dummy)
		{
			Util.MarkUnusedVariable(ref dummy);

			mSystem = null;
			mBuildHandle = EngineBuildHandle.None;
		}
		internal EngineSystemReference(T system, EngineBuildHandle buildHandle)
		{
			Contract.Assume(system != null);
			Contract.Assume(!buildHandle.IsNone);

			mSystem = system;
			mBuildHandle = buildHandle;

#pragma warning disable 4014
			system.AddReferenceAsync(buildHandle);
#pragma warning restore 4014
		}
		#endregion

		/// <summary>Has this reference not yet been disposed of?</summary>
		[Contracts.Pure]
		public bool IsValid => mSystem != null;
		public bool IsNotValid => !IsValid;

		#region IDisposable Members
		public void Dispose()
		{
			if (mSystem != null)
			{
#pragma warning disable 4014
				mSystem.RemoveReferenceAsync(mBuildHandle);
#pragma warning restore 4014
				mSystem = null;
			}
		}
		#endregion

		/// <summary>Access the reference's underlying <see cref="System"/>. Only use in temporary copies and calls!</summary>
		/// <returns></returns>
		public T ToSystem() => this.System;
		/// <summary>Access the reference's underlying <see cref="System"/>. Only use in temporary copies and calls!</summary>
		/// <param name="reference"></param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static implicit operator T(EngineSystemReference<T> reference) =>
			reference.ToSystem();
	};
}
