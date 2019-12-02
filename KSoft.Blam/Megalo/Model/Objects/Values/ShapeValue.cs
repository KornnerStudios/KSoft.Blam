#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	using Proto;

	using MegaloScriptShapeTypeBitStreamer = IO.EnumBitStreamer<MegaloScriptShapeType>;

	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed partial class MegaloScriptShapeValue
		: MegaloScriptValueBase
	{
		#region ShapeType
		MegaloScriptShapeType mShapeType;
		public MegaloScriptShapeType ShapeType {
			get { return mShapeType; }
			set { mShapeType = value;
				NotifyPropertyChanged(kShapeTypeChanged);
		} }
		#endregion

		#region Radius
		MegaloScriptVariableReferenceData mRadius = MegaloScriptVariableReferenceData.Custom;
		public MegaloScriptVariableReferenceData Radius {
			get { return mRadius; }
			set { mRadius = value;
				NotifyPropertyChanged(kRadiusChanged);
		} }
		#endregion
		#region Length
		MegaloScriptVariableReferenceData mLength = MegaloScriptVariableReferenceData.Custom;
		public MegaloScriptVariableReferenceData Length {
			get { return mLength; }
			set { mLength = value;
				NotifyPropertyChanged(kLengthChanged);
		} }
		#endregion
		#region Top
		MegaloScriptVariableReferenceData mTop = MegaloScriptVariableReferenceData.Custom;
		public MegaloScriptVariableReferenceData Top {
			get { return mTop; }
			set { mTop = value;
				NotifyPropertyChanged(kTopChanged);
		} }
		#endregion
		#region Bottom
		MegaloScriptVariableReferenceData mBottom = MegaloScriptVariableReferenceData.Custom;
		public MegaloScriptVariableReferenceData Bottom {
			get { return mBottom; }
			set { mBottom = value;
				NotifyPropertyChanged(kBottomChanged);
		} }
		#endregion

		public MegaloScriptShapeValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.Shape);
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptShapeValue)model.CreateValue(ValueType);
			result.ShapeType = ShapeType;

			switch (ShapeType)
			{
				case MegaloScriptShapeType.Sphere:
					result.Radius = Radius;
					break;
				case MegaloScriptShapeType.Cylinder:
					result.Radius = Radius;
					result.Top = Top;
					result.Bottom = Bottom;
					break;
				case MegaloScriptShapeType.Box:
					result.Radius = Radius;
					result.Length = Length;
					result.Top = Top;
					result.Bottom = Bottom;
					break;
				case MegaloScriptShapeType.None: break;

				default: throw new KSoft.Debug.UnreachableException(ShapeType.ToString());
			}

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = (MegaloScriptShapeValue)other;

			bool equals = ShapeType == obj.ShapeType;

			switch (ShapeType)
			{
				case MegaloScriptShapeType.Sphere:
					equals &= Radius.Equals(obj.Radius);
					break;
				case MegaloScriptShapeType.Cylinder:
					equals &= Radius.Equals(obj.Radius) &&
						Top.Equals(obj.Top) && Bottom.Equals(obj.Bottom);
					break;
				case MegaloScriptShapeType.Box:
					equals &= Radius.Equals(obj.Radius) &&
						Length.Equals(obj.Length) &&
						Top.Equals(obj.Top) && Bottom.Equals(obj.Bottom);
					break;
				case MegaloScriptShapeType.None: break;

				default: throw new KSoft.Debug.UnreachableException(ShapeType.ToString());
			}

			return equals;
		}

		#region SetAs
		public void SetAsNone()
		{
			mRadius = MegaloScriptVariableReferenceData.Custom;
			mLength = MegaloScriptVariableReferenceData.Custom;
			mTop = MegaloScriptVariableReferenceData.Custom;
			mBottom = MegaloScriptVariableReferenceData.Custom;

			ShapeType = MegaloScriptShapeType.None;
		}
		public void SetAsSphere(MegaloScriptVariableReferenceData radius)
		{
			Contract.Requires(radius.ReferenceKind == MegaloScriptVariableReferenceType.Custom);

			mRadius = radius;
			mLength = MegaloScriptVariableReferenceData.Custom;
			mTop = MegaloScriptVariableReferenceData.Custom;
			mBottom = MegaloScriptVariableReferenceData.Custom;

			NotifyPropertyChanged(kRadiusChanged);
			ShapeType = MegaloScriptShapeType.Sphere;
		}
		public void SetAsCylinder(MegaloScriptVariableReferenceData radius,
			MegaloScriptVariableReferenceData top, MegaloScriptVariableReferenceData bottom)
		{
			Contract.Requires(radius.ReferenceKind == MegaloScriptVariableReferenceType.Custom);
			Contract.Requires(top.ReferenceKind == MegaloScriptVariableReferenceType.Custom);
			Contract.Requires(bottom.ReferenceKind == MegaloScriptVariableReferenceType.Custom);

			mRadius = radius;
			mLength = MegaloScriptVariableReferenceData.Custom;
			mTop = top;
			mBottom = bottom;

			NotifyPropertyChanged(kRadiusChanged);
			NotifyPropertyChanged(kTopChanged);
			NotifyPropertyChanged(kBottomChanged);
			ShapeType = MegaloScriptShapeType.Cylinder;
		}
		public void SetAsBox(MegaloScriptVariableReferenceData width, MegaloScriptVariableReferenceData length,
			MegaloScriptVariableReferenceData top, MegaloScriptVariableReferenceData bottom)
		{
			Contract.Requires(width.ReferenceKind == MegaloScriptVariableReferenceType.Custom);
			Contract.Requires(length.ReferenceKind == MegaloScriptVariableReferenceType.Custom);
			Contract.Requires(top.ReferenceKind == MegaloScriptVariableReferenceType.Custom);
			Contract.Requires(bottom.ReferenceKind == MegaloScriptVariableReferenceType.Custom);

			mRadius = width;
			mLength = length;
			mTop = top;
			mBottom = bottom;

			NotifyPropertyChanged(kRadiusChanged);
			NotifyPropertyChanged(kLengthChanged);
			NotifyPropertyChanged(kTopChanged);
			NotifyPropertyChanged(kBottomChanged);
			ShapeType = MegaloScriptShapeType.Box;
		}
		#endregion

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.Stream(ref mShapeType, 2, MegaloScriptShapeTypeBitStreamer.Instance);

			switch (ShapeType)
			{
				case MegaloScriptShapeType.Sphere:
					mRadius.SerializeCustom(model, s);
					break;
				case MegaloScriptShapeType.Cylinder:
					mRadius.SerializeCustom(model, s);
					mTop.SerializeCustom(model, s);
					mBottom.SerializeCustom(model, s);
					break;
				case MegaloScriptShapeType.Box:
					mRadius.SerializeCustom(model, s);
					mLength.SerializeCustom(model, s);
					mTop.SerializeCustom(model, s);
					mBottom.SerializeCustom(model, s);
					break;
				case MegaloScriptShapeType.None: break;

				default: throw new KSoft.Debug.UnreachableException(ShapeType.ToString());
			}
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		const string kVar0ElementName = "Radius";
		const string kVar1ElementName = "Length";
		const string kVar2ElementName = "Top";
		const string kVar3ElementName = "Bottom";

		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeEnum("shapeType", ref mShapeType);

			switch (ShapeType)
			{
				case MegaloScriptShapeType.Sphere:
					using (s.EnterCursorBookmark(kVar0ElementName)) mRadius.SerializeCustom(model, s);
					break;
				case MegaloScriptShapeType.Cylinder:
					using (s.EnterCursorBookmark(kVar0ElementName)) mRadius.SerializeCustom(model, s);
					using (s.EnterCursorBookmark(kVar2ElementName)) mTop.SerializeCustom(model, s);
					using (s.EnterCursorBookmark(kVar3ElementName)) mBottom.SerializeCustom(model, s);
					break;
				case MegaloScriptShapeType.Box:
					using (s.EnterCursorBookmark("Width"))			mRadius.SerializeCustom(model, s);
					using (s.EnterCursorBookmark(kVar1ElementName)) mLength.SerializeCustom(model, s);
					using (s.EnterCursorBookmark(kVar2ElementName)) mTop.SerializeCustom(model, s);
					using (s.EnterCursorBookmark(kVar3ElementName)) mBottom.SerializeCustom(model, s);
					break;
				case MegaloScriptShapeType.None: break;

				default: throw new KSoft.Debug.UnreachableException(ShapeType.ToString());
			}
		}
		#endregion
	};
}