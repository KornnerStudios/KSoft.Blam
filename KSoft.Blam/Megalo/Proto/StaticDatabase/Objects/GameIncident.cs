
namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class GameIncident
		: MegaloStaticDataNamedObject
	{
		public bool IsAvailable { get { return kIsAvailable(Name); } }

		#region ITagElementStringNameStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOpt("name", ref Name, kIsAvailable);
		}
		#endregion
	};
}