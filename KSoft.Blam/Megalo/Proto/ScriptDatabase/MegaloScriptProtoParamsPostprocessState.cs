using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using TextWriter = System.IO.TextWriter;

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	struct MegaloScriptProtoParamsPostprocessState
	{
		readonly IMegaloScriptProtoAction mAction;
		readonly MegaloScriptProtoCondition mCond;

		/*public*/ MegaloScriptProtoParam[] ParamsBySigId { get; /*private*/ set; }
		readonly TextWriter mErrorWriter;
		string mErrorPrefix;
		bool ParamSigIdsMatchIndex; // Params[x] == Params[sigId] is true for all params

		public MegaloScriptProtoParamsPostprocessState(TextWriter errorWriter, IMegaloScriptProtoAction action) : this()
		{
			mAction = action;
			ParamsBySigId = new MegaloScriptProtoParam[action.Parameters.Count];
			mErrorWriter = errorWriter;
		}
		public MegaloScriptProtoParamsPostprocessState(TextWriter errorWriter, MegaloScriptProtoCondition cond) : this()
		{
			mCond = cond;
			ParamsBySigId = new MegaloScriptProtoParam[cond.Parameters.Count];
			mErrorWriter = errorWriter;
		}

		void WriteError(string format, params object[] args)
		{
			Debug.Trace.MegaloProto.TraceInformation(format, args);

			if (mErrorWriter != null)
			{
				mErrorWriter.Write(mErrorPrefix);
				mErrorWriter.WriteLine(format, args);
			}
		}

		void ParamVisited(MegaloScriptProtoParam param)
		{
			int sig_id = param.SigId;
			if (sig_id >= ParamsBySigId.Length)
				WriteError("Param {0}-{1} has an invalid SigID", sig_id.ToString(), param.Name);
			else if (ParamsBySigId[sig_id] == null)
				ParamsBySigId[sig_id] = param;
			else
				WriteError("SigID {0} already in use by {1}", sig_id.ToString(), ParamsBySigId[sig_id].Name);
		}

		void PostprocessParameters(IEnumerable<MegaloScriptProtoParam> parameters,
			out bool containsVirtualTriggerParam,
			out bool containsObjectTypeParam)
		{
			containsVirtualTriggerParam = containsObjectTypeParam = false;

			ParamSigIdsMatchIndex = true;
			int idx = 0;
			foreach (var param in parameters)
			{
				ParamVisited(param);

				if (param.Type.BaseType == MegaloScriptValueBaseType.VirtualTrigger)
					containsVirtualTriggerParam = true;
				else if (param.Type.BaseType == MegaloScriptValueBaseType.Index &&
					param.Type.IndexTarget == MegaloScriptValueIndexTarget.ObjectType)
					containsObjectTypeParam = true;

				ParamSigIdsMatchIndex &= param.SigId == idx++;
			}
		}

		void PostprocessObjectWithParams(IMegaloScriptProtoObjectWithParams obj, string typeName,
			out bool containsVirtualTriggerParam)
		{
			mErrorPrefix = string.Format("{2} {0}/{1} ", obj.DBID.ToString(), obj.Name, typeName);

			bool contains_object_type_param;
			PostprocessParameters(obj.ParameterList, out containsVirtualTriggerParam,
				out contains_object_type_param);

			for (int x = 0; x < ParamsBySigId.Length; x++)
				if (ParamsBySigId[x] == null)
					WriteError("SigID {0} is undefined", x.ToString());

			if (contains_object_type_param)
				obj.ContainsObjectTypeParameter = contains_object_type_param;

			var obj_with_params = obj as MegaloScriptProtoObjectWithParams;
			if (obj_with_params != null)
			{
				if (!ParamSigIdsMatchIndex)
					obj_with_params.SetParamsBySigId(new List<MegaloScriptProtoParam>(ParamsBySigId));
			}
		}
		public void Postprocess()
		{
			bool contains_virtual_trigger_param;
			if (mAction != null)
			{
				if (mAction is MegaloScriptProtoAction)
					((MegaloScriptProtoAction)mAction).InitializeParameterList();

				PostprocessObjectWithParams(mAction, "Action", out contains_virtual_trigger_param);
				if (contains_virtual_trigger_param && mAction is IMegaloScriptProtoAction)
					((MegaloScriptProtoAction)mAction).ContainsVirtualTriggerParameter = contains_virtual_trigger_param;
			}
			else if (mCond != null)
			{
				PostprocessObjectWithParams(mCond, "Condition", out contains_virtual_trigger_param);
				Contract.Assert(!contains_virtual_trigger_param);
			}
		}
	};
}