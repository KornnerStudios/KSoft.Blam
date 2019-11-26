#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Engine
{
	/// <summary>Represents the prototype information for a specific system that an engine requires (eg, Blobs)</summary>
	internal sealed class BlamEngineSystem
		: IO.ITagElementStringNameStreamable
	{
		public BlamEngine Engine { get; private set; }

		internal EngineSystemAttribute SystemMetadata { get; private set; }

		/// <summary>Name of the file with the system's external definitions</summary>
		public string ExternsFile { get; private set; }

		internal bool SystemRequiresReferenceTracking { get { return SystemMetadata.KeepExternsLoaded; } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			var engine = KSoft.Debug.TypeCheck.CastReference<BlamEngine>(s.UserData);
			if (reading)
				Engine = engine;
			else
				Contract.Assert(engine == Engine);

			if (reading)
			{
				var system_guid = Values.KGuid.Empty;
				s.StreamAttribute("guid", ref system_guid);

				SystemMetadata = EngineRegistry.TryGetRegisteredSystem(system_guid);

				if (SystemMetadata == null)
				{
					string msg = string.Format("No system is registered with the GUID {0}",
						system_guid.ToString(Values.KGuid.kFormatHyphenated));

					s.ThrowReadException(new System.IO.InvalidDataException(msg));
				}
			}
			else
				Contract.Assert(false, "Writing not supported");

			s.StreamAttribute("externs", this, o => o.ExternsFile);
		}
		#endregion
	};
};