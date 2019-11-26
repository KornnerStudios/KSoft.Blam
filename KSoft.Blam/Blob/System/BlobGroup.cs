using System.Collections.Generic;

namespace KSoft.Blam.Blob
{
	using GroupTagDatum = Values.GroupTagData32;

	public sealed class BlobGroup
		: IO.ITagElementStringNameStreamable
	{
		GroupTagDatum mGroupTag;
		public GroupTagDatum GroupTag { get { return mGroupTag; } }

		public WellKnownBlob mKnownAs;
		public WellKnownBlob KnownAs { get { return mKnownAs; } }

		Dictionary<int, Engine.EngineBuildHandle> mVersionToBuildMap;
		public IReadOnlyDictionary<int, Engine.EngineBuildHandle> VersionToBuildMap { get { return mVersionToBuildMap; } }

		public BlobGroup()
		{
			mGroupTag = GroupTagDatum.Null;
			mKnownAs = WellKnownBlob.NotWellKnown;
			mVersionToBuildMap = new Dictionary<int, Engine.EngineBuildHandle>();
		}

		#region ITagElementStreamable<string> Members
		internal static void SerializeVersionToBuildMapKey<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Engine.EngineBuildHandle _dontUse,
			ref int versionId)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("id", ref versionId, NumeralBase.Decimal);
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var system = KSoft.Debug.TypeCheck.CastReference<BlobSystem>(s.UserData);

			BlobSystem.SerializeGroupsKey(s, system, ref mGroupTag);

			s.StreamAttributeEnumOpt("type", ref mKnownAs, v => v != WellKnownBlob.NotWellKnown);

			using (s.EnterCursorBookmark("Versions"))
			{
				s.StreamElements("Version",
					mVersionToBuildMap, system.Engine.RootBuildHandle,
					SerializeVersionToBuildMapKey,
					Engine.EngineBuildHandle.SerializeWithBaseline);
			}
		}
		#endregion
	};
}