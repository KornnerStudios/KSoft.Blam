
namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class MegaloStaticDataTagNamedObject
		: MegaloStaticDataNamedObject
	{
		public bool IsAvailable { get { return kIsAvailable(Name); } }

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOpt("tagName", ref Name, kIsAvailable);
		}
		#endregion
	};
}