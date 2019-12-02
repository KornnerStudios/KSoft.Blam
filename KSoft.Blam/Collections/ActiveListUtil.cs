using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public static class ActiveListUtil
	{
		#region IBitStreamSerializable Members
		public static void Serialize<T, TContext>(IO.BitStream s, ActiveList<T> list, int countBitLength,
			TContext ctxt, Func<IO.BitStream, TContext, T> ctor,
			IReadOnlyList<int> writeOrder = null)
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
					// #REVIEW_BLAM: well, shall we warn?
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
				var read_mode = getReadMode == null
					? TagElementStreamReadMode.PostConstructor
					: getReadMode(ctxt);

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

			if (writeShouldSkip == null)
				writeShouldSkip = Predicates.False;

				 if (s.IsReading) TagElementTextStreamUtils<TDoc,TCursor,T,TContext>.ReadElements(s, elementName, list, ctxt, ctor, getReadMode);
			else if (s.IsWriting) TagElementTextStreamUtils<TDoc,TCursor,T,TContext>.WriteElements(s, elementName, list, writeShouldSkip);
		}
		#endregion
	};
}