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
		public MegaloScriptModelTagElementStreamFlags TagElementStreamSerializeFlags;

		internal void SerializeOperationId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, ref int type,
			MegaloScriptModelObjectType objType)
			where TDoc : class
			where TCursor : class
		{
			if (objType == MegaloScriptModelObjectType.Condition && TagElementStreamSerializeFlags.UseConditionTypeNames())
			{
				string name = s.IsReading ? null : Database.Conditions[type].Name;
				s.StreamAttribute("name", ref name);
				if (s.IsReading)
				{
					Proto.MegaloScriptProtoCondition proto;
					if (Database.TryGetCondition(name, out proto))
						type = proto.DBID;
					else
						throw new KeyNotFoundException("Not a valid condition name: " + name);
				}
			}
			else if (objType == MegaloScriptModelObjectType.Action && TagElementStreamSerializeFlags.UseActionTypeNames())
			{
				string name = s.IsReading ? null : Database.Actions[type].Name;
				s.StreamAttribute("name", ref name);
				if (s.IsReading)
				{
					Proto.MegaloScriptProtoAction proto;
					if (Database.TryGetAction(name, out proto))
						type = proto.DBID;
					else
						throw new KeyNotFoundException("Not a valid action name: " + name);
				}
			}
			else
				s.StreamAttribute("DBID", ref type);
		}

		static Collections.ActiveListUtil.TagElementStreamReadMode GetTriggersTagElementStreamReadMode(MegaloScriptModel @this)
		{
			return @this.TagElementStreamSerializeFlags.EmbedObjectsWriteSansIds() ?
				Collections.ActiveListUtil.TagElementStreamReadMode.PostAdd :
				Collections.ActiveListUtil.TagElementStreamReadMode.PostConstructor;
		}

		static int SerializeObjectTypeReference<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BitSet bitset, int bitIndex, MegaloScriptModel model)
			where TDoc : class
			where TCursor : class
		{
			if (s.IsReading)
			{
				string object_name = null;
				s.ReadCursor(ref object_name);
				bitIndex = model.FromIndexName(Proto.MegaloScriptValueIndexTarget.ObjectType, object_name);
			}
			else if(s.IsWriting)
			{
				string object_name = model.ToIndexName(Proto.MegaloScriptValueIndexTarget.ObjectType, bitIndex);
				s.WriteCursor(object_name);
			}

			return bitIndex;
		}

		static void SerializeTriggerExecutionOrder<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			MegaloScriptModel model, ref int triggerIndex)
			where TDoc : class
			where TCursor : class
		{
			if (model.TagElementStreamSerializeFlags.EmbedObjects())
			{
				var id_resolving_ctxt = new TriggerIndexNameResolvingContext(model);
				var id_resolver = TriggerIndexNameResolvingContext.IdResolver;
				var name_resolver = TriggerIndexNameResolvingContext.NameResolver;

				s.StreamCursorIdAsString(ref triggerIndex, id_resolving_ctxt, id_resolver, name_resolver);
			}
			else
			{
				s.StreamCursor(ref triggerIndex);

				if (model.Triggers.SlotIsFreeOrInvalidIndex(triggerIndex))
					throw new System.IO.InvalidDataException(string.Format(
						"Couldn't define execution order for invalid trigger index {0}", triggerIndex));
			}

			if(!model.Triggers[triggerIndex].TriggerType.IsUpdatedOnGameTick())
				throw new System.IO.InvalidDataException(string.Format(
						"Trigger '{0}' can't have its execution explicitly ordered",
						triggerIndex.ToString()
						));
		}

		protected void SerializeGameObjectFilters<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("GameObjectFilters", CandySpawnerFilters, Predicates.HasItems)) if (bm.IsNotNull)
				s.StreamableElements("Filter", CandySpawnerFilters);
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("SerializeFlags"))
				s.StreamCursorEnum(ref TagElementStreamSerializeFlags, true);

			#region TryToPort sanity checks
			if ((TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.TryToPort) != 0)
				if (s.IsWriting)
					Contract.Assert(Proto.MegaloScriptDatabase.HaloReach != null && Proto.MegaloScriptDatabase.Halo4 != null,
						"Can't port, other game's DB isn't loaded");
				else if(s.IsReading)
					throw new InvalidOperationException("Can't load variants saved with TryToPort");
			#endregion

			bool embed_model_objects = TagElementStreamSerializeFlags.EmbedObjects();
			bool write_sans_ids = TagElementStreamSerializeFlags.EmbedObjectsWriteSansIds();

			#region Version sanity checks
			if ((TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.IgnoreVersionIds) == 0)
			{
				s.StreamVersionViaAttribute("version", kVersion, "MegaFloW Script");
				s.StreamVersionViaAttribute("dbVersion", Database.Version, "Database");
			}
			#endregion

			using (s.EnterOwnerBookmark(this))
			{
				#region helpful info dump
				if (s.IsWriting && embed_model_objects && !write_sans_ids) using (s.EnterCursorBookmark("IDs"))
				{
					s.WriteAttribute("NextCondID", Conditions.FirstInactiveIndex);
					s.WriteAttribute("NextActionID", Actions.FirstInactiveIndex);
					s.WriteAttribute("NextValueID", Values.FirstInactiveIndex);
					s.WriteAttribute("NextTrigID", Triggers.FirstInactiveIndex);
					if(Database.Limits.SupportsVirtualTriggers)
						s.WriteAttribute("NextVirtualTriggerID", VirtualTriggers.FirstInactiveIndex);
				}
				#endregion

				#region ObjectTypeReferences
				using (var bm = s.EnterCursorBookmarkOpt("ObjectTypeReferences", ObjectTypeReferences, Predicates.HasBits)) if (bm.IsNotNull)
					ObjectTypeReferences.Serialize(s, "Ref", this, SerializeObjectTypeReference,
						Database.StaticDatabase.ObjectTypeList.Types.Count - 1);
				#endregion

				#region GameStatistics
				using (var bm = s.EnterCursorBookmarkOpt("Statistics", GameStatistics, Predicates.HasItems)) if (bm.IsNotNull)
					s.StreamableElements("Stat", GameStatistics, this, _this => _this.NewGameStatistic());
				#endregion

				#region HudWidgets
				using (var bm = s.EnterCursorBookmarkOpt("HudWidgets", HudWidgets, Predicates.HasItems)) if (bm.IsNotNull)
					s.StreamableElements("Widget", HudWidgets);
				#endregion

				#region Object Filters
				using (var bm = s.EnterCursorBookmarkOpt("ObjectFilters", ObjectFilters, Predicates.HasItems)) if (bm.IsNotNull)
					s.StreamableElements("Filter", ObjectFilters);

				if (Database.Limits.SupportsGameObjectFilters)
					SerializeGameObjectFilters(s);
				#endregion

				#region Variables
				Predicate<MegaloScriptModelVariableSet> set_is_not_empty = v => v.IsNotEmpty;

				using (var bm = s.EnterCursorBookmarkOpt("GlobalVariables", GlobalVariables, set_is_not_empty)) if (bm.IsNotNull)
					GlobalVariables.Serialize(this, s);
				using (var bm = s.EnterCursorBookmarkOpt("PlayerVariables", PlayerVariables, set_is_not_empty)) if (bm.IsNotNull)
					PlayerVariables.Serialize(this, s);
				using (var bm = s.EnterCursorBookmarkOpt("ObjectVariables", ObjectVariables, set_is_not_empty)) if (bm.IsNotNull)
					ObjectVariables.Serialize(this, s);
				using (var bm = s.EnterCursorBookmarkOpt("TeamVariables", TeamVariables, set_is_not_empty)) if (bm.IsNotNull)
					TeamVariables.Serialize(this, s);
				#endregion

				#region UnionGroups
				if (!TagElementStreamSerializeFlags.EmbedObjectsWriteSansIds())
					using (s.EnterCursorBookmark("UnionGroups"))	Collections.ActiveListUtil.
						Serialize(s, "Group", UnionGroups,			this, NewUnionGroupFromTagStream);
				#endregion

				#region Values or Global Values
				// #NOTE_BLAM: A Values element will be created even if there are no global values
				using (s.EnterCursorBookmark("Values"))				Collections.ActiveListUtil.
					Serialize(s, "Value", Values,					this, MegaloScriptValueBase.NewFromTagStream,
						embed_model_objects ? MegaloScriptValueBase.SkipIfNotGlobalPredicate : null);
				#endregion

				#region Conditions / Actions / Virtual Triggers (non-embedded)
				if (!embed_model_objects)
				{
					using (s.EnterCursorBookmark("Conditions"))		Collections.ActiveListUtil.
						Serialize(s, "Condition", Conditions,		this, NewConditionFromTagStream);
					using (s.EnterCursorBookmark("Actions"))		Collections.ActiveListUtil.
						Serialize(s, "Action", Actions,				this, NewActionFromTagStream);
					using (s.EnterCursorBookmark("VirtualTriggers"))Collections.ActiveListUtil.
						Serialize(s, "Trigger", VirtualTriggers,	this, NewVirtualTriggerFromTagStream);
				}
				#endregion

				using (s.EnterCursorBookmark("Triggers"))			Collections.ActiveListUtil.
					Serialize(s, "Trigger", Triggers,				this, NewTriggerFromTagStream,
						embed_model_objects ? MegaloScriptTrigger.SkipIfNotRootPredicate : null,
						GetTriggersTagElementStreamReadMode);

				#region Trigger Indexes
				using (s.EnterCursorBookmark("EntryPoints"))
				{
					var id_resolving_ctxt = new TriggerIndexNameResolvingContext(this);
					var id_resolver = TriggerIndexNameResolvingContext.IdResolver;
					var name_resolver = TriggerIndexNameResolvingContext.NameResolver;

					s.StreamElementOptIdAsString("Initialization", ref mInitializationTriggerIndex,				id_resolving_ctxt, id_resolver, name_resolver, Predicates.IsNotNull);
					s.StreamElementOptIdAsString("LocalInitialization", ref mLocalInitializationTriggerIndex,	id_resolving_ctxt, id_resolver, name_resolver, Predicates.IsNotNull);
					s.StreamElementOptIdAsString("HostMigration", ref mHostMigrationTriggerIndex,				id_resolving_ctxt, id_resolver, name_resolver, Predicates.IsNotNull);
					s.StreamElementOptIdAsString("DoubleHostMigration", ref mDoubleHostMigrationTriggerIndex,	id_resolving_ctxt, id_resolver, name_resolver, Predicates.IsNotNull);
					s.StreamElementOptIdAsString("ObjectDeathEvent", ref mObjectDeathEventTriggerIndex,			id_resolving_ctxt, id_resolver, name_resolver, Predicates.IsNotNull);
					s.StreamElementOptIdAsString("Local", ref mLocalTriggerIndex,								id_resolving_ctxt, id_resolver, name_resolver, Predicates.IsNotNull);
					s.StreamElementOptIdAsString("Pregame", ref mPregameTriggerIndex,							id_resolving_ctxt, id_resolver, name_resolver, Predicates.IsNotNull);
					s.StreamElementOptIdAsString("Incident", ref mIncidentTriggerIndex,							id_resolving_ctxt, id_resolver, name_resolver, Predicates.IsNotNull);
				}
				#endregion

				using (var bm = s.EnterCursorBookmarkOpt("ExecutionOrder", TriggerExecutionOrder, Predicates.HasItems)) if (bm.IsNotNull)
					s.StreamElements("Trigger", TriggerExecutionOrder, this, SerializeTriggerExecutionOrder);
			}

			#region List count sanity checks
			if (s.IsReading)
			{
				// #TODO_IMPLEMENT: sanity check list counts
				var limits = Database.Limits;

				limits.GameStatistics.ValidateListCount(GameStatistics, "GameStatistics");
				GlobalVariables.ValidateVariableListCounts();
				PlayerVariables.ValidateVariableListCounts();
				ObjectVariables.ValidateVariableListCounts();
				TeamVariables.ValidateVariableListCounts();
				limits.HudWidgets.ValidateListCount(HudWidgets, "HudWidgets");
				limits.ObjectFilters.ValidateListCount(ObjectFilters, "ObjectFilters");
				limits.GameObjectFilters.ValidateListCount(CandySpawnerFilters, "GameObjectFilters");
			}
			#endregion
		}
	};
}