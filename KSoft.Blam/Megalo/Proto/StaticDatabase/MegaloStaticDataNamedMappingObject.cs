
namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract class MegaloStaticDataNamedMappingObject
		: MegaloStaticDataNamedObject
		, IMegaloStaticDataMappingObject
	{
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
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			SerializeTypeName(s, kAttributeKeyName, ref TypeName);
		}
		#endregion
	};
}