using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Engine
{
	partial class BlamEngine
	{
		static void StreamSystemPrototypeKey<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			BlamEngine engine,
			ref Values.KGuid systemGuid)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("guid", ref systemGuid);

			if (s.IsReading)
			{
				string invalid_guid_msg = null;

				if (systemGuid == Values.KGuid.Empty)
					invalid_guid_msg = "Invalid system guid: ";
				else if (EngineRegistry.TryGetRegisteredSystem(systemGuid) == null)
					invalid_guid_msg = "Unknown system guid: ";

				if (invalid_guid_msg != null)
				{
					s.ThrowReadException(new System.IO.InvalidDataException(invalid_guid_msg + systemGuid.ToString(Values.KGuid.kFormatHyphenated)));
				}
			}
		}
/*		static void StreamDeclaredSystemsValue<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			BlamEngine engine,
			ref string externsFile)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("externs", ref externsFile, Predicates.IsNotNullOrEmpty);
		}*/

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			Contract.Assert(s.Owner == null);
			s.Owner = this;

			using (s.EnterUserDataBookmark(this))
			{
				BuildRepository.Serialize(s);

				using (var bm = s.EnterCursorBookmarkOpt("Systems", mSystemPrototypes, Predicates.HasItems)) if (bm.IsNotNull)
					s.StreamableElements("System",
						mSystemPrototypes, this, StreamSystemPrototypeKey);
			}

/*			using (var bm = s.EnterCursorBookmarkOpt("Systems", mDeclaredSystems, Predicates.HasItems)) if (bm.IsNotNull)
				s.StreamElements("System", mDeclaredSystems, this,
					StreamSystemPrototypeKey, StreamDeclaredSystemsValue,
					_ctxt => null);*/

			if (s.IsReading)
			{
				if (mSystemPrototypes.Count > 0)
					mActiveSystems = new Dictionary<Values.KGuid, EngineSystemBase>(mSystemPrototypes.Count);
			}
		}

		internal static void SerializePrototype<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			object _context, ref BlamEngine engine)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnum("generation", engine, obj => obj.Generation);
			s.StreamAttribute("name", engine, obj => obj.Name);
		}


		internal static bool SerializeId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref int engineId, bool isOptional = false)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = true;

			if (isOptional)
			{
				streamed = s.StreamAttributeOptIdAsString(attributeName, ref engineId, null,
					EngineIdResolver, EngineNameResolver, Predicates.IsNotNullOrEmpty);

				if (!streamed && s.IsReading)
					engineId = TypeExtensions.kNone;
			}
			else
			{
				s.StreamAttributeIdAsString(attributeName, ref engineId, null,
					EngineIdResolver, EngineNameResolver);
			}

			return streamed;
		}
	};
}