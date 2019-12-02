using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModel
	{
		protected MegaloScriptUnionGroup NewUnionGroup()
		{
			return new MegaloScriptUnionGroup();
		}

		internal MegaloScriptUnionGroup CreateUnionGroup(int id = TypeExtensions.kNone)
		{
			return CreateObjectPostprocess(UnionGroups, NewUnionGroup(), id);
		}

		void UnionGroupSwapLogicUnits(int unionGroupId,
			MegaloScriptModelObjectHandle lhs, MegaloScriptModelObjectHandle rhs)
		{
			var union_group = UnionGroups[unionGroupId];

			union_group.Swap(lhs, rhs);
		}

		internal void HandleRemoval(MegaloScriptUnionGroup unionGroup)
		{
			UnionGroups[unionGroup.Id] = null;
		}

		#region NewFrom* Utils
		static MegaloScriptUnionGroup NewUnionGroupFromTagStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, MegaloScriptModel model)
			where TDoc : class
			where TCursor : class
		{ return model.NewUnionGroup(); }
		#endregion
	};

	/// <summary>Represents a group of OR'd conditions</summary>
	/// <remarks>
	/// Actual evaluation order is determined by the trigger's elements order. Order is only preserved here to keep
	/// any lookups lightweight
	/// </remarks>
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed partial class MegaloScriptUnionGroup
		: MegaloScriptModelObject
		, IList<MegaloScriptModelObjectHandle>
		, IMegaloScriptModelObjectContainer
	{
		public override MegaloScriptModelObjectType ObjectType { get { return MegaloScriptModelObjectType.UnionGroup; } }

		readonly List<MegaloScriptModelObjectHandle> mConditions;

		public MegaloScriptUnionGroup()
		{
			mConditions = new List<MegaloScriptModelObjectHandle>();
		}

		internal void Add(MegaloScriptCondition cond)
		{
			cond.AssociateWith(this);
			mConditions.Add(cond.Handle);
		}
		internal void Insert(MegaloScriptCondition cond, MegaloScriptModelObjectHandle evaluateBefore)
		{
			cond.AssociateWith(this);
			if (evaluateBefore.IsNone)
				mConditions.Add(cond.Handle);
			else
			{
				int insert_index = mConditions.IndexOf(evaluateBefore);
				Contract.Assert(insert_index >= 0);
				mConditions.Insert(insert_index, cond.Handle);
			}
		}
		internal void Swap(MegaloScriptModelObjectHandle lhs, MegaloScriptModelObjectHandle rhs)
		{
			int lhs_index = mConditions.IndexOf(lhs);
			Contract.Assert(lhs_index >= 0);
			int rhs_index = mConditions.IndexOf(rhs);
			Contract.Assert(rhs_index >= 0);

			mConditions[rhs_index] = lhs;
			mConditions[lhs_index] = rhs;
			NotifyItemsSwapped(lhs_index, lhs, rhs_index, rhs);
		}
		internal bool Remove(MegaloScriptModel model, MegaloScriptModelObjectHandle conditionHandle)
		{
			int index = mConditions.IndexOf(conditionHandle);
			if (index >= 0)
			{
				RemoveAt(index);

				if (mConditions.Count == 0)
					model.HandleRemoval(this);
			}

			return index >= 0;
		}

		public MegaloScriptModelObjectHandle this[int index] {
			get { return mConditions[index]; }
			set { NotifyItemChanged(index, mConditions[index], mConditions[index] = value); }
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			throw new NotImplementedException("This should never be called");
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		internal static void PostprocessConditionsForEmbedObjectsWriteSansIds<TDoc, TCursor>(
			MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s,
			IEnumerable<MegaloScriptModelObjectHandle> elements)
			where TDoc : class
			where TCursor : class
		{
			MegaloScriptUnionGroup prev_union_group = null;
			foreach (var obj in elements)
			{
				if (obj.Type != MegaloScriptModelObjectType.Condition) continue;
				var cond = model.Conditions[obj.Id];

				if (cond.UnionGroup >= 0) // id is already valid
				{
					prev_union_group = model.UnionGroups[cond.UnionGroup]; // mark it as prev in case it was inserted into existing code
					continue;
				}
				else if (cond.UnionGroup == MegaloScriptCondition.kUseNewUnionGroupId)
				{
					prev_union_group = model.CreateUnionGroup();
					prev_union_group.Add(cond);
				}
				else if (cond.UnionGroup == MegaloScriptCondition.kUsePrevUnionGroupId)
				{
					Contract.Assert(prev_union_group != null);
					prev_union_group.Add(cond);
				}
				else
					throw new KSoft.Debug.UnreachableException(cond.UnionGroup.ToString());
			}
		}
		internal static void ReadPostprocessConditions<TDoc, TCursor>(
			MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s,
			IEnumerable<MegaloScriptModelObjectHandle> elements)
			where TDoc : class
			where TCursor : class
		{
			foreach (var obj in elements)
			{
				if (obj.Type != MegaloScriptModelObjectType.Condition) continue;
				var cond = model.Conditions[obj.Id];

				var union_group = model.UnionGroups[cond.UnionGroup];
				union_group.Add(cond);
			}
		}

		public override void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(model, s);

			if (s.IsWriting)
			{
				//s.WriteAttribute("condCount", mConditions.Count);
				Contract.Assert(Count > 0, "Found a non-disposed union group!");
			}
		}
		#endregion

		#region ICollection<MegaloScriptModelObjectHandle> Members
		void ICollection<MegaloScriptModelObjectHandle>.Add(MegaloScriptModelObjectHandle item)
		{ throw new NotImplementedException(); }
		bool ICollection<MegaloScriptModelObjectHandle>.Remove(MegaloScriptModelObjectHandle item)
		{ throw new NotImplementedException(); }
		void ICollection<MegaloScriptModelObjectHandle>.Clear()
		{ throw new NotImplementedException(); }

		public bool Contains(MegaloScriptModelObjectHandle item)
		{
			return mConditions.Contains(item);
		}
		void ICollection<MegaloScriptModelObjectHandle>.CopyTo(MegaloScriptModelObjectHandle[] array, int arrayIndex)
		{
			mConditions.CopyTo(array, arrayIndex);
		}

		public int Count											{ get { return mConditions.Count; } }
		bool ICollection<MegaloScriptModelObjectHandle>.IsReadOnly	{ get { return true; } }
		#endregion

		#region IList<MegaloScriptModelObjectHandle> Members
		public int IndexOf(MegaloScriptModelObjectHandle item)
		{
			return mConditions.IndexOf(item);
		}
		public void Insert(int index, MegaloScriptModelObjectHandle item)
		{
			mConditions.Insert(index, item);
			NotifyItemInserted(index, item);
		}
		public void RemoveAt(int index)
		{
			var item = mConditions[index];
			mConditions.RemoveAt(index);
			NotifyItemRemoved(index, item);
		}
		#endregion

		#region IEnumerable<MegaloScriptModelObjectHandle> Members
		public IEnumerator<MegaloScriptModelObjectHandle> GetEnumerator()
		{
			return mConditions.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return mConditions.GetEnumerator();
		}
		#endregion
	};
}