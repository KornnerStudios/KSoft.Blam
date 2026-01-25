using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptModel
	{
		public MegaloScriptModelVariableSet GetModelVariableSet(MegaloScriptVariableSet set)
		{
			return set switch
			{
				MegaloScriptVariableSet.Globals => GlobalVariables,
				MegaloScriptVariableSet.Player => PlayerVariables,
				MegaloScriptVariableSet.Object => ObjectVariables,
				MegaloScriptVariableSet.Team => TeamVariables,
				_ => throw new KSoft.Debug.UnreachableException(set.ToString()),
			};
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
		public MegaloScriptModelVariableSet this[MegaloScriptVariableSet set] => GetModelVariableSet(set);

		/// <summary>Validates that the variable index in a given set is valid based on its type</summary>
		/// <param name="type">Type of variable</param>
		/// <param name="set">Set the variable is in</param>
		/// <param name="index">Index of the variable in the set</param>
		/// <returns></returns>
		public bool VarIndexIsValid(MegaloScriptVariableType type, MegaloScriptVariableSet set, int index)
			=> index >= 0 && this[set].VarIndexIsValid(type, index);
	};

	[System.Reflection.Obfuscation(Exclude=false)]
	public class MegaloScriptModelVariableSet
	{
		public MegaloScriptVariableSet SetType { get; private set; }
		public Proto.MegaloScriptProtoVariableSet ProtoData { get; private set; }

		public ObservableCollection<MegaloScriptCustomVariable> Numerics { get; private set; } = new();
		public ObservableCollection<MegaloScriptTimerVariable> Timers { get; private set; } = new();
		public ObservableCollection<MegaloScriptTeamVariable> Teams { get; private set; } = new();
		public ObservableCollection<MegaloScriptPlayerVariable> Players { get; private set; } = new();
		public ObservableCollection<MegaloScriptObjectVariable> Objects { get; private set; } = new();

		public bool IsNotEmpty =>
			Numerics.Count > 0 || Timers.Count > 0 || Teams.Count > 0 ||
			Players.Count > 0 || Objects.Count > 0;

		public MegaloScriptModelVariableSet(Proto.MegaloScriptDatabase db, MegaloScriptVariableSet set)
		{
			SetType = set;
			ProtoData = db.VariableSets[set];
		}

		internal void ValidateVariableListCounts(IO.ICanThrowReadExceptionsWithExtraDetails readExceptionThrower)
		{
			var set = SetType.ToString();

			ProtoData.Traits[MegaloScriptVariableType.Numeric].ValidateListCount(Numerics, set+".Numerics", readExceptionThrower);
			ProtoData.Traits[MegaloScriptVariableType.Timer].ValidateListCount(Timers, set+".Timers", readExceptionThrower);
			ProtoData.Traits[MegaloScriptVariableType.Team].ValidateListCount(Teams, set+".Teams", readExceptionThrower);
			ProtoData.Traits[MegaloScriptVariableType.Player].ValidateListCount(Players, set+".Players", readExceptionThrower);
			ProtoData.Traits[MegaloScriptVariableType.Object].ValidateListCount(Objects, set+".Objects", readExceptionThrower);
		}
		internal bool ValidateVariableCounts()
		{
			return Numerics.Count <= ProtoData.Traits[MegaloScriptVariableType.Numeric].MaxCount &&
				Timers.Count <= ProtoData.Traits[MegaloScriptVariableType.Timer].MaxCount &&
				Teams.Count <= ProtoData.Traits[MegaloScriptVariableType.Team].MaxCount &&
				Players.Count <= ProtoData.Traits[MegaloScriptVariableType.Player].MaxCount &&
				Objects.Count <= ProtoData.Traits[MegaloScriptVariableType.Object].MaxCount;
		}
		internal bool VarIndexIsValid(MegaloScriptVariableType type, int index)
		{
			System.Collections.IList list = type switch
			{
				MegaloScriptVariableType.Numeric => Numerics,
				MegaloScriptVariableType.Timer => Timers,
				MegaloScriptVariableType.Team => Teams,
				MegaloScriptVariableType.Player => Players,
				MegaloScriptVariableType.Object => Objects,
				_ => throw new KSoft.Debug.UnreachableException(type.ToString()),
			};

			return index < list.Count;
		}

		#region Variable Index Name resolving
		int FromVariableIndexName(MegaloScriptVariableType type, string name)
		{
#pragma warning disable IDE0059 // Unnecessary assignment of a value
			int id = TypeExtensionsBlam.IndexOfByPropertyNotFoundResult;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
			id = type switch
			{
				MegaloScriptVariableType.Numeric =>	MegaloScriptModel.FindNameIndex(Numerics, name),
				MegaloScriptVariableType.Timer =>	MegaloScriptModel.FindNameIndex(Timers, name),
				MegaloScriptVariableType.Team =>	MegaloScriptModel.FindNameIndex(Teams, name),
				MegaloScriptVariableType.Player =>	MegaloScriptModel.FindNameIndex(Players, name),
				MegaloScriptVariableType.Object =>	MegaloScriptModel.FindNameIndex(Objects, name),

				_ => throw new KSoft.Debug.UnreachableException(type.ToString()),
			};

			if (id == TypeExtensionsBlam.IndexOfByPropertyNotFoundResult)
			{
				throw new KeyNotFoundException(string.Format(Util.InvariantCultureInfo,
					"Couldn't find {0} {1} variable named {2}",
					SetType, type, name));
			}

			return id;
		}
		string ToVariableIndexName(MegaloScriptVariableType type, int index)
		{
			if (index < 0 || !VarIndexIsValid(type, index))
			{
				throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"#{0} is not a valid {1}.{2} variable index",
					index.ToString(Util.InvariantCultureInfo), SetType.ToString(), type.ToString()));
			}

			return type switch
			{
				MegaloScriptVariableType.Numeric => Numerics[index].CodeName,
				MegaloScriptVariableType.Timer => Timers[index].CodeName,
				MegaloScriptVariableType.Team => Teams[index].CodeName,
				MegaloScriptVariableType.Player => Players[index].CodeName,
				MegaloScriptVariableType.Object => Objects[index].CodeName,
				_ => throw new KSoft.Debug.UnreachableException(type.ToString()),
			};
		}

		static int FromVariableIndexName(MegaloScriptModel model, MegaloScriptVariableSet varSet, MegaloScriptVariableType varType, string name, bool supportNone)
		{
			if (supportNone && name == MegaloScriptModel.kIndexNameNone)
			{
				return TypeExtensions.kNone;
			}

			return model[varSet].FromVariableIndexName(varType, name);
		}
		static string ToVariableIndexName(MegaloScriptModel model, MegaloScriptVariableSet varSet, MegaloScriptVariableType varType, int index, bool supportNone)
		{
			if (index.IsNone())
			{
				if (supportNone)
				{
					return MegaloScriptModel.kIndexNameNone;
				}
				else
				{
					throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
						"Encountered a {0}.{1} variable reference that was NONE, where NONE isn't supported",
						varSet.ToString(), varType.ToString()));
				}
			}

			return model[varSet].ToVariableIndexName(varType, index);
		}
		internal readonly struct IndexNameResolvingContext
		{
			readonly MegaloScriptModel Model;
			readonly MegaloScriptVariableSet VarSet;
			readonly MegaloScriptVariableType VarType;
			readonly bool SupportNone;

			public IndexNameResolvingContext(MegaloScriptModel model, Proto.MegaloScriptValueType valueType, bool supportNone = false)
			{ Model = model; VarSet = valueType.VarSet; VarType = valueType.VarType; SupportNone = supportNone; }
			public IndexNameResolvingContext(MegaloScriptModel model, MegaloScriptVariableSet varSet, MegaloScriptVariableType varType, bool supportNone = false)
			{ Model = model; VarSet = varSet; VarType = varType; SupportNone = supportNone; }

			public static readonly Func<IndexNameResolvingContext, string, int> IdResolver =
				(ctxt, name) => FromVariableIndexName(ctxt.Model, ctxt.VarSet, ctxt.VarType, name, ctxt.SupportNone);
			public static readonly Func<IndexNameResolvingContext, int, string> NameResolver =
				(ctxt, id) => ToVariableIndexName(ctxt.Model, ctxt.VarSet, ctxt.VarType, id, ctxt.SupportNone);
		};
		#endregion

		#region IBitStreamSerializable Members
		public void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			s.StreamElements(Numerics, ProtoData.Traits[MegaloScriptVariableType.Numeric].CountBitLength,	model, _model => _model.NewCustomVariable());
			s.StreamElements(Timers, ProtoData.Traits[MegaloScriptVariableType.Timer].CountBitLength,		model, _model => _model.NewTimerVariable());
			s.StreamElements(Teams, ProtoData.Traits[MegaloScriptVariableType.Team].CountBitLength,			model, _model => _model.NewTeamVariable());
			s.StreamElements(Players, ProtoData.Traits[MegaloScriptVariableType.Player].CountBitLength,		model, _model => _model.NewPlayerVariable());
			s.StreamElements(Objects, ProtoData.Traits[MegaloScriptVariableType.Object].CountBitLength,		model, _model => _model.NewObjectVariable());
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		public virtual void Serialize<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var bm = s.EnterCursorBookmarkOpt("Numeric", Numerics, Predicates.HasItems))
			{
				if (bm.IsNotNull)
				{
					s.StreamableElements("Var", Numerics, model, _model => _model.NewCustomVariable());
				}
			}

			using (var bm = s.EnterCursorBookmarkOpt("Timers", Timers, Predicates.HasItems))
			{
				if (bm.IsNotNull)
				{
					s.StreamableElements("Var", Timers, model, _model => _model.NewTimerVariable());
				}
			}

			using (var bm = s.EnterCursorBookmarkOpt("Teams", Teams, Predicates.HasItems))
			{
				if (bm.IsNotNull)
				{
					s.StreamableElements("Var", Teams, model, _model => _model.NewTeamVariable());
				}
			}

			using (var bm = s.EnterCursorBookmarkOpt("Players", Players, Predicates.HasItems))
			{
				if (bm.IsNotNull)
				{
					s.StreamableElements("Var", Players, model, _model => _model.NewPlayerVariable());
				}
			}

			using (var bm = s.EnterCursorBookmarkOpt("Objects", Objects, Predicates.HasItems))
			{
				if (bm.IsNotNull)
				{
					s.StreamableElements("Var", Objects, model, _model => _model.NewObjectVariable());
				}
			}

			if (s.IsReading)
			{
				Contract.Assert(Numerics.Count <= ProtoData.Traits[MegaloScriptVariableType.Numeric].MaxCount);
				Contract.Assert(Timers.Count <= ProtoData.Traits[MegaloScriptVariableType.Timer].MaxCount);
				Contract.Assert(Teams.Count <= ProtoData.Traits[MegaloScriptVariableType.Team].MaxCount);
				Contract.Assert(Players.Count <= ProtoData.Traits[MegaloScriptVariableType.Player].MaxCount);
				Contract.Assert(Objects.Count <= ProtoData.Traits[MegaloScriptVariableType.Object].MaxCount);
			}
		}
		#endregion
	};
}
