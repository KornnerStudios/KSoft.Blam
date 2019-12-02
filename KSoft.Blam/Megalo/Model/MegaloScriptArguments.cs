using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

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
					continue;

				Values[value_id] = null;
			}
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public partial class MegaloScriptArguments
		: IReadOnlyList<int>
	{
		public Proto.IMegaloScriptProtoObjectWithParams ProtoData { get; private set; }
		readonly int[] mValueIds;

		public MegaloScriptArguments(MegaloScriptModel model, Proto.IMegaloScriptProtoObjectWithParams protoObj, params int[] valueIds)
		{
			Contract.Requires(model != null);
			Contract.Requires(protoObj != null);
			Contract.Requires(valueIds != null); // should never happen unless we're explicitly passed null
			Contract.Requires(valueIds.Length==0 || valueIds.Length==protoObj.ParameterList.Count,
				"Either don't specify the parameter values or specify them all");

			ProtoData = protoObj;
			this.mValueIds = new int[protoObj.ParameterList.Count];
			if (valueIds.Length == 0)
				for (int x = 0; x < this.mValueIds.Length; x++)
					this.mValueIds[x] = TypeExtensions.kNone;
			else
				Array.Copy(valueIds, this.mValueIds, valueIds.Length);
		}

		#region Initialize values
		public void InitializeEmptyValues(MegaloScriptModel model)
		{
			int change_count = 0;
			for (int x = 0; x < mValueIds.Length; x++)
			{
				if (mValueIds[x].IsNotNone()) continue;
				change_count++;

				var value = model.CreateValue(ProtoData.ParameterList[x].Type);
				mValueIds[x] = value.Id;
			}

			if (change_count > 0)
				NotifyItemsInitialized();
		}
		public void InitializeValues(params MegaloScriptValueBase[] values)
		{
			Contract.Requires(values.Length == Count);

			for (int x = 0; x < mValueIds.Length; x++)
			{
				Contract.Assert(values[x] != null);
				Contract.Assert(ProtoData.ParameterList[x].Type.Equals(values[x].ValueType));
				mValueIds[x] = values[x].Id;
			}

			NotifyItemsInitialized();
		}
		#endregion

		#region Get
		public T Get<T>(MegaloScriptModel model, int paramIndex)
			where T : MegaloScriptValueBase
		{
			Contract.Requires(paramIndex >= 0 && paramIndex < ProtoData.ParameterList.Count);

			int value_id = mValueIds[paramIndex];
			Contract.Assert(value_id.IsNotNone());

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
				if (s.IsWriting) Contract.Assert(value_id.IsNotNone());
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
			foreach (var node in s.ElementsByName("Param")) using (s.EnterCursorBookmark(node))
			{
				if (embedValues)
					MegaloScriptValueBase.SerializeValueForEmbed(model, s, ref mValueIds[param_index]);
				else
					s.StreamCursor(ref mValueIds[param_index]);

				Contract.Assert(mValueIds[param_index].IsNotNone());
				param_index++;

				if (param_index == mValueIds.Length) break;
			}
		}
		void Write<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s,
			bool embedValues)
			where TDoc : class
			where TCursor : class
		{
			bool multiple_params = mValueIds.Length > 1;
			bool write_extra_info = s.IsWriting && model.TagElementStreamSerializeFlags.HasParamFlags();

			for (int x = 0; x < mValueIds.Length; x++) using (s.EnterCursorBookmark("Param"))
			{
				if (write_extra_info)
					ProtoData.ParameterList[x].WriteExtraModelInfo(model.Database, s, multiple_params, model.TagElementStreamSerializeFlags);

				Contract.Assert(mValueIds[x].IsNotNone());
				if (embedValues)
					MegaloScriptValueBase.SerializeValueForEmbed(model, s, ref mValueIds[x]);
				else
					s.StreamCursor(ref mValueIds[x]);
			}
		}
		public void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool embed_values = model.TagElementStreamSerializeFlags.EmbedObjects();

			if (s.IsReading)Read (model, s, embed_values);
			else			Write(model, s, embed_values);
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