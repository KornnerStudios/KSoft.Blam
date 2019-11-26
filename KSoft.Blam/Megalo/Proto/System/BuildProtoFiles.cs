
namespace KSoft.Blam.RuntimeData.Megalo.Proto
{
	internal struct BuildProtoFiles
		: IO.ITagElementStringNameStreamable
	{
		public string StaticDatabaseFile;
		public string MegaloDatabaseFile;

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("staticDb", ref StaticDatabaseFile);
			s.StreamAttribute("megaloDb", ref MegaloDatabaseFile);
		}
		#endregion
	};
}