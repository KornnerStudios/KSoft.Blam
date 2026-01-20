using System;
using System.Collections.Generic;

namespace KSoft.Collections
{
	partial class ActiveList<T>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0250:Make struct 'readonly'")]
		public struct ActiveItemsEnumerator
			: IEnumerator<T>
			, ICloneable
		{
			readonly ActiveList<T> mList;
			readonly /*IEnumerator<int>*/IReadOnlyBitSetEnumerators.StateFilterEnumerator mActiveIndicesEnumerator;

			internal ActiveItemsEnumerator(ActiveList<T> list)
			{
				mList = list;
				mActiveIndicesEnumerator = list.mSlotStates.SetBitIndices.GetEnumerator();
			}
			public readonly object Clone() =>	MemberwiseClone();

			public readonly T Current =>		mList[mActiveIndicesEnumerator.Current];
			readonly object System.Collections.IEnumerator.Current =>	this.Current;
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0251:Make member 'readonly'")]
			void IDisposable.Dispose() =>		((IDisposable)mActiveIndicesEnumerator).Dispose();
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0251:Make member 'readonly'")]
			public void Reset() =>				mActiveIndicesEnumerator.Reset();
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0251:Make member 'readonly'")]
			public bool MoveNext() =>			mActiveIndicesEnumerator.MoveNext();
		};
	};
}
