
namespace KSoft.Blam.Megalo.Proto
{
	internal struct BuildProtoFiles
		: IO.ITagElementStringNameStreamable
	{
		public static BuildProtoFiles Empty { get { return new BuildProtoFiles(); } }

		public string StaticDatabaseFile;
		public string MegaloDatabaseFile;

		public bool IsEmpty { get { return StaticDatabaseFile.IsNullOrEmpty() || MegaloDatabaseFile.IsNullOrEmpty(); } }

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