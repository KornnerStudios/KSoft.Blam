
namespace KSoft.Blam.Megalo.Model
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public abstract partial class MegaloScriptModelObjectWithParameters
		: MegaloScriptModelObject
	{
		#region CommentOut
		bool mCommentOut;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public bool CommentOut {
			get { return mCommentOut; }
			set { mCommentOut = value;
				NotifyPropertyChanged(kCommentOutChangedEventArgs);
		} }
		#endregion

		#region Arguments
		MegaloScriptArguments mArgs = null!;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public MegaloScriptArguments Arguments {
			get { return mArgs; }
			protected set { mArgs = value;
				NotifyPropertyChanged(kArgumentsChangedEventArgs);
		} }
		#endregion

		public T GetArgument<T>(MegaloScriptModel model, int paramIndex)
			where T : MegaloScriptValueBase
		{
			return mArgs.Get<T>(model, paramIndex);
		}
		public MegaloScriptValueBase Get(MegaloScriptModel model, int paramIndex)
		{
			return mArgs.Get(model, paramIndex);
		}
		public bool ArgumentsEqual(MegaloScriptModel model, MegaloScriptModelObjectWithParameters other)
		{
			return mArgs.ValuesEqual(model, other.Arguments);
		}

		protected void SerializeCommentOut<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("commentOut", ref mCommentOut, Predicates.IsTrue);
		}
	};
}