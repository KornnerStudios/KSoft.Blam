using System.Collections.Generic;

namespace KSoft.Blam.Blob
{
	using GroupTagDatum = Values.GroupTagData32;

	public sealed class BlobGroupVersionAndBuildInfo
		: IO.ITagElementStringNameStreamable
	{
		int mMajorVersion;
		public int MajorVersion { get { return mMajorVersion; } }

		Engine.EngineBuildHandle mBuildHandle;
		public Engine.EngineBuildHandle BuildHandle { get { return mBuildHandle; } }

		bool mForceLittleEndian;
		// Some people seem to have a certain affinity for breaking conventions
		public bool ForceLittleEndian { get { return mForceLittleEndian; } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var system = KSoft.Debug.TypeCheck.CastReference<BlobSystem>(s.UserData);

			if (s.IsReading)
			{
				BlobGroup.SerializeVersionToBuildMapKey(s, (object)null,
					ref mMajorVersion);
			}

			Engine.EngineBuildHandle.SerializeWithBaseline(s, system.RootBuildHandle, ref mBuildHandle);

			s.StreamAttributeOpt("forceLittleEndian", ref mForceLittleEndian, Predicates.IsTrue);
		}
		#endregion
	};

	public sealed class BlobGroup
		: IO.ITagElementStringNameStreamable
	{
		public BlobSystem ParentSystem { get; private set; }

		GroupTagDatum mGroupTag;
		public GroupTagDatum GroupTag { get { return mGroupTag; } }

		WellKnownBlob mKnownAs;
		public WellKnownBlob KnownAs { get { return mKnownAs; } }

		readonly Dictionary<int, BlobGroupVersionAndBuildInfo> mVersionAndBuildMap;
		public IReadOnlyDictionary<int, BlobGroupVersionAndBuildInfo> VersionAndBuildMap { get { return mVersionAndBuildMap; } }

		public BlobGroup()
		{
			mGroupTag = GroupTagDatum.Null;
			mKnownAs = WellKnownBlob.NotWellKnown;
			mVersionAndBuildMap = new Dictionary<int, BlobGroupVersionAndBuildInfo>();
		}

		public BlobGroupVersionAndBuildInfo FindMostRelaventVersionInfo(Engine.EngineBuildHandle forBuild)
		{
			if (forBuild.IsNone)
				throw new ArgumentNoneException(nameof(forBuild));

			BlobGroupVersionAndBuildInfo exact_match = null;
			BlobGroupVersionAndBuildInfo most_relevant = null;
			int possibly_relevant_count = 0;

			foreach (var kvp in VersionAndBuildMap)
			{
				var possible_info = kvp.Value;

				// Solves for versions tied to a specific revision, if any
				if (exact_match == null &&
					possible_info.BuildHandle == forBuild)
				{
					exact_match = possible_info;
				}

				// Else, find the last one in the branch
				if (forBuild.IsWithinSameBranch(possible_info.BuildHandle))
				{
					most_relevant = possible_info;
					possibly_relevant_count++;
				}
			}

			return exact_match ?? most_relevant;
		}

		#region ITagElementStreamable<string> Members
		internal static void SerializeVersionToBuildMapKey<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			object _dontUse,
			ref int versionId)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("id", ref versionId);
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (s.IsReading)
			{
				var system = KSoft.Debug.TypeCheck.CastReference<BlobSystem>(s.UserData);
				ParentSystem = system;
			}

			BlobSystem.SerializeGroupsKey(s, ParentSystem, ref mGroupTag);

			s.StreamAttributeEnumOpt("type", ref mKnownAs, v => v != WellKnownBlob.NotWellKnown);

			using (s.EnterCursorBookmark("Versions"))
			{
				s.StreamableElements("V",
					mVersionAndBuildMap,
					SerializeVersionToBuildMapKey);
			}
		}
		#endregion
	};
}