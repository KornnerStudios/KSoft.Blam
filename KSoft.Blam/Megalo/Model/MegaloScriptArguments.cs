using System;
using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModel
	{
		void HandleRemoval(MegaloScriptArguments args)
		{
			foreach (var value_id in args)
			{
				var value = Values[value_id];
				if (value.IsGlobal)
				{
					continue;
				}

				Values[value_id] = null;
			}
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public partial class MegaloScriptArguments
		: IReadOnlyList<int>
	{
		public Proto.IMegaloScriptProtoObjectWithParams ProtoData { get; private set; }
		readonly int[] mValueIds;

		public MegaloScriptArguments(MegaloScriptModel model, Proto.IMegaloScriptProtoObjectWithParams protoObj, params int[] valueIds)
		{
			ArgumentNullException.ThrowIfNull(model);
			ArgumentNullException.ThrowIfNull(protoObj);
			ArgumentNullException.ThrowIfNull(valueIds);

			if (valueIds.Length != 0 && valueIds.Length != protoObj.ParameterList.Count)
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Either don't specify parameter values or specify them all; expected 0 or {0}, actual {1}.",
					protoObj.ParameterList.Count, valueIds.Length), nameof(valueIds));
			}

			ProtoData = protoObj;
			this.mValueIds = new int[protoObj.ParameterList.Count];
			if (valueIds.Length == 0)
			{
				for (int x = 0; x < this.mValueIds.Length; x++)
				{
					this.mValueIds[x] = TypeExtensions.kNone;
				}
			}
			else
			{
				Array.Copy(valueIds, this.mValueIds, valueIds.Length);
			}
		}

		#region Initialize values
		public void InitializeEmptyValues(MegaloScriptModel model)
		{
			int change_count = 0;
			for (int x = 0; x < mValueIds.Length; x++)
			{
				if (mValueIds[x].IsNotNone()) { continue; }
				change_count++;

				var value = model.CreateValue(ProtoData.ParameterList[x].Type);
				mValueIds[x] = value.Id;
			}

			if (change_count > 0)
			{
				NotifyItemsInitialized();
			}
		}
		public void InitializeValues(params MegaloScriptValueBase[] values)
		{
			ArgumentNullException.ThrowIfNull(values);

			if (values.Length != Count)
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Expected {0} values, but received {1}.", Count, values.Length), nameof(values));
			}

			for (int x = 0; x < mValueIds.Length; x++)
			{
				if (values[x] == null)
				{
					throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
						"Value at parameter index {0} cannot be null.", x), nameof(values));
				}
				if (!ProtoData.ParameterList[x].Type.Equals(values[x].ValueType))
				{
					throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
						"Value at parameter index {0} has type {1}; expected {2}.",
						x, values[x].ValueType, ProtoData.ParameterList[x].Type), nameof(values));
				}

				mValueIds[x] = values[x].Id;
			}

			NotifyItemsInitialized();
		}
		#endregion

		#region Get
		public T Get<T>(MegaloScriptModel model, int paramIndex)
			where T : MegaloScriptValueBase
		{
			ArgumentNullException.ThrowIfNull(model);

			if (paramIndex < 0 || paramIndex >= ProtoData.ParameterList.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(paramIndex), paramIndex,
					string.Format(Util.InvariantCultureInfo,
						"Parameter index must be between 0 and {0}.", ProtoData.ParameterList.Count - 1));
			}

			int value_id = mValueIds[paramIndex];
			if (value_id.IsNone())
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Parameter index {0} has not been initialized with a script value.", paramIndex));
			}

			return (T)model.Values[value_id];
		}
		public MegaloScriptValueBase Get(MegaloScriptModel model, int paramIndex)
		{
			return Get<MegaloScriptValueBase>(model, paramIndex);
		}
		#endregion

		public bool ValuesEqual(MegaloScriptModel model, MegaloScriptArguments other)
		{
			bool equals = ProtoData.ParameterList.Count == other.ProtoData.ParameterList.Count;

			for (int x = 0; equals && x < mValueIds.Length; x++)
			{
				var value = Get(model, x);
				var other_value = other.Get(model, x);

				equals &= value.Equals(other_value);
			}

			return equals;
		}

		#region IBitStreamSerializable Members
		public void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			foreach (var value_id in mValueIds)
			{
				if (s.IsWriting)
				{
					if (value_id.IsNone())
					{
						throw new InvalidOperationException(
							"Cannot write Megalo script arguments before all parameter values are initialized.");
					}
				}

				var value = model.Values[value_id];
				value.Serialize(model, s);
			}
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		void Read<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s,
			bool embedValues)
			where TDoc : class
			where TCursor : class
		{
			int param_index = 0;
			foreach (var node in s.ElementsByName("Param"))
			{
				if (param_index >= mValueIds.Length)
				{
					s.ThrowReadException(new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
						"Encountered more Param elements than expected; expected {0}.", mValueIds.Length)));
					continue;
				}

				using (s.EnterCursorBookmark(node))
				{
					if (embedValues)
					{
						MegaloScriptValueBase.SerializeValueForEmbed(model, s, ref mValueIds[param_index]);
					}
					else
					{
						s.StreamCursor(ref mValueIds[param_index]);
					}

					if (mValueIds[param_index].IsNone())
					{
						s.ThrowReadException(new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
							"Param element at index {0} did not resolve to a script value.", param_index)));
					}

					param_index++;

					if (param_index == mValueIds.Length) { break; }
				}
			}
		}
		void Write<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s,
			bool embedValues)
			where TDoc : class
			where TCursor : class
		{
			bool multiple_params = mValueIds.Length > 1;
			bool write_extra_info = s.IsWriting && model.TagElementStreamSerializeFlags.HasParamFlags();

			for (int x = 0; x < mValueIds.Length; x++)
			{
				using (s.EnterCursorBookmark("Param"))
				{
					if (write_extra_info)
					{
						ProtoData.ParameterList[x].WriteExtraModelInfo(model.Database, s, multiple_params, model.TagElementStreamSerializeFlags);
					}

					if (mValueIds[x].IsNone())
					{
						throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
							"Cannot write Param element at index {0}; script value is not initialized.", x));
					}

					if (embedValues)
					{
						MegaloScriptValueBase.SerializeValueForEmbed(model, s, ref mValueIds[x]);
					}
					else
					{
						s.StreamCursor(ref mValueIds[x]);
					}
				}
			}
		}
		public void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool embed_values = model.TagElementStreamSerializeFlags.EmbedObjects();

			if (s.IsReading){ Read (model, s, embed_values); }
			else			{ Write(model, s, embed_values); }
		}
		#endregion

		#region IList<int> Members
#if false
		int IList<int>.IndexOf(int item)
		{
			return Array.IndexOf(mValueIds, item);
		}
#endif

		public int this[int index] {
			get { return mValueIds[index]; }
			set { int old_value_id = mValueIds[index];
				mValueIds[index] = value;
				NotifyItemChanged(index, old_value_id, value);
		} }
		#endregion

		#region IReadOnlyCollection<int> Members
#if false
		bool ICollection<int>.Contains(int item)
		{
			foreach (var id in mValueIds)
				if (id == item)
					return true;

			return false;
		}

		void IReadOnlyCollection<int>.CopyTo(int[] array, int arrayIndex)
		{
			Array.Copy(mValueIds, 0, array, arrayIndex, mValueIds.Length);
		}
#endif
		public int /*IReadOnlyCollection<int>.*/Count { get { return mValueIds.Length; } }
		#endregion

		#region IEnumerable<int> Members
		public IEnumerator<int> GetEnumerator()
		{
			return (IEnumerator<int>)mValueIds.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return mValueIds.GetEnumerator();
		}
		#endregion
	};
}
