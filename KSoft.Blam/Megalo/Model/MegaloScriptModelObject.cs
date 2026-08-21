using System;

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModel
	{
		/// <summary>Setup a newly created object in its respective list, with an optional existing id</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="theObject"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		protected static T CreateObjectPostprocess<T>(Collections.ActiveList<T> list, T theObject, int id = TypeExtensions.kNone)
			where T : MegaloScriptModelObject
		{
			if (id.IsNone())
			{
				list.Add(theObject, out id);
				theObject.Id = id;
			}
			else
			{
				list.AddExplicit(theObject, id);
			}

			return theObject;
		}

		public MegaloScriptModelObject GetModelObjectFromHandle(MegaloScriptModelObjectHandle handle)
		{
			int id = handle.Id;
			return handle.Type switch
			{
				MegaloScriptModelObjectType.None =>				null,
				MegaloScriptModelObjectType.Value =>			Values[id],
				MegaloScriptModelObjectType.UnionGroup =>		UnionGroups[id],
				MegaloScriptModelObjectType.Condition =>		Conditions[id],
				MegaloScriptModelObjectType.Action =>			Actions[id],
				MegaloScriptModelObjectType.Trigger =>			Triggers[id],
				MegaloScriptModelObjectType.VirtualTrigger =>	VirtualTriggers[id],
				_ => throw new KSoft.Debug.UnreachableException(handle.Type.ToString()),
			};
		}

		public MegaloScriptModelObject this[MegaloScriptModelObjectHandle handle] => GetModelObjectFromHandle(handle);
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Type = {ObjectType}")]
	public abstract partial class MegaloScriptModelObject
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		internal static readonly Func<MegaloScriptModelObject, int> kObjectToIndex = obj => obj.Id;

		public MegaloScriptModelObjectHandle Handle { get {
			if (Id.IsNone())
			{
				throw new InvalidOperationException("Cannot create a handle for a model object without an ID.");
			}

			return new MegaloScriptModelObjectHandle(ObjectType, Id);
		} }

		public abstract MegaloScriptModelObjectType ObjectType { get; }
		#region ID
		int mId;
		public int Id {
			get { return mId; }
			internal set {
				if (mId.IsNotNone())
				{
					throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
						"Model object ID is immutable once set; current ID is {0}, requested ID is {1}.",
						mId, value));
				}

				mId = value;
		} }
		#endregion

		protected MegaloScriptModelObject()
		{
			mId = TypeExtensions.kNone;
		}

		#region IBitStreamSerializable Members
		public abstract void Serialize(MegaloScriptModel model, IO.BitStream s);

		void IO.IBitStreamSerializable.Serialize(IO.BitStream s)
		{
			Serialize((MegaloScriptModel)s.Owner, s);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		#region SerializeId
		internal const string kIdAttributeName = "ID";

		protected static void SerializeId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, ref int id)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute(kIdAttributeName, ref id);
		}
		protected void SerializeId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute(kIdAttributeName, ref mId);
		}
		protected void SerializeIdOpt<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (!model.TagElementStreamSerializeFlags.EmbedObjectsWriteSansIds())
			{
				s.StreamAttribute(kIdAttributeName, ref mId);
			}
			else if (s.IsReading)
			{
				if (Id.IsNone())
				{
					s.ThrowReadException(new System.IO.InvalidDataException(
						"Tried to read an embedded object without an ID before one was assigned."));
				}
			}
		}
		#endregion

		public virtual void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			SerializeId(s);
		}

		void IO.ITagElementStreamable<string>.Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			Serialize((MegaloScriptModel)s.Owner, s);
		}
		#endregion
	};
}
