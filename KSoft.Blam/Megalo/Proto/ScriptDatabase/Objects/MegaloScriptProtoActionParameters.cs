using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public sealed class MegaloScriptProtoActionParameters
		: IO.ITagElementStringNameStreamable
		, IReadOnlyList<MegaloScriptProtoParam>
	{
		readonly IMegaloScriptProtoAction mAction;

		MegaloScriptProtoActionParameters mBase;
		readonly List<MegaloScriptProtoParam> mParams;
		Dictionary<int, string> mNameOverrides;

		public int Count { get; private set; }
		public bool HasNameOverrides { get { return mNameOverrides != null; } }
		public IReadOnlyDictionary<int, string> NameOverrides { get {
			Contract.Requires(HasNameOverrides, "It's an invalid operation to access the name overrides when there are none");
			return mNameOverrides;
		} }

		public bool ContainsObjectTypeParameter { get; set; }

		public MegaloScriptProtoActionParameters(IMegaloScriptProtoAction action)
		{
			mAction = action;

			mParams = new List<MegaloScriptProtoParam>();
		}

		internal void SetBase(MegaloScriptProtoActionParameters baseParams)
		{
			Contract.Requires(baseParams != null);
			Contract.Assert(mParams.Count == 0, "Base shouldn't be set once params are loaded!");

			mBase = baseParams;
			Count = mBase.Count;
		}
		// RESURSIVE!
		internal void BuildOrderedParamsList(List<MegaloScriptProtoParam> list)
		{
			if (mBase != null)
				mBase.BuildOrderedParamsList(list);

			list.AddRange(mParams);
		}

		public MegaloScriptProtoParam this[int index] { get {
			if (index >= Count) return null;

			MegaloScriptProtoParam param = null;

			if (mBase != null)
			{
				param = mBase[index];

				// If the parameter isn't in the base, adjust the index
				// to be relative to 'this'
				if (param == null)
					index -= mBase.Count;
			}

			if (param == null)
				param = mParams[index];

			return param;
		} }

		public string GetParameterNameOverrideBySigId(int sigId)
		{
			if (sigId >= Count) return null;

			string name = null;
			if (!HasNameOverrides || mNameOverrides.TryGetValue(sigId, out name))
				name = mBase.GetParameterNameOverrideBySigId(sigId);

			return name;
		}

		#region ITagElementStringNameStreamable Members
		const string kNameOverridesRootName = "ParamNameOverrides";
		const string kNameOverrideElementName = "Param";

		void NameOverridesToStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			foreach (var kv in mNameOverrides)
				using (s.EnterCursorBookmark(kNameOverrideElementName))
					MegaloScriptProtoParam.SigIdNamePairToStream(s, kv);
		}
		void NameOverridesFromStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			mNameOverrides = new Dictionary<int, string>();

			foreach (var node in s.ElementsByName(kNameOverrideElementName))
				using (s.EnterCursorBookmark(node))
				{
					var pair = MegaloScriptProtoParam.SigIdNamePairFromStream(s);
					mNameOverrides.Add(pair.Key, pair.Value);
				}
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			if (reading && mAction.Template != null)
				SetBase(mAction.Template.Parameters);

			s.StreamableElements("Param", mParams);

			using (var bm = s.EnterCursorBookmarkOpt(kNameOverridesRootName, this, obj=>!obj.HasNameOverrides)) if(bm.IsNotNull)
			{
					 if (s.IsReading) NameOverridesFromStream(s);
				else if (s.IsWriting) NameOverridesToStream(s);
			}

			if (reading) Count += mParams.Count;
		}
		#endregion

		#region IEnumerable<MegaloScriptProtoParam> Members
		public IEnumerator<MegaloScriptProtoParam> GetEnumerator()
		{
			return mParams.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return (mParams as System.Collections.IEnumerable).GetEnumerator();
		}
		#endregion
	};
}