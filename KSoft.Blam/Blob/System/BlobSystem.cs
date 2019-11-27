using System;
using System.Collections.Generic;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.Blob
{
	using GroupTagDatum = Values.GroupTagData32;
	using GroupTagDatumCollection = Values.GroupTag32Collection;

	/// <summary>Interface for 'blob' (blf files) related services for an engine and its builds</summary>
	[Engine.EngineSystem]
	public sealed class BlobSystem
		: Engine.EngineSystemBase
	{
		public static Values.KGuid SystemGuid { get; } = new Values.KGuid("EA5FFB03-9991-4B81-B952-022EDC371F27");

		GroupTagDatumCollection mGroupTags;
		public GroupTagDatumCollection GroupTags { get { return mGroupTags; } }

		Dictionary<GroupTagDatum, BlobGroup> mGroups;
		public IReadOnlyDictionary<GroupTagDatum, BlobGroup> Groups { get { return mGroups; } }

		internal BlobSystem()
		{
			mGroupTags = GroupTagDatumCollection.Empty;
			mGroups = new Dictionary<GroupTagDatum, BlobGroup>();
		}

		#region TryGetBlobGroup
		bool TryGetBlobGroup(GroupTagDatum groupTag, int version,
			ref BlobGroup group, ref Blam.Engine.EngineBuildHandle buildForVersion)
		{
			if (groupTag != null)
			{
				var group_candidate = Groups[groupTag];
				if (group_candidate.VersionToBuildMap.TryGetValue(version, out buildForVersion))
				{
					group = group_candidate;
					return true;
				}
			}

			return false;
		}

		bool TryGetBlobGroup(string tagString, int version,
			out BlobGroup group, out Blam.Engine.EngineBuildHandle buildForVersion)
		{
			Contract.Requires<ArgumentOutOfRangeException>(version.IsNotNone());

			group = null;
			buildForVersion = Blam.Engine.EngineBuildHandle.None;

			var group_tag = (GroupTagDatum)GroupTags.FindGroupByTag(tagString);
			return TryGetBlobGroup(group_tag, version, ref group, ref buildForVersion);
		}

		public bool TryGetBlobGroup(uint signature, int binarySize, int version,
			out BlobGroup group, out Blam.Engine.EngineBuildHandle buildForVersion)
		{
			Contract.Requires<ArgumentOutOfRangeException>(version.IsNotNone());

			group = null;
			buildForVersion = Blam.Engine.EngineBuildHandle.None;

			var group_tag = GroupTags.FindGroupByTag(signature);
			return TryGetBlobGroup(group_tag, version, ref group, ref buildForVersion);
		}
		public bool TryGetBlobGroup(uint signature, int binarySize, int version,
			out BlobGroup group)
		{
			Contract.Requires<ArgumentOutOfRangeException>(version.IsNotNone());

			Blam.Engine.EngineBuildHandle build;
			return TryGetBlobGroup(signature, binarySize, version, out group, out build);
		}
		#endregion

		#region CreateObject
		static BlobObject CreateObject(BlobSystem system, Engine.BlamEngineTargetHandle gameTarget,
			BlobGroup blobGroup,
			int version, int binarySize)
		{
			Contract.Requires(system != null);

			return null;
		}

		public BlobObject CreateObject(Engine.BlamEngineTargetHandle gameTarget,
			BlobGroup blobGroup,
			int version = TypeExtensions.kNone, int binarySize = TypeExtensions.kNone)
		{
			Contract.Requires(!gameTarget.IsNone);
			Contract.Requires(blobGroup != null);
			Contract.Requires(version.IsNoneOrPositive());

			return CreateObject(this, gameTarget, blobGroup, version, binarySize);
		}
		#endregion

		#region ITagElementStreamable<string> Members
		internal static void SerializeGroupsKey<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			BlobSystem system,
			ref GroupTagDatum groupTag)
			where TDoc : class
			where TCursor : class
		{
			var tag_string = s.IsReading
				? null
				: groupTag.TagString;

			s.StreamAttribute("tag", ref tag_string);

			if (s.IsReading)
			{
				groupTag = (GroupTagDatum)system.GroupTags.FindGroupByTag(tag_string);

				if (groupTag == null)
					s.ThrowReadException(new KeyNotFoundException(string.Format(
						"The tag '{0}' isn't defined in this system",
						tag_string)));
			}
		}

		protected override void SerializeExternBody<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			IO.TagElementStreamDefaultSerializer.Serialize(s, ref mGroupTags,
				"BlobGroupTags");

			using (s.EnterCursorBookmark("BlobGroups"))
			{
				s.StreamableElements("BlobGroup", mGroups, this, SerializeGroupsKey);
			}
		}
		#endregion

		#region StreamObjects
		static Exception SerializeObjectFoundBuildIncompatibility(BlobSystem blobSystem,
			BlobGroup blobGroup, int version,
			Engine.EngineBuildHandle buildForBlobVersion, Engine.EngineBuildHandle actualBuild)
		{
			var msg = string.Format(
				"Build incompatibility for blob object {0} v{1} which uses build={2} " +
				"but stream uses build={3}",
				blobGroup.GroupTag.TagString, version, buildForBlobVersion.ToDisplayString(),
				actualBuild.ToDisplayString());

			return new System.IO.InvalidDataException(msg);
		}

		struct SerializeObjectContext
		{
			public readonly BlobSystem System;
			public readonly Engine.BlamEngineTargetHandle GameTarget;

			public SerializeObjectContext(BlobSystem system, Engine.BlamEngineTargetHandle gameTarget)
			{
				System = system;
				GameTarget = gameTarget;
			}
		};
		static void SerializeObject<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, SerializeObjectContext ctxt,
			ref BlobObject obj)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(obj != null || s.IsReading);

			const string kAttributeNameSignature = "signature";
			const string kAttributeNameVersion = "version";

			if (s.IsReading)
			{
				string group_tag = null;
				s.ReadAttribute(kAttributeNameSignature, ref group_tag);

				int version = TypeExtensions.kNone;
				s.ReadAttribute(kAttributeNameVersion, ref version);

				BlobGroup blob_group;
				Engine.EngineBuildHandle build_for_version;
				if (ctxt.System.TryGetBlobGroup(group_tag, version, out blob_group, out build_for_version))
				{
					var target_build = ctxt.GameTarget.Build;

					if (!build_for_version.IsWithinSameBranch(target_build))
						s.ThrowReadException(SerializeObjectFoundBuildIncompatibility(ctxt.System,
							blob_group, version, build_for_version, target_build));

					obj = ctxt.System.CreateObject(Blam.Engine.BlamEngineTargetHandle.None, blob_group, version);
				}
				else
				{
					string msg = string.Format("{0}: No blob matching '{1}'/v{2} exists",
						ctxt.GameTarget.ToDisplayString(), group_tag, version);
					s.ThrowReadException(new System.IO.InvalidDataException(msg));
				}
			}
			else if (s.IsWriting)
			{
				s.WriteAttribute(kAttributeNameSignature, obj.SystemGroup.GroupTag.TagString);
				s.WriteAttribute(kAttributeNameVersion, obj.Version);
			}
			s.StreamAttribute("flags", obj, _obj => _obj.BlobFlags);

			obj.Serialize(s);
		}

		public ICollection<BlobObject> StreamObjects<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			ICollection<BlobObject> results, Engine.BlamEngineTargetHandle gameTarget)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires<ArgumentNullException>(!gameTarget.IsNone);

			if (s.IsReading)
				results = new List<BlobObject>();

			var ctxt = new SerializeObjectContext(this, gameTarget);
			s.StreamElements("BlobObject", results, ctxt, SerializeObject,
				_ctxt => null);

			return results;
		}
		#endregion
	};
}