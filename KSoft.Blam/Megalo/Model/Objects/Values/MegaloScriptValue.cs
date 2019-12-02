using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModel
	{
		public MegaloScriptValueBase CreateValue(Proto.MegaloScriptValueType valueType, int id = TypeExtensions.kNone)
		{
			return CreateObjectPostprocess(Values, NewValue(valueType), id);
		}

		/// <summary>Create the identiy of an existing value based on an its core traits</summary>
		/// <param name="valueType"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		internal MegaloScriptValueBase Recreate(Proto.MegaloScriptValueType valueType, int id)
		{
			return CreateValue(valueType, id);
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract partial class MegaloScriptValueBase
		: MegaloScriptModelNamedObject
		, IEquatable<MegaloScriptValueBase>
	{
		public override MegaloScriptModelObjectType ObjectType { get { return MegaloScriptModelObjectType.Value; } }

		public Proto.MegaloScriptValueType ValueType { get; private set; }
		#region IsGlobal
		bool mIsGlobal;
		public bool IsGlobal {
			get { return mIsGlobal; }
			set { mIsGlobal = value;
				NotifyPropertyChanged(kIsGlobalChanged);
		} }
		#endregion

		protected MegaloScriptValueBase(Proto.MegaloScriptValueType valueType)
		{
			ValueType = valueType;
		}

		public abstract MegaloScriptValueBase Copy(MegaloScriptModel model);

		#region NewFrom* Utils
		internal static MegaloScriptValueBase NewFromTagStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, MegaloScriptModel model)
			where TDoc : class
			where TCursor : class
		{
			var value_type = ReadType(s, model.Database);

			return model.NewValue(value_type);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		const string kTypeAttributeName = "type";
		const string kIsGlobalAttributeName = "global";

		/// <summary>When we're streaming in 'EmbedObjects' mode we only want to write globals in the root script node</summary>
		internal static Predicate<MegaloScriptValueBase> SkipIfNotGlobalPredicate = value => value.IsGlobal == false;

		static Proto.MegaloScriptValueType ReadType<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, Proto.MegaloScriptDatabase db)
			where TDoc : class
			where TCursor : class
		{
			string value_type_name = null;
			s.ReadAttribute(kTypeAttributeName, ref value_type_name);
			return db.GetValueType(value_type_name);
		}

		internal static void SerializeValueForEmbed<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s,
			ref int valueId)
			where TDoc : class
			where TCursor : class
		{
			bool is_global = false;
			MegaloScriptValueBase value = null;

			if (s.IsReading)
			{
				if (s.ReadAttributeOpt(kIsGlobalAttributeName, ref is_global) && is_global)
					s.ReadCursor(ref valueId);
				else
				{
					bool streamed_sans_ids = model.TagElementStreamSerializeFlags.EmbedObjectsWriteSansIds();
					var value_type = ReadType(s, model.Database);

					if (streamed_sans_ids)	valueId = TypeExtensions.kNone; // set to NONE to automatically add
					else					SerializeId(s, ref valueId);

					value = model.Recreate(value_type, valueId);

					if (streamed_sans_ids) // since the stream didn't have the id, we need to explicit set it via value
						valueId = value.Id;
				}
			}
			else
			{
				value = model.Values[valueId];
				if (is_global = value.IsGlobal)
				{
					s.WriteAttribute(kIsGlobalAttributeName, true);
					s.WriteCursor(valueId);
				}
			}

			if (!is_global) // stream non-global values essentially like locals
			{
				Contract.Assume(value != null);
				value.Serialize(model, s);
			}
		}

		protected abstract void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		public override void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			SerializeIdOpt(model, s);

			// #NOTE_BLAM: if we're the flags EmbedObjects and WriteParamTypes are on then the type will be written twice on
			// non-globals, but still to the same attribute. 1st write happens in MegaloScriptProtoParam.WriteExtraModelInfo
			// then of course here. Not a problem but we could avoid the second write here
			if (s.IsWriting)
				s.WriteAttribute(kTypeAttributeName, model.Database.ValueTypeNames[ValueType.NameIndex]);

			SerializeNameOpt(s);

			s.StreamAttributeOpt(kIsGlobalAttributeName, ref mIsGlobal, Predicates.IsTrue);

			SerializeValue(model, s);
		}
		#endregion

		#region IEquatable<MegaloScriptValueBase> Members
		protected abstract bool ValueEquals(MegaloScriptValueBase other);

		public bool Equals(MegaloScriptValueBase other)
		{
			return ValueType.Equals(other.ValueType) && ValueEquals(other);
		}
		#endregion
	};
}