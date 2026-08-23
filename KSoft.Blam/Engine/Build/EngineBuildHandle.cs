using System;
using System.Collections.Generic;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Blam.Engine
{
	using BitFieldTraits = Bitwise.BitFieldTraits;

	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	[System.Diagnostics.DebuggerDisplay("Engine# = {EngineIndex}, Branch# = {BranchIndex}, Rev# = {RevisionIndex}")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public readonly struct EngineBuildHandle
		: IComparer<EngineBuildHandle>, System.Collections.IComparer // #REMOVE_BLAM
		, IComparable<EngineBuildHandle>, IComparable
		, IEquatable<EngineBuildHandle>
	{
		#region Constants
		// nesting these into a static class makes them run before the struct's static ctor...
		// which, being a value type cctor, may not run when we want it
		static class Constants
		{
			public static readonly BitFieldTraits kRevisionBitField =
				new(EngineBuildRevision.kIndexBitCount);
			public static readonly BitFieldTraits kBranchBitField =
				new(EngineBuildBranch.kIndexBitCount, kRevisionBitField);
			public static readonly BitFieldTraits kEngineBitField =
				new(BlamEngine.kIndexBitCount, kBranchBitField);

			public static readonly BitFieldTraits kLastBitField =
				kEngineBitField;
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		/// <remarks>11 bits at last count</remarks>
		public static int BitCount => Constants.kLastBitField.FieldsBitCount;
		public static uint Bitmask => Constants.kLastBitField.FieldsBitmask.u32;

		public static readonly EngineBuildHandle None = new();
		#endregion

		#region Internal Value
		[Interop.FieldOffset(0)] readonly uint mHandle;

		internal readonly uint Handle => mHandle;

		static void InitializeHandle(out uint handle,
			int engineIndex, int branchIndex, int revisionIndex)
		{
			var encoder = new Bitwise.HandleBitEncoder();
			EngineBuildRevision.BitEncodeIndex(ref encoder, revisionIndex);
			EngineBuildBranch.BitEncodeIndex(ref encoder, branchIndex);
			BlamEngine.BitEncodeIndex(ref encoder, engineIndex);

			if (encoder.UsedBitCount != EngineBuildHandle.BitCount)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Encoded build handle used {0} bits; expected {1}.",
					encoder.UsedBitCount, EngineBuildHandle.BitCount));
			}

			handle = encoder.GetHandle32();
		}
		#endregion

		#region Ctor
		public EngineBuildHandle(int engineIndex, int branchIndex, int revisionIndex)
		{
			InitializeHandle(out mHandle, engineIndex, branchIndex, revisionIndex);
		}
		internal EngineBuildHandle(uint handle, BitFieldTraits buildField)
		{
			handle >>= buildField.BitIndex;
			handle &= Bitmask;

			mHandle = handle;
		}

		public static EngineBuildHandle Create(int engineIndex,
			int branchIndex = TypeExtensions.kNone, int revisionIndex = TypeExtensions.kNone)
		{
			if (!BlamEngine.IsValidIndex(engineIndex))
			{
				throw new ArgumentOutOfRangeException(nameof(engineIndex));
			}
			ArgumentOutOfRangeException.ThrowIfLessThan(branchIndex, TypeExtensions.kNone);
			ArgumentOutOfRangeException.ThrowIfLessThan(revisionIndex, TypeExtensions.kNone);

			return new EngineBuildHandle(engineIndex, branchIndex, revisionIndex);
		}
		#endregion

		#region Value properties
		public readonly int EngineIndex => BlamEngine.BitDecodeIndex(mHandle, Constants.kEngineBitField.BitIndex);
		public readonly int BranchIndex => EngineBuildBranch.BitDecodeIndex(mHandle, Constants.kBranchBitField.BitIndex);
		public readonly int RevisionIndex => EngineBuildRevision.BitDecodeIndex(mHandle, Constants.kRevisionBitField.BitIndex);

		public readonly BlamEngine? Engine { get {
			int index = EngineIndex;

			return index.IsNotNone()
				? EngineRegistry.Engines[index]
				: null;
		} }

		public readonly EngineBuildBranch? Branch { get {
			int index = BranchIndex;

			if (index.IsNotNone())
			{
				if (EngineIndex.IsNone())
				{
					throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
						"Build handle has branch index {0}, but its engine index is NONE.", index));
				}

				return Engine!.BuildRepository.Branches[index];
			}

			return null;
		} }

		public readonly EngineBuildRevision? Revision { get {
			int index = RevisionIndex;

			if (index.IsNotNone())
			{
				if (EngineIndex.IsNone())
				{
					throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
						"Build handle has revision index {0}, but its engine index is NONE.", index));
				}
				if (BranchIndex.IsNone())
				{
					throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
						"Build handle has revision index {0}, but its branch index is NONE.", index));
				}

				return Branch!.Revisions[index];
			}

			return null;
		} }
		#endregion

		public readonly bool IsNone =>
			// this only works because ALL bitfields are NONE encoded, meaning -1 values are encoded as 0
			mHandle == 0;
		public readonly bool IsNotNone => !IsNone;
		/// <summary>This handle refers to a fully formed build, down to the revision)</summary>
		public readonly bool IsFullyFormed => IsNotNone
				&& EngineIndex.IsNotNone()
				&& BranchIndex.IsNotNone()
				&& RevisionIndex.IsNotNone()
				;

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override readonly bool Equals(object? obj)
		{
			if (obj is EngineBuildHandle handle)
			{
				return this.mHandle == handle.mHandle;
			}

			return false;
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on its exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode() => (int)mHandle;
		/// <summary>Returns a string representation of this object</summary>
		/// <returns>"[Engine\tBranch\tRevision]"</returns>
		public override string ToString()
		{
			// #REVIEW_BLAM: This isn't great when viewing in a debugger, as the tabs seem to be ignored (so there's no whitespace)
			return string.Format(Util.InvariantCultureInfo,
				"[{0}\t{1}\t{2}]",
				Engine, Branch, Revision);
		}
		#endregion

		/// <summary>Creates a string of the build component name ids separated by periods</summary>
		/// <returns></returns>
		/// <remarks>If the <see cref="Branch"/>'s display name is the same as <see cref="Engine"/>, the former isn't included in the output</remarks>
		public readonly string ToDisplayString()
		{
			if (IsNone)
			{
				return TypeExtensions.kNoneDisplayString;
			}

			var sb = new System.Text.StringBuilder();
			int engine_index = EngineIndex;
			int branch_index = BranchIndex;
			int revisn_index = RevisionIndex;

			if (engine_index.IsNotNone())
			{
				BlamEngine engine = EngineRegistry.Engines[engine_index];
				sb.Append(engine);

				#region Branch
				if (branch_index.IsNotNone())
				{
					EngineBuildBranch branch = engine.BuildRepository.Branches[branch_index];
					// only include the branch display name if it isn't the same as the engine's
					if (branch.ToString() != engine.ToString())
					{
						sb.AppendFormat(Util.InvariantCultureInfo,
							".{0}", branch);
					}

					#region Revision
					if (revisn_index.IsNotNone())
					{
						EngineBuildRevision revision = branch.Revisions[revisn_index];
						sb.AppendFormat(Util.InvariantCultureInfo,
							".{0}",
							revision.Version.ToString(Util.InvariantCultureInfo));
					}
					#endregion
				}
				#endregion
			}

			return sb.ToString();
		}

		#region Relationship testing
		/// <summary>Tests whether this handle and the other are of the same <see cref="Engine"/> and <see cref="Branch"/></summary>
		/// <param name="otherHandle">The other handle to compare with. Revision data is ignored</param>
		/// <returns></returns>
		/// <remarks>If either handle in this equation <see cref="IsNone"/>, this will return false</remarks>
		public readonly bool IsWithinSameBranch(EngineBuildHandle otherHandle)
		{
			if (this.IsNone || otherHandle.IsNone)
			{
				return false;
			}

			if (EngineIndex != otherHandle.EngineIndex)
			{
				return false;
			}

			return BranchIndex == otherHandle.BranchIndex;
		}
		/// <summary>Tests whether this handle and the branch's are of the same <see cref="Engine"/> and <see cref="Branch"/></summary>
		/// <param name="branch">The branch to compare with</param>
		/// <returns></returns>
		/// <remarks>If either handle in this equation <see cref="IsNone"/>, this will return false</remarks>
		public readonly bool IsWithinSameBranch(EngineBuildBranch branch)
		{
			ArgumentNullException.ThrowIfNull(branch);

			return this.IsWithinSameBranch(branch.BranchHandle);
		}

		/// <summary>Tests whether this handle is a descendant of the given parent handle</summary>
		/// <param name="parentHandle">The handle for the parent build. Revision data is ignored</param>
		/// <returns></returns>
		/// <remarks>
		/// A build can be considered a child if:
		/// 1) It's of the same engine
		/// 2) It's of the same branch, if the parent's branch data isn't NONE
		/// </remarks>
		public readonly bool IsChildOf(EngineBuildHandle parentHandle) // IsSupersetOf (this includes all of other)
		{
			if (EngineIndex != parentHandle.EngineIndex)
			{
				return false;
			}

			int parent_branch_index = parentHandle.BranchIndex;
			if (parent_branch_index.IsNotNone() && BranchIndex != parent_branch_index)
			{
				return false;
			}

			return true;
		}
		#endregion

		#region IComparer<EngineBuildHandle> Members
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public readonly int Compare(EngineBuildHandle x, EngineBuildHandle y)
		{
			return EngineBuildHandle.StaticCompare(x, y);
		}
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		readonly int System.Collections.IComparer.Compare(object? x, object? y)
		{
			KSoft.Debug.TypeCheck.CastValue(x!, out EngineBuildHandle _x);
			KSoft.Debug.TypeCheck.CastValue(y!, out EngineBuildHandle _y);

			return EngineBuildHandle.StaticCompare(_x, _y);
		}
		#endregion

		#region IComparable<EngineBuildHandle> Members
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public readonly int CompareTo(EngineBuildHandle other) => EngineBuildHandle.StaticCompare(this, other);
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		readonly int IComparable.CompareTo(object? obj)
		{
			KSoft.Debug.TypeCheck.CastValue(obj!, out EngineBuildHandle _obj);

			return EngineBuildHandle.StaticCompare(this, _obj);
		}
		#endregion

		#region IEquatable<EngineBuildHandle> Members
		/// <summary>See <see cref="IEquatable{T}.Equals"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public readonly bool Equals(EngineBuildHandle other)
		{
			return this.mHandle == other.mHandle;
		}
		#endregion

		#region Operators
		public static bool operator==(EngineBuildHandle lhs, EngineBuildHandle rhs) => lhs.Handle == rhs.Handle;
		public static bool operator!=(EngineBuildHandle lhs, EngineBuildHandle rhs) => lhs.Handle != rhs.Handle;
		#endregion


		#region Util
		static int StaticCompare(EngineBuildHandle lhs, EngineBuildHandle rhs)
		{
			// #TODO figure out a a utility to do this generically for bit-encoded handles that can run
			// in the internal Constants class.
			if (EngineBuildHandle.BitCount >= Bits.kInt32BitCount)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Handle bit count must be less than {0}; actual bit count is {1}.",
					Bits.kInt32BitCount, EngineBuildHandle.BitCount));
			}

			int lhs_data = (int)lhs.mHandle;
			int rhs_data = (int)rhs.mHandle;
			int result = lhs_data - rhs_data;

			return result;
		}

		/// <summary>Get a 'trimmed' handle consisting only of this handle's engine index</summary>
		/// <returns>
		/// A new handle with only <see cref="EngineIndex"/> copied.
		/// <see cref="BranchIndex"/> and <see cref="RevisionIndex"/> will always be NONE
		/// </returns>
		public EngineBuildHandle ToEngineOnlyHandle()
		{
			if (IsNone) // avoid any bit operations if we're already 'none'
			{
				return this;
			}

			return new EngineBuildHandle(EngineIndex, TypeExtensions.kNone, TypeExtensions.kNone);
		}
		/// <summary>Get a 'trimmed' handle consisting only of this handle's engine and branch index</summary>
		/// <returns>
		/// A new handle with only <see cref="EngineIndex"/> and <see cref="BranchIndex"/> copied.
		/// <see cref="RevisionIndex"/> will always be NONE
		/// </returns>
		public EngineBuildHandle ToEngineBranchHandle()
		{
			if (IsNone) // avoid any bit operations if we're already 'none'
			{
				return this;
			}

			return new EngineBuildHandle(EngineIndex, BranchIndex, TypeExtensions.kNone);
		}

		/// <summary>
		/// Creates an engine target handle using this build handle, but with the platform and resource model set to NONE
		/// </summary>
		/// <returns></returns>
		public BlamEngineTargetHandle ToEngineTargetHandle()
		{
			return BlamEngineTargetHandle.FromBuildHandleOnly(this);
		}

		/// <summary>Decompose this handle into new handles consisting of just the macro indexes (ie, sans revision)</summary>
		/// <param name="engine">A new handle consisting of <see cref="EngineIndex"/></param>
		/// <param name="branch">A new handle consisting of <see cref="EngineIndex"/> and <see cref="BranchIndex"/></param>
		/// <returns>True if <see cref="IsNone"/> is false (ie, this has a meaningful value), else false</returns>
		public bool ExtractHandles(out EngineBuildHandle engine, out EngineBuildHandle branch)
		{
			if (IsNone)
			{
				engine = branch = this;
				return false;
			}

			int engine_index = EngineIndex;
			int branch_index = BranchIndex;

			// we're either a fully qualified handle (ie, include up to revision) or not
			branch = new EngineBuildHandle(engine_index, branch_index, TypeExtensions.kNone);
			engine = new EngineBuildHandle(engine_index, TypeExtensions.kNone, TypeExtensions.kNone);

			return true;
		}

		/// <summary>Looks up a value associated with a given build</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dic"></param>
		/// <param name="forBuild"></param>
		/// <param name="value"></param>
		/// <param name="actualBuild">The build handle which is actually associated with the value, or NONE</param>
		/// <returns></returns>
		/// <remarks>
		/// Tries to use <paramref name="forBuild"/>'s absolute value first in the lookup. If that fails,
		/// it then looks up by <see cref="Branch"/>, and then by <see cref="Engine"/>.
		/// <paramref name="actualBuild"/> will then have the handle for whichever looks up successfully.
		///
		/// Will safely handle <see cref="IsNone"/> handles.
		/// </remarks>
		public static bool TryGetValue<T>(IReadOnlyDictionary<EngineBuildHandle, T> dic, EngineBuildHandle forBuild,
			ref T value, out EngineBuildHandle actualBuild)
		{
			actualBuild = forBuild;

			if (forBuild.IsNone)
			{
				return false;
			}

			if (dic.TryGetValue(forBuild, out T? absolute_value))
			{
				value = absolute_value!;
				return true;
			}

			forBuild.ExtractHandles(out EngineBuildHandle engine, out EngineBuildHandle branch);

			if (dic.TryGetValue(branch, out T? branch_value))
			{
				value = branch_value!;
				actualBuild = branch;
				return true;
			}

			if (dic.TryGetValue(engine, out T? engine_value))
			{
				value = engine_value!;
				actualBuild = engine;
				return true;
			}

			actualBuild = EngineBuildHandle.None;
			value = default!;
			return false;
		}
		public static bool TryGetValue<T>(IReadOnlyDictionary<EngineBuildHandle, T> dic, EngineBuildHandle forBuild,
			ref T value)
		{
			return TryGetValue(dic, forBuild, ref value, out EngineBuildHandle /*actual_build*/_);
		}

		public bool TryGetValue<T>(IReadOnlyDictionary<EngineBuildHandle, T> dic,
			ref T value, out EngineBuildHandle actualBuild)
		{
			return TryGetValue(dic, this, ref value, out actualBuild);
		}
		public bool TryGetValue<T>(IReadOnlyDictionary<EngineBuildHandle, T> dic,
			ref T value)
		{
			return TryGetValue(dic, this, ref value);
		}
		#endregion

		#region ITagElementStreamable<string> Members
		const string kAttributeNameEngine = "engine";
		const string kAttributeNameBranch = "branch";
		const string kAttributeNameRevisn = "rev";

		public static void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			ref EngineBuildHandle value)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			int engine_index = reading
				? TypeExtensions.kNone
				: value.EngineIndex;
			int branch_index = reading
				? TypeExtensions.kNone
				: value.BranchIndex;
			int revisn_index = reading
				? TypeExtensions.kNone
				: value.RevisionIndex;

			if (BlamEngine.SerializeId(s, kAttributeNameEngine, ref engine_index, true))
			{
				var repo = EngineRegistry.Engines[engine_index].BuildRepository;

				if (repo.SerializeBranchId(s, kAttributeNameBranch, ref branch_index, true))
				{
					var branch = repo.Branches[branch_index];

					branch.SerializeRevisionId(s, kAttributeNameRevisn, ref revisn_index, true);
				}
			}

			if (reading)
			{
				value = new EngineBuildHandle(engine_index, branch_index, revisn_index);
			}
		}
		internal static void SerializeWithContext<TDoc, TCursor, TContext>(IO.TagElementStream<TDoc, TCursor, string> s,
			TContext _unused,
			ref EngineBuildHandle value)
			where TDoc : class
			where TCursor : class
		{
			Util.MarkUnusedVariable(ref _unused);

			Serialize(s, ref value);
		}

		/// <summary>Serialize a build handle, using a 'baseline' to populate or cull (from writing) root build information</summary>
		/// <param name="s"></param>
		/// <param name="baseline">Root build information (ie, engine, or engine and branch)</param>
		/// <param name="value"></param>
		public static void SerializeWithBaseline<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			EngineBuildHandle baseline,
			ref EngineBuildHandle value)
			where TDoc : class
			where TCursor : class
		{
			int engine_index = baseline.EngineIndex;
			int branch_index = baseline.BranchIndex;
			int revisn_index = baseline.RevisionIndex;
			EngineBuildRepository repo = null!;
			EngineBuildBranch branch = null!;

			if (s.IsWriting)
			{
				#region prepare engine_index
				if (value.EngineIndex == engine_index)
				{
					repo = EngineRegistry.Engines[engine_index].BuildRepository;
					// cause engine_index not to be written
					engine_index = TypeExtensions.kNone;
				}
				else
				{
					engine_index = value.EngineIndex;
				}
				#endregion

				#region prepare branch_index
				if (value.BranchIndex == branch_index)
				{
					branch = repo!.Branches[branch_index];
					// cause branch_index not to be written
					branch_index = TypeExtensions.kNone;
				}
				else
				{
					branch_index = value.BranchIndex;
				}
				#endregion

				#region prepare revisn_index
				if (value.BranchIndex == revisn_index)
				{
					// cause branch_index not to be written
					branch_index = TypeExtensions.kNone;
				}
				else
				{
					revisn_index = value.RevisionIndex;
				}
				#endregion
			}

			// This is a logic driven mess, but having branches for both IsReading and IsWriting would result in more copy&paste code

			// reading: baseline EngineIndex is valid, or index is serialized
			// writing: value EngineIndex mismatches baseline
			if (engine_index.IsNotNone() || BlamEngine.SerializeId(s, kAttributeNameEngine, ref engine_index, true))
			{
				repo = EngineRegistry.Engines[engine_index].BuildRepository;
			}

			// precondition: someone's EngineIndex was valid
			// reading: baseline BranchIndex is valid, or index is serialized
			// writing: value BranchIndex mismatches baseline
			if (repo != null && (branch_index.IsNotNone() || repo.SerializeBranchId(s, kAttributeNameBranch, ref branch_index, true)))
			{
				branch = repo.Branches[branch_index];
			}

			// precondition: someone's BranchIndex was valid
			// reading: baseline RevisionIndex is valid
			// writing: value RevisionIndex mismatches baseline
			if (branch != null && (revisn_index.IsNotNone() || s.IsReading))
			{
				branch.SerializeRevisionId(s, kAttributeNameRevisn, ref revisn_index, true);
			}

			if (s.IsReading)
			{
				if (engine_index.IsNotNone())
				{
					value = new EngineBuildHandle(engine_index, branch_index, revisn_index);
				}
				else // engine_index is NONE, don't even bother trying to encode a new handle that should be all NONE anyway
				{
					value = EngineBuildHandle.None;
				}
			}
		}
		#endregion
	};
}
