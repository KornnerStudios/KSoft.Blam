
namespace KSoft.Blam.Megalo.Proto
{
	/// <summary>Remaps a name to an index</summary>
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class MegaloStaticDataReMappingObject
		: IMegaloStaticDataMappingObject
	{
		public int TypeIndex;
		public string TypeName;

		#region IMegaloStaticDataMappingObject Members
		string IMegaloStaticDataMappingObject.TypeName	{ get { return TypeName; } }
		#endregion

		#region ITagElementStringNameStreamable Members
		internal const string kAttributeKeyName = "typeName";
		internal static void SerializeTypeName<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string keyName, ref string typeName)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute(keyName, ref typeName);
		}
		public virtual void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("typeIndex", ref TypeIndex);
			SerializeTypeName(s, kAttributeKeyName, ref TypeName);
		}
		#endregion
	};
}