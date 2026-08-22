using System;
using System.Diagnostics.CodeAnalysis;

namespace KSoft.Blam.Engine
{
	/// <summary>Represents a lightweight reference to a <see cref="EngineSystemBase"/></summary>
	/// <remarks>Should be declared in a using() statement, or as an object member that implements IDisposable (and disposes this)</remarks>
	public struct EngineSystemReference
		: IDisposable
	{
		public static readonly EngineSystemReference None = new(true);

		EngineSystemBase mSystem;
		readonly EngineBuildHandle mBuildHandle;

		/// <summary>The system that is referenced</summary>
		public readonly EngineSystemBase System { get {
			if (IsValid)
			{
				bool didntTimeout = mSystem.WaitForExternsIO();
				if (!didntTimeout)
				{
					throw new TimeoutException(string.Format(Util.InvariantCultureInfo,
						"Timed out waiting for extern I/O for {0}.", mSystem.Engine));
				}
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
			ArgumentNullException.ThrowIfNull(system);
			if (buildHandle.IsNone)
			{
				throw new ArgumentNoneException(nameof(buildHandle));
			}

			mSystem = system;
			mBuildHandle = buildHandle;

#pragma warning disable 4014
			system.AddReferenceAsync(buildHandle);
#pragma warning restore 4014
		}
		#endregion
		/// <summary>Has this reference not yet been disposed of?</summary>
		/// <summary>Has this reference not yet been disposed of?</summary>
		public readonly bool IsValid => mSystem != null;
		public readonly bool IsNotValid => !IsValid;

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
		public readonly EngineSystemBase ToSystem() => this.System;
		/// <summary>Access the reference's underlying <see cref="System"/>. Only use in temporary copies and calls!</summary>
		/// <param name="reference"></param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static implicit operator EngineSystemBase(EngineSystemReference reference) =>
			reference.ToSystem();
	};

	/// <summary>Represents a lightweight reference to a specific <see cref="EngineSystemBase"/> implementation</summary>
	/// <remarks>Should be declared in a using() statement, or as an object member that implements IDisposable (and disposes this)</remarks>
	public struct EngineSystemReference<T>
		: IDisposable
		where T : EngineSystemBase
	{
		public static readonly EngineSystemReference<T> None = new(true);

		T mSystem;
		readonly EngineBuildHandle mBuildHandle;

		/// <summary>The system that is referenced</summary>
		public readonly T System { get {
			if (IsValid)
			{
				bool didntTimeout = mSystem.WaitForExternsIO();
				if (!didntTimeout)
				{
					throw new TimeoutException(string.Format(Util.InvariantCultureInfo,
						"Timed out waiting for extern I/O for {0}.", mSystem.Engine));
				}
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
			ArgumentNullException.ThrowIfNull(system);
			if (buildHandle.IsNone)
			{
				throw new ArgumentNoneException(nameof(buildHandle));
			}

			mSystem = system;
			mBuildHandle = buildHandle;

#pragma warning disable 4014
			system.AddReferenceAsync(buildHandle);
#pragma warning restore 4014
		}
		#endregion
		/// <summary>Has this reference not yet been disposed of?</summary>
		/// <summary>Has this reference not yet been disposed of?</summary>
		public readonly bool IsValid => mSystem != null;
		public readonly bool IsNotValid => !IsValid;

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
		public readonly T ToSystem() => this.System;
		/// <summary>Access the reference's underlying <see cref="System"/>. Only use in temporary copies and calls!</summary>
		/// <param name="reference"></param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static implicit operator T(EngineSystemReference<T> reference) =>
			reference.ToSystem();
	};
}
