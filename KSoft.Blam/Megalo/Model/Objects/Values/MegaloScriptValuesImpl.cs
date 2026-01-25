#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	using Proto;

	partial class MegaloScriptModel
	{
		internal MegaloScriptValueBase NewValue(Proto.MegaloScriptValueType valueType)
		{
			var base_type = valueType.BaseType;
			return base_type switch
			{
				MegaloScriptValueBaseType.Bool =>			new MegaloScriptBoolValue(valueType),

				MegaloScriptValueBaseType.Int =>			new MegaloScriptIntValue(valueType),
				MegaloScriptValueBaseType.UInt =>			new MegaloScriptUIntValue(valueType),
				MegaloScriptValueBaseType.Single =>			new MegaloScriptSingleValue(valueType),
				MegaloScriptValueBaseType.Point3d =>		new MegaloScriptPoint3dValue(valueType),

				MegaloScriptValueBaseType.Flags =>			new MegaloScriptFlagsValue(valueType),
				MegaloScriptValueBaseType.Enum =>			new MegaloScriptEnumValue(valueType),
				MegaloScriptValueBaseType.Index =>			new MegaloScriptIndexValue(valueType),

				MegaloScriptValueBaseType.Var =>			new MegaloScriptVarIndexValue(valueType),
				MegaloScriptValueBaseType.VarReference =>	new MegaloScriptVarReferenceValue(valueType),
				MegaloScriptValueBaseType.Tokens =>			new MegaloScriptTokensValue(this, valueType),

				MegaloScriptValueBaseType.VirtualTrigger =>	new MegaloScriptVirtualTriggerValue(valueType),
				MegaloScriptValueBaseType.Shape =>			new MegaloScriptShapeValue(valueType),
				MegaloScriptValueBaseType.TargetVar =>		new MegaloScriptTargetVarValue(valueType),
				MegaloScriptValueBaseType.TeamFilterParameters =>
															new MegaloScriptTeamFilterParametersValue(valueType),
				MegaloScriptValueBaseType.NavpointIconParameters =>
															new MegaloScriptNavpointIconParametersValue(valueType),
				MegaloScriptValueBaseType.WidgetMeterParameters =>
															new MegaloScriptWidgetMeterParametersValue(valueType),
				MegaloScriptValueBaseType.ObjectReferenceWithPlayerVarIndex =>
															new MegaloScriptObjectReferenceWithPlayerVarIndexValue(valueType),

				_ => throw new KSoft.Debug.UnreachableException(base_type.ToString()),
			};
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Bool = {Value}")]
	public sealed partial class MegaloScriptBoolValue
		: MegaloScriptValueBase
	{
		#region Value
		bool mValue;
		public bool Value {
			get { return mValue; }
			set { mValue = value;
				NotifyPropertyChanged(kValueChanged);
		} }
		#endregion

		public MegaloScriptBoolValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.Bool);
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptBoolValue)model.CreateValue(ValueType);
			result.Value = Value;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = KSoft.Debug.TypeCheck.CastReference<MegaloScriptBoolValue>(other);

			return Value == obj.Value;
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.Stream(ref mValue);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamCursor(ref mValue);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Int = {Value}")]
	public sealed partial class MegaloScriptIntValue
		: MegaloScriptValueBase
	{
		#region Value
		int mValue;
		public int Value {
			get { return mValue; }
			set { mValue = value;
				NotifyPropertyChanged(kValueChanged);
		} }
		#endregion

		public MegaloScriptIntValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.Int);
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptIntValue)model.CreateValue(ValueType);
			result.Value = Value;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = KSoft.Debug.TypeCheck.CastReference<MegaloScriptIntValue>(other);

			return Value == obj.Value;
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.Stream(ref mValue, ValueType.BitLength, signExtend:true);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamCursor(ref mValue);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, UInt = {Value}")]
	public sealed partial class MegaloScriptUIntValue
		: MegaloScriptValueBase
	{
		#region Value
		uint mValue;
		public uint Value {
			get { return mValue; }
			set { mValue = value;
				NotifyPropertyChanged(kValueChanged);
		} }
		#endregion

		public MegaloScriptUIntValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.UInt);
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptUIntValue)model.CreateValue(ValueType);
			result.Value = Value;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = KSoft.Debug.TypeCheck.CastReference<MegaloScriptUIntValue>(other);

			return Value == obj.Value;
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.Stream(ref mValue, ValueType.BitLength);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamCursor(ref mValue);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Single = {Value}")]
	public sealed partial class MegaloScriptSingleValue
		: MegaloScriptValueBase
	{
		#region Value
		float mValue;
		public float Value {
			get { return mValue; }
			set { mValue = value;
				NotifyPropertyChanged(kValueChanged);
		} }
		#endregion

		public MegaloScriptSingleValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.Single);
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptSingleValue)model.CreateValue(ValueType);
			result.Value = Value;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = KSoft.Debug.TypeCheck.CastReference<MegaloScriptSingleValue>(other);

			return Value == obj.Value;
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			int encoding_index = ValueType.EncodingIndex;
			if (encoding_index.IsNone())
			{
				s.Stream(ref mValue);
			}
			else
			{
				var encoding = model.Database.SingleEncodings[encoding_index];
				s.Stream(ref mValue, encoding.Min, encoding.Max, encoding.BitLength, encoding.IsSigned, encoding.Flag1);
			}
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamCursor(ref mValue);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, X = {X}, Y = {Y}, Z = {Z}")]
	public sealed partial class MegaloScriptPoint3dValue
		: MegaloScriptValueBase
	{
		#region Value X
		int mX;
		public int X {
			get { return mX; }
			set { mX = value;
				NotifyPropertyChanged(kXChanged);
		} }
		#endregion
		#region Value Y
		int mY;
		public int Y {
			get { return mY; }
			set { mY = value;
				NotifyPropertyChanged(kYChanged);
		} }
		#endregion
		#region Value Z
		int mZ;
		public int Z {
			get { return mZ; }
			set { mZ = value;
				NotifyPropertyChanged(kZChanged);
		} }
		#endregion

		public MegaloScriptPoint3dValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.Point3d);
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptPoint3dValue)model.CreateValue(ValueType);
			result.X = X;
			result.Y = Y;
			result.Z = Z;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = KSoft.Debug.TypeCheck.CastReference<MegaloScriptPoint3dValue>(other);

			return X == obj.X && Y == obj.Y && Z == obj.Z;
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.Stream(ref mX, ValueType.BitLength, signExtend:ValueType.PointIsSigned);
			s.Stream(ref mY, ValueType.BitLength, signExtend:ValueType.PointIsSigned);
			s.Stream(ref mZ, ValueType.BitLength, signExtend:ValueType.PointIsSigned);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOpt("x", ref mX, Predicates.IsNotZero);
			s.StreamAttributeOpt("y", ref mY, Predicates.IsNotZero);
			s.StreamAttributeOpt("z", ref mZ, Predicates.IsNotZero);
		}
		#endregion
	};


	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, {ValueType.VarSet}.{ValueType.VarType} = {Value}")]
	public sealed partial class MegaloScriptVarIndexValue
		: MegaloScriptValueBase
	{
		#region Value
		int mValue;
		public int Value {
			get { return mValue; }
			set { mValue = value;
				NotifyPropertyChanged(kValueChanged);
		} }
		#endregion

		public MegaloScriptVarIndexValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.Var);
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptVarIndexValue)model.CreateValue(ValueType);
			result.Value = Value;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = KSoft.Debug.TypeCheck.CastReference<MegaloScriptVarIndexValue>(other);

			return Value == obj.Value;
		}

		public bool ValidateValue(MegaloScriptModel model)
		{
			return Value.IsNone() || model.VarIndexIsValid(ValueType.VarType, ValueType.VarSet, Value);
		}

		public void ChangeValue(MegaloScriptModel model, string varName)
		{
			Contract.Requires(!string.IsNullOrEmpty(varName));

			var id_resolving_ctxt = new MegaloScriptModelVariableSet.IndexNameResolvingContext(model, ValueType);
			Value = MegaloScriptModelVariableSet.IndexNameResolvingContext.IdResolver(id_resolving_ctxt, varName);
		}

		// #NOTE_BLAM: Assumes all Parameter instances need PointerHasValue encoding
		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.StreamIndex(ref mValue, ValueType.BitLength);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			if ((model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.UseIndexNames) != 0)
			{
				var resolving_ctxt = new MegaloScriptModelVariableSet.IndexNameResolvingContext(model, ValueType, supportNone:true);
				s.StreamCursorIdAsString(ref mValue, resolving_ctxt,
					MegaloScriptModelVariableSet.IndexNameResolvingContext.IdResolver,
					MegaloScriptModelVariableSet.IndexNameResolvingContext.NameResolver);
			}
			else
			{
				s.StreamCursor(ref mValue);
			}

			if (!ValidateValue(model))
			{
				throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"VarIndex value #{0} has an invalid value {1}",
					Id, Value));
			}
		}
		#endregion
	};
}
