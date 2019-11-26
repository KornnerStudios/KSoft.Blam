
namespace KSoft.Blam.RuntimeData.Variants
{
	partial class GameEngineMegaloVariant
	{
		protected virtual MegaloVariantPlayerTraitsBase NewMegaloPlayerTraits()
		{
			return new MegaloVariantPlayerTraitsDefaultImpl(this);
		}

		public int CreateMegaloPlayerTraits(string codeName, int nameStringIndex, int descStringIndex)
		{
			if (PlayerTraits.Count == PlayerTraits.Capacity)
				return TypeExtensions.kNone;

			var traits = NewMegaloPlayerTraits();
			traits.CodeName = codeName;
			traits.NameStringIndex = nameStringIndex;
			traits.DescriptionStringIndex = descStringIndex;

			PlayerTraits.Add(traits);
			return PlayerTraits.Count - 1;
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class MegaloVariantPlayerTraitsBase
		: Blam.Megalo.Model.MegaloScriptAccessibleObjectBase
		, IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		public int NameStringIndex, DescriptionStringIndex;

		public PlayerTraitsBase Traits { get; protected set; }

		protected MegaloVariantPlayerTraitsBase(GameEngineMegaloVariant variant)
		{
			NameStringIndex = DescriptionStringIndex = TypeExtensions.kNone;

			Traits = variant.BaseVariant.NewPlayerTraits();
		}

		public bool IsValid { get { return NameStringIndex.IsNotNone(); } }

		#region IBitStreamSerializable Members
		public virtual void Serialize(IO.BitStream s)
		{
			var megalo = (GameEngineMegaloVariant)s.Owner;

			megalo.StreamStringTableIndexReference(s, ref NameStringIndex);
			megalo.StreamStringTableIndexReference(s, ref DescriptionStringIndex);

			Traits.Serialize(s);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var variant = (GameEngineMegaloVariant)s.Owner;

			variant.SerializeStringTableIndex(s, "nameIndex", ref NameStringIndex);
			variant.SerializeStringTableIndex(s, "descIndex", ref DescriptionStringIndex);

			SerializeCodeName(s);

			using (var bm = s.EnterCursorBookmarkOpt("Traits", Traits, obj=>!obj.IsUnchanged)) if (bm.IsNotNull)
				s.StreamObject(Traits);
		}
		#endregion
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	sealed class MegaloVariantPlayerTraitsDefaultImpl
		: MegaloVariantPlayerTraitsBase
	{
		public MegaloVariantPlayerTraitsDefaultImpl(GameEngineMegaloVariant variant) : base(variant)
		{
		}
	};
}