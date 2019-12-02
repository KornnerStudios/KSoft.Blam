
namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class UnitCustomApp
		: MegaloStaticDataNamedObject
	{
		public string IconId = "";

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeOpt("icon", ref IconId, Predicates.IsNotNullOrEmpty);
		}
		#endregion
	};
}