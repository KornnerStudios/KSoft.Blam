using System;
using System.Collections.Generic;

using NCollections = System.Collections;

namespace KSoft.Collections
{
	using BitStateFilterEnumeratorWrapper = EnumeratorWrapper<int, IReadOnlyBitSetEnumerators.StateFilterEnumerator>;

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("Count = {Count}, Length = {Length}"),
	 System.Diagnostics.DebuggerTypeProxy(typeof(ActiveList<>.DebugView))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public partial class ActiveList<T>
		: IList<T>
		where T : class
	{
		// #REVIEW_BLAM: we can either keep this here (meaning it's defined for every generic instance)
		// or we move it out and provide an internal accessor to mSlots
		sealed class DebugView
		{
			readonly ActiveList<T> mList;

			public DebugView(ActiveList<T> list)
			{
				ArgumentNullException.ThrowIfNull(list);

				mList = list;
			}

			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
			public IList<T> Items => mList.mSlots;
		};

		const bool kSlotStateInvalid = false;
		//const bool kSlotStateValid = true;

		readonly ActiveListDesc<T> mDesc;
		readonly List<T> mSlots;
		readonly BitSet mSlotStates;

		internal ActiveListDesc<T> Description => mDesc;

		#region Ctor
		public ActiveList(ActiveListDesc<T> description)
		{
			ArgumentNullException.ThrowIfNull(description);

			mDesc = description;
			mSlots = new List<T>(description.Capacity);
			mSlotStates = new BitSet(description.Capacity, fixedLength: description.IsFixedLength);

			for (int x = 0; x < description.Capacity; x++)
			{
				mSlots.Add(description.InvalidData);
			}

			IsReadOnly = true;
		}
		ActiveList<T> NewScratchList()
		{
			return new ActiveList<T>(mDesc);
		}
		#endregion

		#region IList<T> Members
		int IList<T>.IndexOf(T item)
		{
			return mDesc.IsInvalid(item) ? -1 : mSlots.IndexOf(item);
		}
		public void Insert(int index, T item)
		{
			mSlots[index] = item;
			mSlotStates[index] = mDesc.IsInvalid(item) == false;
		}
		// Can't support this, our preconditions differ from vanilla List
		void IList<T>.Insert(int index, T item) => throw new NotImplementedException();

		public void RemoveAt(int index)
		{
			mSlots[index] = mDesc.InvalidData;
			mSlotStates[index] = kSlotStateInvalid;
		}
		// Can't support this, our preconditions differ from vanilla List
		void IList<T>.RemoveAt(int index) => throw new NotImplementedException();

		public T this[int index]
		{
			get { return mSlots[index]; }
			set { Insert(index, value); }
		}
		#endregion

		#region ICollection<T> Members
		void ICollection<T>.Add(T item)							=> throw new NotImplementedException();
		bool ICollection<T>.Contains(T item)					=> mSlots.Contains(item);
		// #TODO_BLAM: only copy active elements
		void ICollection<T>.CopyTo(T[] array, int arrayIndex)	=> mSlots.CopyTo(array, arrayIndex);
		/// <summary>Number of active items in the list</summary>
		public int Count			=> mSlotStates.Cardinality;
		/// <summary>Number of inactive items in the list</summary>
		public int InactiveCount	=> mSlotStates.CardinalityZeros;
		/// <summary>Total number of items in the list, active or inactive</summary>
		public int Length			=> mSlots.Count;
		// #REVIEW_BLAM: decide if we want to use this
		public bool IsReadOnly		{ get; private set; }

		public void Clear()
		{
			for (int x = 0; x < mSlots.Count; x++) { mSlots[x] = mDesc.InvalidData; }
			mSlotStates.SetAll(kSlotStateInvalid);
		}

		public bool Remove(T item)
		{
			if (mDesc.IsInvalid(item))
			{
				return false;
			}

			int index = mSlots.IndexOf(item);
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}

			return false;
		}
		#endregion

		#region IEnumerable<T> Members
		/// <summary>Get an active-only slot items enumerator</summary>
		/// <returns></returns>
		public ActiveItemsEnumerator GetEnumerator()						{ return new ActiveItemsEnumerator(this); }
		IEnumerator<T> IEnumerable<T>.GetEnumerator()						{ return GetEnumerator(); }
		NCollections.IEnumerator NCollections.IEnumerable.GetEnumerator()	{ return GetEnumerator(); }
		#endregion

		/// <summary>Get the index of the first inactive item slot</summary>
		public int FirstInactiveIndex => mSlotStates.NextClearBitIndex(0);
		/// <summary>Get an inactive-only slot items enumerator</summary>
		public BitStateFilterEnumeratorWrapper InactiveIndices => mSlotStates.ClearBitIndices;

		void ValidateSlotIndex(int index)
		{
			if (index < 0 || index >= Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
		}
		void ValidateSlotIndexOrNone(int index)
		{
			if (!index.IsNoneOrPositive() || index >= Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
		}
		public bool SlotIsFree(int index)
		{
			ValidateSlotIndex(index);

			return mSlotStates[index] == kSlotStateInvalid;
		}
		public bool SlotIsFreeOrIndexIsNone(int index)
		{
			ValidateSlotIndexOrNone(index);

			return !index.IsNotNone() || mSlotStates[index] == kSlotStateInvalid;
		}
		public bool SlotIsFreeOrInvalidIndex(int index)
		{
			return index < 0 || index >= Length || mSlotStates[index] == kSlotStateInvalid;
		}

		/// <summary>Add an item at the next free slot</summary>
		/// <param name="item"></param>
		/// <param name="index">Slot index the item was added at</param>
		public void Add(T item, out int index)
		{
			index = FirstInactiveIndex;
			if (index.IsNotNone())
			{
				Insert(index, item);
			}
			else
			{
				// #REVIEW_BLAM: use a different exception type
				throw new OutOfMemoryException("Ran out of free slots!");
			}
		}

		/// <summary>Insert an item at an assumed free slot</summary>
		/// <param name="item"></param>
		/// <param name="index">Slot index we want the item inserted at</param>
		public void AddExplicit(T item, int index)
		{
			ValidateSlotIndex(index);
			if (!SlotIsFree(index))
			{
				throw new InvalidOperationException("Tried inserting an item where we thought there was none. Duplicate entry?");
			}

			Insert(index, item);
		}

		public void GarbageCollect(out Dictionary<int,int> idRemappings)
		{
			// If the next inactive index is the same as the active item count
			// then we don't need to perform
			if (FirstInactiveIndex == Count)
			{
				idRemappings = new Dictionary<int, int>(0);
				return;
			}
			else
			{
				idRemappings = new Dictionary<int, int>(Count);
			}

			// #TODO_BLAM: do me?
		}
	};
}
