using System;
using System.Collections.Generic;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.Localization
{
	/// <summary>Interface for language related services for an engine and its builds</summary>
	[Engine.EngineSystem(KeepExternsLoaded=true)]
	public sealed class LanguageSystem
		: Engine.EngineSystemBase
	{
		#region SystemGuid
		static readonly Values.KGuid kSystemGuid = new Values.KGuid("EF39D343-DAD5-43D4-A215-F91722ED1CC5");
		public static Values.KGuid SystemGuid { get { return kSystemGuid; } }
		#endregion

		// As it stands, all engines only need on table. All of their branches and pre-ship builds don't use different lang sets
		GameLanguageTable mEngineTable;

		internal LanguageSystem()
		{
			mEngineTable = new GameLanguageTable();
		}

		public GameLanguageTable GetLanguageTable(Engine.EngineBuildHandle forBuild)
		{
			Contract.Requires<ArgumentNullException>(!forBuild.IsNone);
			Contract.Assert(forBuild.EngineIndex == mEngineTable.BuildHandle.EngineIndex);

			return mEngineTable;
		}

		#region ITagElementStreamable<string> Members
		protected override void SerializeExternBody<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			const string kElementNameLanguageTable = "T";

			using (s.EnterCursorBookmark("LanguageTables"))
			{
				using (s.EnterCursorBookmark(kElementNameLanguageTable))
				{
					mEngineTable.Serialize(s);
				}

				if (s.IsReading)
				{
					Contract.Assert(s.ElementsByName(kElementNameLanguageTable).Count() == 1,
						"Engine has multiple tables defined! This is unexpected, backend code needs to be rewritten");
				}
			}
		}
		#endregion
	};
}