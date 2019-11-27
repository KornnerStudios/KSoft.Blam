using System;
using System.Collections.Generic;

namespace KSoft.Collections
{
	partial class ActiveList<T>
	{
		public struct ActiveItemsEnumerator : IEnumerator<T>, ICloneable
		{
			ActiveList<T> mList;
			IEnumerator<int> mActiveIndicesEnumerator;

			internal ActiveItemsEnumerator(ActiveList<T> list)
			{
				mList = list;
				mActiveIndicesEnumerator = list.mSlotStates.SetBitIndices.GetEnumerator();
			}
			public object Clone() { return MemberwiseClone(); }

			public T Current								{ get { return mList[mActiveIndicesEnumerator.Current]; } }
			object System.Collections.IEnumerator.Current	{ get { return this.Current; } }
			void IDisposable.Dispose()						{ ((IDisposable)mActiveIndicesEnumerator).Dispose(); }
			public void Reset()								{ mActiveIndicesEnumerator.Reset(); }
			public bool MoveNext()							{ return mActiveIndicesEnumerator.MoveNext(); }
		};
	};
}