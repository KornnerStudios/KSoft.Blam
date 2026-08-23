using System.Collections.Generic;

using TextWriter = System.IO.TextWriter;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	struct MegaloScriptProtoParamsPostprocessState
	{
		readonly IMegaloScriptProtoAction mAction = null!;
		readonly MegaloScriptProtoCondition mCond = null!;

		/*public*/ MegaloScriptProtoParam[] ParamsBySigId { get; /*private*/ set; }
		readonly TextWriter? mErrorWriter;
		string mErrorPrefix = null!;
		bool ParamSigIdsMatchIndex; // Params[x] == Params[sigId] is true for all params

		public MegaloScriptProtoParamsPostprocessState(TextWriter? errorWriter, IMegaloScriptProtoAction action) : this()
		{
			mAction = action;
			ParamsBySigId = new MegaloScriptProtoParam[action.Parameters.Count];
			mErrorWriter = errorWriter;
		}
		public MegaloScriptProtoParamsPostprocessState(TextWriter? errorWriter, MegaloScriptProtoCondition cond) : this()
		{
			mCond = cond;
			ParamsBySigId = new MegaloScriptProtoParam[cond.Parameters.Count];
			mErrorWriter = errorWriter;
		}

		readonly void WriteError(string format, params object[] args)
		{
			Debug.Trace.MegaloProto.TraceInformation(format, args);

			if (mErrorWriter != null)
			{
				mErrorWriter.Write(mErrorPrefix);
				mErrorWriter.WriteLine(format, args);
			}
		}

		readonly void ParamVisited(MegaloScriptProtoParam param)
		{
			int sig_id = param.SigId;
			if (sig_id >= ParamsBySigId.Length)
			{
				WriteError("Param {0}-{1} has an invalid SigID", sig_id.ToString(Util.InvariantCultureInfo), param.Name);
			}
			else if (ParamsBySigId[sig_id] == null)
			{
				ParamsBySigId[sig_id] = param;
			}
			else
			{
				WriteError("SigID {0} already in use by {1}", sig_id.ToString(Util.InvariantCultureInfo), ParamsBySigId[sig_id].Name);
			}
		}

		void PostprocessParameters(IEnumerable<MegaloScriptProtoParam> parameters,
			out bool containsVirtualTriggerParam,
			out bool containsObjectTypeParam)
		{
			containsVirtualTriggerParam = containsObjectTypeParam = false;

			ParamSigIdsMatchIndex = true;
			int idx = 0;
			foreach (MegaloScriptProtoParam param in parameters)
			{
				ParamVisited(param);

				if (param.Type.BaseType == MegaloScriptValueBaseType.VirtualTrigger)
				{
					containsVirtualTriggerParam = true;
				}
				else if (param.Type.BaseType == MegaloScriptValueBaseType.Index &&
					param.Type.IndexTarget == MegaloScriptValueIndexTarget.ObjectType)
				{
					containsObjectTypeParam = true;
				}

				ParamSigIdsMatchIndex &= param.SigId == idx++;
			}
		}

		void PostprocessObjectWithParams(IMegaloScriptProtoObjectWithParams obj, string typeName,
			out bool containsVirtualTriggerParam)
		{
			mErrorPrefix = string.Format(Util.InvariantCultureInfo,
				"{2} {0}/{1} ", obj.DBID.ToString(Util.InvariantCultureInfo), obj.Name, typeName);

			PostprocessParameters(obj.ParameterList, out containsVirtualTriggerParam,
				out bool contains_object_type_param);

			for (int x = 0; x < ParamsBySigId.Length; x++)
			{
				if (ParamsBySigId[x] == null)
				{
					WriteError("SigID {0} is undefined", x.ToString(Util.InvariantCultureInfo));
				}
			}

			if (contains_object_type_param)
			{
				obj.ContainsObjectTypeParameter = contains_object_type_param;
			}

			if (obj is MegaloScriptProtoObjectWithParams obj_with_params)
			{
				if (!ParamSigIdsMatchIndex)
				{
					obj_with_params.SetParamsBySigId(new List<MegaloScriptProtoParam>(ParamsBySigId));
				}
			}
		}
		public void Postprocess()
		{
			bool contains_virtual_trigger_param;
			if (mAction != null)
			{
				MegaloScriptProtoAction? protoAction = mAction as MegaloScriptProtoAction;
				if (protoAction != null)
				{
					protoAction.InitializeParameterList();
				}

				PostprocessObjectWithParams(mAction, "Action", out contains_virtual_trigger_param);
				if (contains_virtual_trigger_param && protoAction != null)
				{
					protoAction.ContainsVirtualTriggerParameter = contains_virtual_trigger_param;
				}
			}
			else if (mCond != null)
			{
				PostprocessObjectWithParams(mCond, "Condition", out contains_virtual_trigger_param);
				if (contains_virtual_trigger_param)
				{
					throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
						"Condition {0}/{1} contains a virtual trigger parameter.",
						mCond.DBID.ToString(Util.InvariantCultureInfo),
						mCond.Name));
				}
			}
		}
	};
}
