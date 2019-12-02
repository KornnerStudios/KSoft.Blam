using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModel
	{
		/// <summary>Setup a newly created object in its respective list, with an optional existing id</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="obj"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		protected T CreateObjectPostprocess<T>(Collections.ActiveList<T> list, T obj, int id = TypeExtensions.kNone)
			where T : MegaloScriptModelObject
		{
			if (id.IsNone())
			{
				list.Add(obj, out id);
				obj.Id = id;
			}
			else
				list.AddExplicit(obj, id);

			return obj;
		}

		public MegaloScriptModelObject this[MegaloScriptModelObjectHandle handle] { get {
			int id = handle.Id;
			switch (handle.Type)
			{
				case MegaloScriptModelObjectType.None:			return null;
				case MegaloScriptModelObjectType.Value:			return Values[id];
				case MegaloScriptModelObjectType.UnionGroup:	return UnionGroups[id];
				case MegaloScriptModelObjectType.Condition:		return Conditions[id];
				case MegaloScriptModelObjectType.Action:		return Actions[id];
				case MegaloScriptModelObjectType.Trigger:		return Triggers[id];
				case MegaloScriptModelObjectType.VirtualTrigger:return VirtualTriggers[id];

				default: throw new KSoft.Debug.UnreachableException(handle.Type.ToString());
			}
		} }
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, Type = {ObjectType}")]
	public abstract partial class MegaloScriptModelObject
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
	{
		internal static readonly Func<MegaloScriptModelObject, int> kObjectToIndex = obj => obj.Id;

		public MegaloScriptModelObjectHandle Handle { get {
			Contract.Requires(Id.IsNotNone());
			return new MegaloScriptModelObjectHandle(ObjectType, Id);
		} }

		public abstract MegaloScriptModelObjectType ObjectType { get; }
		#region ID
		int mId;
		public int Id {
			get { return mId; }
			internal set {
				Contract.Assert(mId.IsNone(), "ID should be considered immutable after it is first set");
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
				s.StreamAttribute(kIdAttributeName, ref mId);
			else if (s.IsReading)
				Contract.Assert(Id.IsNotNone(), // ID should have been set prior to serialize (eg, in the object's Create method in the Model)
					"Tried to read an embedded object (sans ID) which wasn't given an ID already");
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