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
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mShapeType), AlwaysNotify = true)]
		public partial MegaloScriptShapeType ShapeType { get; set; }
		#endregion

		#region Radius
		MegaloScriptVariableReferenceData mRadius = MegaloScriptVariableReferenceData.Custom;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mRadius), AlwaysNotify = true)]
		public partial MegaloScriptVariableReferenceData Radius { get; set; }
		#endregion
		#region Length
		MegaloScriptVariableReferenceData mLength = MegaloScriptVariableReferenceData.Custom;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mLength), AlwaysNotify = true)]
		public partial MegaloScriptVariableReferenceData Length { get; set; }
		#endregion
		#region Top
		MegaloScriptVariableReferenceData mTop = MegaloScriptVariableReferenceData.Custom;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mTop), AlwaysNotify = true)]
		public partial MegaloScriptVariableReferenceData Top { get; set; }
		#endregion
		#region Bottom
		MegaloScriptVariableReferenceData mBottom = MegaloScriptVariableReferenceData.Custom;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mBottom), AlwaysNotify = true)]
		public partial MegaloScriptVariableReferenceData Bottom { get; set; }
		#endregion

		public MegaloScriptShapeValue(MegaloScriptValueType valueType) : base(valueType)
		{
			ThrowIfUnexpectedBaseType(valueType, MegaloScriptValueBaseType.Shape);
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
			var obj = KSoft.Debug.TypeCheck.CastReference<MegaloScriptShapeValue>(other);

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
			ThrowIfUnexpectedReferenceKind(radius, MegaloScriptVariableReferenceType.Custom, nameof(radius));

			mRadius = radius;
			mLength = MegaloScriptVariableReferenceData.Custom;
			mTop = MegaloScriptVariableReferenceData.Custom;
			mBottom = MegaloScriptVariableReferenceData.Custom;

			NotifyPropertyChanged(kRadiusChangedEventArgs);
			ShapeType = MegaloScriptShapeType.Sphere;
		}
		public void SetAsCylinder(MegaloScriptVariableReferenceData radius,
			MegaloScriptVariableReferenceData top, MegaloScriptVariableReferenceData bottom)
		{
			ThrowIfUnexpectedReferenceKind(radius, MegaloScriptVariableReferenceType.Custom, nameof(radius));
			ThrowIfUnexpectedReferenceKind(top, MegaloScriptVariableReferenceType.Custom, nameof(top));
			ThrowIfUnexpectedReferenceKind(bottom, MegaloScriptVariableReferenceType.Custom, nameof(bottom));

			mRadius = radius;
			mLength = MegaloScriptVariableReferenceData.Custom;
			mTop = top;
			mBottom = bottom;

			NotifyPropertyChanged(kRadiusChangedEventArgs);
			NotifyPropertyChanged(kTopChangedEventArgs);
			NotifyPropertyChanged(kBottomChangedEventArgs);
			ShapeType = MegaloScriptShapeType.Cylinder;
		}
		public void SetAsBox(MegaloScriptVariableReferenceData width, MegaloScriptVariableReferenceData length,
			MegaloScriptVariableReferenceData top, MegaloScriptVariableReferenceData bottom)
		{
			ThrowIfUnexpectedReferenceKind(width, MegaloScriptVariableReferenceType.Custom, nameof(width));
			ThrowIfUnexpectedReferenceKind(length, MegaloScriptVariableReferenceType.Custom, nameof(length));
			ThrowIfUnexpectedReferenceKind(top, MegaloScriptVariableReferenceType.Custom, nameof(top));
			ThrowIfUnexpectedReferenceKind(bottom, MegaloScriptVariableReferenceType.Custom, nameof(bottom));

			mRadius = width;
			mLength = length;
			mTop = top;
			mBottom = bottom;

			NotifyPropertyChanged(kRadiusChangedEventArgs);
			NotifyPropertyChanged(kLengthChangedEventArgs);
			NotifyPropertyChanged(kTopChangedEventArgs);
			NotifyPropertyChanged(kBottomChangedEventArgs);
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
					using (s.EnterCursorBookmark(kVar0ElementName)) { mRadius.SerializeCustom(model, s); }
					break;
				case MegaloScriptShapeType.Cylinder:
					using (s.EnterCursorBookmark(kVar0ElementName)) { mRadius.SerializeCustom(model, s); }
					using (s.EnterCursorBookmark(kVar2ElementName)) { mTop.SerializeCustom(model, s); }
					using (s.EnterCursorBookmark(kVar3ElementName)) { mBottom.SerializeCustom(model, s); }
					break;
				case MegaloScriptShapeType.Box:
					using (s.EnterCursorBookmark("Width"))			{ mRadius.SerializeCustom(model, s); }
					using (s.EnterCursorBookmark(kVar1ElementName)) { mLength.SerializeCustom(model, s); }
					using (s.EnterCursorBookmark(kVar2ElementName)) { mTop.SerializeCustom(model, s); }
					using (s.EnterCursorBookmark(kVar3ElementName)) { mBottom.SerializeCustom(model, s); }
					break;
				case MegaloScriptShapeType.None: break;

				default: throw new KSoft.Debug.UnreachableException(ShapeType.ToString());
			}
		}
		#endregion
	};
}
