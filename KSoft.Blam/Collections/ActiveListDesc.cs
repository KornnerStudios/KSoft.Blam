using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
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
}
