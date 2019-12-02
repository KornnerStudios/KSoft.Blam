using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	using MegaloScriptVariableReferenceTypeBitStreamer = IO.EnumBitStreamer<MegaloScriptVariableReferenceType>;

	[System.Reflection.Obfuscation(Exclude=false)]
	public struct MegaloScriptVariableReferenceData
		: IComparable<MegaloScriptVariableReferenceData>
		, IEquatable<MegaloScriptVariableReferenceData>
	{
		#region Defaults
		public static readonly MegaloScriptVariableReferenceData Null = new MegaloScriptVariableReferenceData
		{ mReferenceKind = MegaloScriptVariableReferenceType.Undefined, Type = -1, DataType = -1, Data = -1 };
		public static readonly MegaloScriptVariableReferenceData Custom = new MegaloScriptVariableReferenceData
		{ mReferenceKind = MegaloScriptVariableReferenceType.Custom, Type = -1, DataType = -1, Data = -1 };
		public static readonly MegaloScriptVariableReferenceData Player = new MegaloScriptVariableReferenceData
		{ mReferenceKind = MegaloScriptVariableReferenceType.Player, Type = -1, DataType = -1, Data = -1 };
		public static readonly MegaloScriptVariableReferenceData Object = new MegaloScriptVariableReferenceData
		{ mReferenceKind = MegaloScriptVariableReferenceType.Object, Type = -1, DataType = -1, Data = -1 };
		public static readonly MegaloScriptVariableReferenceData Team = new MegaloScriptVariableReferenceData
		{ mReferenceKind = MegaloScriptVariableReferenceType.Team, Type = -1, DataType = -1, Data = -1 };
		public static readonly MegaloScriptVariableReferenceData Timer = new MegaloScriptVariableReferenceData
		{ mReferenceKind = MegaloScriptVariableReferenceType.Timer, Type = -1, DataType = -1, Data = -1 };
		#endregion

		public bool IsNull { get { return Type == KSoft.TypeExtensions.kNoneInt8; } }

		MegaloScriptVariableReferenceType mReferenceKind;
		public MegaloScriptVariableReferenceType ReferenceKind { get { return mReferenceKind; } }

		/// <summary>Reference kind's member index. IE, the actual 'reference type'</summary>
		public sbyte Type;

		/// <summary>If <see cref="Data"/> is typed, this represents that information</summary>
		public int DataType;
		public int Data;

		/// <summary>Model helper only: Initialize the core properties of a variable reference</summary>
		/// <param name="model"></param>
		/// <param name="result">The initialized variable reference</param>
		/// <param name="refKind">The kind of variable reference to create</param>
		/// <param name="refTypeMember">Returns the proto member we've initialized the result to</param>
		/// <param name="refMemberName">The specific member of the variable reference to initialize to</param>
		/// <param name="dataTypeName">Only used if the reference member has a data type property</param>
		internal static void Initialize(MegaloScriptModel model,
			out MegaloScriptVariableReferenceData result, MegaloScriptVariableReferenceType refKind,
			out Proto.MegaloScriptProtoVariableReferenceMember refTypeMember,
			string refMemberName, string dataTypeName = null)
		{
			result.mReferenceKind = refKind;

			var protoType = model.Database.VariableRefTypes[refKind];
			result.Type = MegaloScriptVariableReferenceData.ToMemberIndex(protoType, refMemberName);

			refTypeMember = protoType.Members[result.Type];
			if (refTypeMember.HasDataType)
			{
				Contract.Assert(dataTypeName != null, "Reference type uses a data type parameter, but one wasn't defined");
				var id_resolving_ctxt = new Proto.MegaloScriptEnum.EnumNameResolvingContext(model.Database, refTypeMember.EnumValueType);
				result.DataType = Proto.MegaloScriptEnum.EnumNameResolvingContext.IdResolver(id_resolving_ctxt, dataTypeName);
			}
			else
				result.DataType = TypeExtensions.kNone;

			result.Data = TypeExtensions.kNone;
		}

		internal static readonly Func<Proto.MegaloScriptProtoVariableReference, string, sbyte> ToMemberIndex =
			(ctxt, name) => (sbyte)ctxt.NameToMember[name].TypeIndex;
		internal static readonly Func<Proto.MegaloScriptProtoVariableReference, sbyte, string> FromMemberIndex =
			(ctxt, id) => ctxt.Members[id].Name;

		#region IBitStreamSerializable Members
		void SerializeData(MegaloScriptModel model, IO.BitStream s, Proto.MegaloScriptValueType valueType)
		{
			var base_type = valueType.BaseType;
			switch (base_type)
			{
				case Proto.MegaloScriptValueBaseType.Int:
					s.Stream(ref Data, valueType.BitLength, signExtend:true);
					break;
				case Proto.MegaloScriptValueBaseType.UInt:
				case Proto.MegaloScriptValueBaseType.Var:
					s.Stream(ref Data, valueType.BitLength);
					break;

				case Proto.MegaloScriptValueBaseType.Enum:
					MegaloScriptEnumValue.SerializeValue(model, s, valueType, ref Data);
					break;

				case Proto.MegaloScriptValueBaseType.Index:
					MegaloScriptIndexValue.SerializeValue(model, s, valueType, ref Data);
					break;

				default: throw new KSoft.Debug.UnreachableException(base_type.ToString());
			}
		}
		void Serialize(MegaloScriptModel model, IO.BitStream s, Proto.MegaloScriptProtoVariableReference protoType)
		{
			s.Stream(ref Type, protoType.TypeBitLength);
			if (Type < 0 || Type >= protoType.Members.Count)
				throw new System.IO.InvalidDataException(string.Format("{0}/{1}: Encountered invalid {2} type data",
					s.StreamName, model.MegaloVariant.BaseVariant.Header.Title, mReferenceKind));

			var member = protoType.Members[Type];

			if (member.HasDataType)
				MegaloScriptEnumValue.SerializeValue(model, s, member.EnumValueType, ref DataType);
			else
				DataType = TypeExtensions.kNone;

			if (member.HasDataValue)
				SerializeData(model, s, member.ValueType);
			else
				Data = TypeExtensions.kNone;
		}
		#region Serialize explicit-kind
		internal void SerializeCustom(MegaloScriptModel model, IO.BitStream s)
		{
			mReferenceKind = MegaloScriptVariableReferenceType.Custom;

			Serialize(model, s, model.Database.VariableRefTypes[mReferenceKind]);
		}
		internal void SerializePlayer(MegaloScriptModel model, IO.BitStream s)
		{
			mReferenceKind = MegaloScriptVariableReferenceType.Player;

			Serialize(model, s, model.Database.VariableRefTypes[mReferenceKind]);
		}
		internal void SerializeObject(MegaloScriptModel model, IO.BitStream s)
		{
			mReferenceKind = MegaloScriptVariableReferenceType.Object;

			Serialize(model, s, model.Database.VariableRefTypes[mReferenceKind]);
		}
		internal void SerializeTeam(MegaloScriptModel model, IO.BitStream s)
		{
			mReferenceKind = MegaloScriptVariableReferenceType.Team;

			Serialize(model, s, model.Database.VariableRefTypes[mReferenceKind]);
		}
		internal void SerializeTimer(MegaloScriptModel model, IO.BitStream s)
		{
			mReferenceKind = MegaloScriptVariableReferenceType.Timer;

			Serialize(model, s, model.Database.VariableRefTypes[mReferenceKind]);
		}
		#endregion
		internal void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.Stream(ref mReferenceKind, 3, MegaloScriptVariableReferenceTypeBitStreamer.Instance);

			switch (mReferenceKind)
			{
				case MegaloScriptVariableReferenceType.Custom:
				case MegaloScriptVariableReferenceType.Player:
				case MegaloScriptVariableReferenceType.Object:
				case MegaloScriptVariableReferenceType.Team:
				case MegaloScriptVariableReferenceType.Timer:
					Serialize(model, s, model.Database.VariableRefTypes[mReferenceKind]);
					break;

				default: throw new KSoft.Debug.UnreachableException(mReferenceKind.ToString());
			}
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		void SerializeData<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s,
			Proto.MegaloScriptValueType valueType)
			where TDoc : class
			where TCursor : class
		{
			var base_type = valueType.BaseType;
			switch (base_type)
			{
				case Proto.MegaloScriptValueBaseType.Int:
				case Proto.MegaloScriptValueBaseType.UInt:
					s.StreamCursor(ref Data);
					break;

				case Proto.MegaloScriptValueBaseType.Var:
					if ((model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.UseIndexNames) != 0)
					{
						var resolving_ctxt = new MegaloScriptModelVariableSet.IndexNameResolvingContext(model, valueType);
						s.StreamCursorIdAsString(ref Data, resolving_ctxt,
							MegaloScriptModelVariableSet.IndexNameResolvingContext.IdResolver,
							MegaloScriptModelVariableSet.IndexNameResolvingContext.NameResolver);
					}
					else
						s.StreamCursor(ref Data);
					break;

				case Proto.MegaloScriptValueBaseType.Enum:
					MegaloScriptEnumValue.SerializeValue(model, s, valueType, ref Data, IO.TagElementNodeType.Text);
					break;

				case Proto.MegaloScriptValueBaseType.Index:
					MegaloScriptIndexValue.SerializeValue(model, s, valueType, ref Data, IO.TagElementNodeType.Text);
					break;

				default: throw new KSoft.Debug.UnreachableException(base_type.ToString());
			}
		}
		void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s,
			Proto.MegaloScriptProtoVariableReference protoType)
			where TDoc : class
			where TCursor : class
		{
			const string kTypeAttributeName = "varRefType";

			if ((model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.UseEnumNames) != 0)
			{
				s.StreamAttributeIdAsString(kTypeAttributeName, ref Type,
					protoType, ToMemberIndex, FromMemberIndex);
			}
			else
				s.StreamAttribute(kTypeAttributeName, ref Type);

			var member = protoType.Members[Type];

			if (member.HasDataType)
				MegaloScriptEnumValue.SerializeValue(model, s, member.EnumValueType, ref DataType,
					IO.TagElementNodeType.Attribute, "dataType");
			else
				DataType = TypeExtensions.kNone;

			if (member.HasDataValue)
				SerializeData(model, s, member.ValueType);
			else
				Data = TypeExtensions.kNone;
		}
		#region Serialize explicit-kind
		internal void SerializeCustom<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			mReferenceKind = MegaloScriptVariableReferenceType.Custom;

			Serialize(model, s, model.Database.VariableRefTypes[mReferenceKind]);
		}
		internal void SerializePlayer<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			mReferenceKind = MegaloScriptVariableReferenceType.Player;

			Serialize(model, s, model.Database.VariableRefTypes[mReferenceKind]);
		}
		internal void SerializeObject<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			mReferenceKind = MegaloScriptVariableReferenceType.Object;

			Serialize(model, s, model.Database.VariableRefTypes[mReferenceKind]);
		}
		internal void SerializeTeam<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			mReferenceKind = MegaloScriptVariableReferenceType.Team;

			Serialize(model, s, model.Database.VariableRefTypes[mReferenceKind]);
		}
		internal void SerializeTimer<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			mReferenceKind = MegaloScriptVariableReferenceType.Timer;

			Serialize(model, s, model.Database.VariableRefTypes[mReferenceKind]);
		}
		#endregion
		internal void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s,
			bool streamRefKind = true)
			where TDoc : class
			where TCursor : class
		{
			if (streamRefKind)
				s.StreamAttributeEnum("varRefKind", ref mReferenceKind);

			switch (mReferenceKind)
			{
				case MegaloScriptVariableReferenceType.Custom:
				case MegaloScriptVariableReferenceType.Player:
				case MegaloScriptVariableReferenceType.Object:
				case MegaloScriptVariableReferenceType.Team:
				case MegaloScriptVariableReferenceType.Timer:
					Serialize(model, s, model.Database.VariableRefTypes[mReferenceKind]);
					break;

				default: throw new KSoft.Debug.UnreachableException(mReferenceKind.ToString());
			}
		}
		#endregion

		#region IComparable<MegaloScriptVariableReferenceData> Members
		public int CompareTo(MegaloScriptVariableReferenceData other)
		{
			if (mReferenceKind == other.mReferenceKind)
			{
				if (Type == other.Type)
				{
					if (DataType == other.DataType)
						return DataType - other.DataType;
					else
						return Data - other.Data;
				}
				else
					return Type - other.Type;
			}
			else
				return (int)mReferenceKind - (int)other.mReferenceKind;
		}
		#endregion

		#region IEquatable<MegaloScriptVariableReferenceData> Members
		public bool Equals(MegaloScriptVariableReferenceData other)
		{
			return mReferenceKind == other.mReferenceKind &&
				Type == other.Type && DataType == other.DataType && Data == other.Data;
		}
		#endregion

		public override int GetHashCode()
		{
			uint hc = (uint)mReferenceKind << 28;
			hc |= (uint)(Type & 0x7F) << 21;
			hc |= (uint)(DataType & 0x3F) << 16;
			hc |= (uint)(Data & 0xFFFF);

			return (int)hc;
		}
	};
}