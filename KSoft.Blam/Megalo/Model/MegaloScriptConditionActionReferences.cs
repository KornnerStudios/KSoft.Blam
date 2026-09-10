using System.Collections.Generic;

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModelCompilerState
	{
		void Compile(MegaloScriptConditionActionReferences refs)
		{
			short first_cond = (short)ConditionWriteOrder.Count;
			short first_action = (short)ActionWriteOrder.Count;
			short cond_count = 0, action_count = 0;

			int prev_union_group = TypeExtensions.kNone, local_union_group_count = 0;

			foreach (var handle in refs)
			{
				switch (handle.Type)
				{
					case MegaloScriptModelObjectType.Condition:
					{
						var cond = Model.Conditions[handle.Id];
						if (cond.CommentOut)
						{
							cond.ExecuteBeforeAction = -1;
							break;
						}

						cond_count++;
						ConditionWriteOrder.Add(handle.Id);

						// We've encountered a new union group, create a new remapping entry for localizing
						if (prev_union_group != cond.UnionGroup)
						{
							UnionGroupRemappings.Add(cond.UnionGroup, local_union_group_count++);
							prev_union_group = cond.UnionGroup;
						}

						cond.ExecuteBeforeAction = action_count;
					} break;

					case MegaloScriptModelObjectType.Action:
					{
						var action = Model.Actions[handle.Id];
						if (action.CommentOut)
						{
							break;
						}

						action_count++;
						ActionWriteOrder.Add(handle.Id);
					} break;
				}
			}

			refs.Set(first_cond, cond_count, first_action, action_count);
		}
	};
	partial class MegaloScriptModelDecompilerState
	{
		void Decompile(MegaloScriptConditionActionReferences refs)
		{
			refs.Get(
				out short first_cond, out short cond_count,
				out short first_action, out short action_count);

			// predict the amount we're adding to avoid realloc'ing
			if (cond_count > 0 || action_count > 0)
			{
				refs.SetCapacity(cond_count + action_count);
			}

			// first, just add the actions
			for (int x = 0, id = first_action; x < action_count; x++, id++)
			{
				refs.Add(Model.Actions[id].Handle);
			}

			// We do conditions second as to keep the execute-before-action handling
			// simple. Although, insert operations aren't computationally simple...
			MegaloScriptUnionGroup? global_union_group = null;
			for (int prev_union_group_id = TypeExtensions.kNone,
				x = cond_count-1, id = first_cond+x; x >= 0; x--, id--)
			{
				var cond = Model.Conditions[id];

				int insert_index = cond.ExecuteBeforeAction;
				if (insert_index.IsNone())
				{
					throw new System.InvalidOperationException(string.Format(Util.InvariantCultureInfo,
						"Condition {0} has already had its execute-before action processed.", cond.Id));
				}

				refs.Insert(insert_index, cond.Handle);
				cond.ExecuteBeforeAction = TypeExtensions.kNone;

				if (prev_union_group_id != cond.UnionGroup)
				{
					prev_union_group_id = cond.UnionGroup;
					global_union_group = Model.CreateUnionGroup();
				}
				global_union_group!.Add(cond);
			}
		}
	};

	partial class MegaloScriptModel
	{
		void ReferencesSwapLogicUnits(MegaloScriptConditionActionReferences refs,
			MegaloScriptModelObjectHandle lhs, MegaloScriptModelObjectHandle rhs)
		{
			if (lhs.IsNone)
			{
				throw new System.ArgumentException("Handle cannot be NONE.", nameof(lhs));
			}
			if (rhs.IsNone)
			{
				throw new System.ArgumentException("Handle cannot be NONE.", nameof(rhs));
			}

			refs.Swap(lhs, rhs);

			if (lhs.Type == MegaloScriptModelObjectType.Condition && rhs.Type == MegaloScriptModelObjectType.Condition)
			{
				var lhs_cond = Conditions[lhs.Id];
				var rhs_cond = Conditions[rhs.Id];

				if (lhs_cond.UnionGroup == rhs_cond.UnionGroup)
				{
					UnionGroupSwapLogicUnits(lhs_cond.UnionGroup, lhs, rhs);
				}
			}
		}
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public sealed partial class MegaloScriptConditionActionReferences
		: IList<MegaloScriptModelObjectHandle>
		, IMegaloScriptModelObjectContainer
	{
		readonly List<MegaloScriptModelObjectHandle> mElements;

		public MegaloScriptConditionActionReferences()
		{
			mElements = new List<MegaloScriptModelObjectHandle>();
		}

		internal void SetCapacity(int capacity)
		{
			mElements.Capacity = capacity;
		}

		public MegaloScriptModelObjectHandle this[int index] {
			get { return mElements[index]; }
			set { NotifyItemChanged(index, mElements[index], mElements[index] = value); }
		}

		internal void Swap(MegaloScriptModelObjectHandle lhs, MegaloScriptModelObjectHandle rhs)
		{
			int lhs_index = mElements.IndexOf(lhs);
			if (lhs_index < 0)
			{
				throw new System.InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Cannot swap; left handle type {0}, id {1} is not in the references collection.",
					lhs.Type, lhs.Id));
			}

			int rhs_index = mElements.IndexOf(rhs);
			if (rhs_index < 0)
			{
				throw new System.InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Cannot swap; right handle type {0}, id {1} is not in the references collection.",
					rhs.Type, rhs.Id));
			}

			mElements[rhs_index] = lhs;
			mElements[lhs_index] = rhs;
			NotifyItemsSwapped(lhs_index, lhs, rhs_index, rhs);
		}

		#region IBitStreamSerializable Members
		short FirstCondition, ConditionCount;
		short FirstAction, ActionCount;

		internal void Set(short firstCondition, short conditionCount, short firstAction, short actionCount)
		{
			FirstCondition = firstCondition; ConditionCount = conditionCount;
			FirstAction = firstAction; ActionCount = actionCount;
		}
		internal void Get(out short firstCondition, out short conditionCount, out short firstAction, out short actionCount)
		{
			firstCondition = FirstCondition; conditionCount = ConditionCount;
			firstAction = FirstAction; actionCount = ActionCount;
		}

		public void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			var db = model.Database;

			db.StreamTriggerCondAndActionRefs(s, ref FirstCondition, ref ConditionCount, ref FirstAction, ref ActionCount);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if ((model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.EmbedObjects) != 0)
			{
				s.StreamElements("E", mElements, model, MegaloScriptModelObjectHandle.SerializeForEmbed);
			}
			else
			{
				s.StreamElements("E", mElements, model, MegaloScriptModelObjectHandle.Serialize);
			}

			if (s.IsReading)
			{
				// auto-create union groups if needed
				if (model.TagElementStreamSerializeFlags.EmbedObjectsWriteSansIds())
				{
					MegaloScriptUnionGroup.PostprocessConditionsForEmbedObjectsWriteSansIds(model, s, mElements);
				}
				else
				{
					MegaloScriptUnionGroup.ReadPostprocessConditions(model, s, mElements);
				}
			}
		}
		#endregion

		#region ICollection<MegaloScriptModelObjectHandle> Members
		public void Add(MegaloScriptModelObjectHandle item)
		{
			mElements.Add(item);
			NotifyItemInserted(Count - 1, item);
		}
		public bool Remove(MegaloScriptModelObjectHandle item)
		{
			int index = mElements.IndexOf(item);
			if (index >= 0)
			{
				RemoveAt(index);
			}

			return index >= 0;
		}
		public void Clear()
		{
			mElements.Clear();
			NotifyItemsInitialized();
		}
		public bool Contains(MegaloScriptModelObjectHandle item)
		{
			return mElements.Contains(item);
		}
		void ICollection<MegaloScriptModelObjectHandle>.CopyTo(MegaloScriptModelObjectHandle[] array, int arrayIndex)
		{
			mElements.CopyTo(array, arrayIndex);
		}

		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgs]
		public int Count											{ get { return mElements.Count; } }
		bool ICollection<MegaloScriptModelObjectHandle>.IsReadOnly	{ get { return false; } }
		#endregion

		#region IList<MegaloScriptModelObjectHandle> Members
		public int IndexOf(MegaloScriptModelObjectHandle item)
		{
			return mElements.IndexOf(item);
		}
		public void Insert(int index, MegaloScriptModelObjectHandle item)
		{
			mElements.Insert(index, item);
			NotifyItemInserted(index, item);
		}
		public void RemoveAt(int index)
		{
			var item = mElements[index];
			mElements.RemoveAt(index);
			NotifyItemRemoved(index, item);
		}
		#endregion

		#region IEnumerable<MegaloScriptModelObjectHandle> Members
		public IEnumerator<MegaloScriptModelObjectHandle> GetEnumerator()
		{
			return mElements.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return mElements.GetEnumerator();
		}
		#endregion
	};
}
