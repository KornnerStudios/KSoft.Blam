using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using NCollections = System.Collections;

namespace KSoft.Collections
{
	using KSoft.Blam;

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class ActiveListDesc<T>
		where T : class
	{
		#region Null
		static readonly Func<T, bool> kNullEquator = obj => obj == null;
		public static readonly Func<T, int> kObjectToNoneIndex = obj => -1;

		ActiveListDesc(int capacity, bool fixedLength) : this(capacity, null, kNullEquator, fixedLength)
		{
		}

		public static ActiveListDesc<T> CreateForNullData(int capacity, bool fixedLength = true)
		{
			Contract.Requires(capacity >= 0);

			return new ActiveListDesc<T>(capacity, fixedLength);
		}
		#endregion

		readonly Func<T, bool> mInvalidEquator;
		readonly T kInvalidData;
		readonly int kCapacity;
		readonly bool kIsFixedLength;
		Func<T, int> mObjectToIndex;

		public ActiveListDesc(int capacity, T invalidData, Func<T, bool> invalidEquator, bool fixedLength = true)
		{
			Contract.Requires(capacity >= 0);

			mInvalidEquator = invalidEquator;
			kInvalidData = invalidData;
			kCapacity = capacity;
			kIsFixedLength = fixedLength;
			ObjectToIndex = kObjectToNoneIndex;
		}

		public bool IsInvalid(T other)		{ return mInvalidEquator(other); }
		public T InvalidData				{ get { return kInvalidData; } }
		public int Capacity					{ get { return kCapacity; } }
		public bool IsFixedLength			{ get { return kIsFixedLength; } }
		public Func<T, int> ObjectToIndex	{
			get { return mObjectToIndex; }
			set {
				Contract.Requires(value != null);

				mObjectToIndex = value;
			}
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("Count = {Count}, Length = {Length}"),
	 System.Diagnostics.DebuggerTypeProxy(typeof(ActiveList<>.DebugView))]
	public partial class ActiveList<T> : IList<T>
		where T : class
	{
		// TODO: we can either keep this here (meaning it's defined for every generic instance)
		// or we move it out and provide an internal accessor to mSlots
		sealed class DebugView
		{
			ActiveList<T> mList;

			public DebugView(ActiveList<T> list)
			{
				Contract.Requires(list != null);

				mList = list;
			}

			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
			public IList<T> Items { get { return mList.mSlots; } }
		};

		const bool kSlotStateInvalid = false;
		const bool kSlotStateValid = true;

		readonly ActiveListDesc<T> mDesc;
		List<T> mSlots;
		BitSet mSlotStates;

		internal ActiveListDesc<T> Description { get { return mDesc; } }

		#region Ctor
		public ActiveList(ActiveListDesc<T> description)
		{
			Contract.Requires(description != null);

			mDesc = description;
			mSlots = new List<T>(description.Capacity);
			mSlotStates = new BitSet(description.Capacity, fixedLength: description.IsFixedLength);

			for (int x = 0; x < description.Capacity; x++)
				mSlots.Add(description.InvalidData);

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
		void IList<T>.Insert(int index, T item) { throw new NotImplementedException(); }

		public void RemoveAt(int index)
		{
			mSlots[index] = mDesc.InvalidData;
			mSlotStates[index] = kSlotStateInvalid;
		}
		// Can't support this, our preconditions differ from vanilla List
		void IList<T>.RemoveAt(int index) { throw new NotImplementedException(); }

		public T this[int index]
		{
			get { return mSlots[index]; }
			set { Insert(index, value); }
		}
		#endregion

		#region ICollection<T> Members
		void ICollection<T>.Add(T item)							{ throw new NotImplementedException(); }
		bool ICollection<T>.Contains(T item)					{ return mSlots.Contains(item); }
		// TODO: only copy active elements
		void ICollection<T>.CopyTo(T[] array, int arrayIndex)	{ mSlots.CopyTo(array, arrayIndex); }
		/// <summary>Number of active items in the list</summary>
		public int Count			{ get { return mSlotStates.Cardinality; } }
		/// <summary>Number of inactive items in the list</summary>
		public int InactiveCount	{ get { return mSlotStates.CardinalityZeros; } }
		/// <summary>Total number of items in the list, active or inactive</summary>
		public int Length			{ get { return mSlots.Count; } }
		// TODO: decide if we want to use this
		public bool IsReadOnly		{ get; private set; }

		public void Clear()
		{
			for (int x = 0; x < mSlots.Count; x++) mSlots[x] = mDesc.InvalidData;
			mSlotStates.SetAll(kSlotStateInvalid);
		}

		public bool Remove(T item)
		{
			if (mDesc.IsInvalid(item)) return false;

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
		public int FirstInactiveIndex					{ get { return mSlotStates.NextClearBitIndex(0); } }
		/// <summary>Get an inactive-only slot items enumerator</summary>
		public EnumeratorWrapper<int, IReadOnlyBitSetEnumerators.StateFilterEnumerator> InactiveIndices	{ get { return mSlotStates.ClearBitIndices; } }

		[Contracts.Pure]
		public bool SlotIsFree(int index)
		{
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Length);

			return mSlotStates[index] == kSlotStateInvalid;
		}
		[Contracts.Pure]
		public bool SlotIsFreeOrIndexIsNone(int index)
		{
			Contract.Requires<ArgumentOutOfRangeException>(index.IsNoneOrPositive() && index < Length);

			return index.IsNotNone() ? mSlotStates[index] == kSlotStateInvalid : true;
		}
		[Contracts.Pure]
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
				Insert(index, item);
			else
				throw new OutOfMemoryException("Ran out of free slots!");
		}

		/// <summary>Insert an item at an assumed free slot</summary>
		/// <param name="item"></param>
		/// <param name="index">Slot index we want the item inserted at</param>
		public void AddExplicit(T item, int index)
		{
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Length);
			Contract.Requires<InvalidOperationException>(SlotIsFree(index),
				"Tried inserting an item where we thought there was none. Duplicate entry?");

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
				idRemappings = new Dictionary<int, int>(Count);

			// TODO: do me?
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public static class ActiveListUtil
	{
		#region IBitStreamSerializable Members
		public static void Serialize<T, TContext>(IO.BitStream s, ActiveList<T> list, int countBitLength,
			TContext ctxt, Func<IO.BitStream, TContext, T> ctor,
			List<int> writeOrder = null)
			where T : class, IO.IBitStreamSerializable
		{
			Contract.Requires(list != null);
			Contract.Requires(countBitLength <= Bits.kInt32BitCount);
			Contract.Requires(ctor != null);

			int count = writeOrder == null ? list.Count : writeOrder.Count;
			s.Stream(ref count, countBitLength);

			if (s.IsReading)
			{
				for (int x = 0; x < count; x++)
				{
					var t = ctor(s, ctxt);
					t.Serialize(s);
					list.Insert(x, t);
				}
			}
			else if (s.IsWriting)
			{
				if(writeOrder == null)
					foreach (var obj in list)
						obj.Serialize(s);
				else
				{
					// TODO: well, shall we warn?
					//Contract.Assert(writeOrder.Count == list.Count); // would rather just warn...
					foreach (int index in writeOrder)
					{
						Contract.Assert(list.SlotIsFree(index) == false);
						list[index].Serialize(s);
					}
				}
			}
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public enum TagElementStreamReadMode
		{
			/// <summary>Serialize the object after CREATING it</summary>
			PostConstructor,
			/// <summary>Serialize the object after ADDING it</summary>
			PostAdd,
		};

		static class TagElementTextStreamUtils<TDoc, TCursor, T, TContext>
			where TDoc : class
			where TCursor : class
			where T : class, IO.ITagElementStringNameStreamable
		{
			static void ReadElements(IO.TagElementStream<TDoc, TCursor, string> s, IEnumerable<TCursor> elements,
				ActiveList<T> list,
				TContext ctxt, Func<IO.TagElementStream<TDoc, TCursor, string>, TContext, T> ctor,
				Func<TContext, TagElementStreamReadMode> getReadMode)
			{
				var read_mode = getReadMode == null ? TagElementStreamReadMode.PostConstructor : getReadMode(ctxt);

				foreach (var node in elements)
					using (s.EnterCursorBookmark(node))
					{
						var value = ctor(s, ctxt);
						if (read_mode == TagElementStreamReadMode.PostConstructor)
							value.Serialize(s);

						int index = list.Description.ObjectToIndex(value);
						list.AddExplicit(value, index);

						if (read_mode == TagElementStreamReadMode.PostAdd)
							value.Serialize(s);
					}
			}
			public static void ReadElements(IO.TagElementStream<TDoc, TCursor, string> s, string elementName,
				ActiveList<T> list,
				TContext ctxt, Func<IO.TagElementStream<TDoc, TCursor, string>, TContext, T> ctor,
				Func<TContext, TagElementStreamReadMode> getReadMode)
			{
				ReadElements(s, s.ElementsByName(elementName), list, ctxt, ctor, getReadMode);
			}
			public static void WriteElements(IO.TagElementStream<TDoc, TCursor, string> s, string elementName,
				ActiveList<T> list, Predicate<T> writeShouldSkip)
			{
				foreach (var value in list) if (!writeShouldSkip(value))
					using (s.EnterCursorBookmark(elementName))
						value.Serialize(s);
			}
		};
		/// <remarks>
		/// List's description must provide a valid implement for its ObjectToIndex
		///
		/// Reading runs the <paramref name="ctor"/> then calls the object's serialize method. It then uses
		/// ObjectToIndex to figure out where to add the object within the list
		///
		/// Writing will skip items that return true with <paramref name="writeShouldSkip"/>. If the predicate
		/// is null, it defaults to one which always returns false, so -all- items will be written
		///
		/// The caller can define when the serialize method of the object is called during reads via <paramref name="getReadMode"/>.
		/// This is useful when the <paramref name="ctor"/> actually populates the Id of the object, instead of reading it
		/// </remarks>
		public static void Serialize<TDoc, TCursor, T, TContext>(IO.TagElementStream<TDoc, TCursor, string> s, string elementName,
			ActiveList<T> list,
			TContext ctxt, Func<IO.TagElementStream<TDoc, TCursor, string>, TContext, T> ctor,
			Predicate<T> writeShouldSkip = null,
			Func<TContext, TagElementStreamReadMode> getReadMode = null)
			where TDoc : class
			where TCursor : class
			where T : class, IO.ITagElementStringNameStreamable
		{
			Contract.Requires(list != null);
			Contract.Requires(ctor != null);

			if (writeShouldSkip == null) writeShouldSkip = obj => false;

				 if (s.IsReading) TagElementTextStreamUtils<TDoc,TCursor,T,TContext>.ReadElements(s, elementName, list, ctxt, ctor, getReadMode);
			else if (s.IsWriting) TagElementTextStreamUtils<TDoc,TCursor,T,TContext>.WriteElements(s, elementName, list, writeShouldSkip);
		}
		#endregion
	};
}