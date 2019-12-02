
namespace KSoft.Blam.Megalo.Model
{
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}", Name = "{Name}")]
	public abstract partial class MegaloScriptModelNamedObject
		: MegaloScriptModelObject
	{
		#region Name
		string mName;
		public string Name {
			get { return mName; }
			set { mName = value;
				NotifyPropertyChanged(kNameChanged);
		} }
		#endregion

		protected MegaloScriptModelNamedObject()
		{
			mName = "";
		}

		#region ITagElementStringNameStreamable Members
		protected void SerializeName<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("name", ref mName);
		}
		protected void SerializeNameOpt<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (!s.StreamAttributeOpt("name", ref mName, Predicates.IsNotNullOrEmpty))
				mName = "";
		}
		public override void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(model, s);

			SerializeName(s);
		}
		#endregion
	};
}