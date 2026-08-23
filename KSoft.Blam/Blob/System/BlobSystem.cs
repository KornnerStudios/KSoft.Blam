using System;
using System.Collections.Generic;

namespace KSoft.Blam.Blob
{
	using GroupTagDatum = Values.GroupTagData32;
	using GroupTagDatumCollection = Values.GroupTag32Collection;

	/// <summary>Interface for 'blob' (blf files) related services for an engine and its builds</summary>
	[Engine.EngineSystem]
	public sealed class BlobSystem
		: Engine.EngineSystemBase
	{
		public static Values.KGuid SystemGuid { get; } = new("EA5FFB03-9991-4B81-B952-022EDC371F27");

		GroupTagDatumCollection mGroupTags;
		public GroupTagDatumCollection GroupTags => mGroupTags;

		readonly Dictionary<GroupTagDatum, BlobGroup> mGroups = new();
		public IReadOnlyDictionary<GroupTagDatum, BlobGroup> Groups => mGroups;

		internal BlobSystem()
		{
			mGroupTags = GroupTagDatumCollection.Empty;
		}

		#region TryGetBlobGroup
		bool TryGetBlobGroup(GroupTagDatum? groupTag, int version,
			ref BlobGroup group, ref BlobGroupVersionAndBuildInfo infoForVersion)
		{
			if (groupTag != null)
			{
				var group_candidate = Groups[groupTag];
				if (group_candidate.VersionAndBuildMap.TryGetValue(version, out var info_for_version))
				{
					infoForVersion = info_for_version!;
					group = group_candidate;
					return true;
				}
			}

			return false;
		}

		bool TryGetBlobGroup(string? tagString, int version,
			[System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out BlobGroup group,
			[System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out BlobGroupVersionAndBuildInfo infoForVersion)
		{
			if (version.IsNone())
			{
				throw new ArgumentOutOfRangeException(nameof(version));
			}

			group = null!;
			infoForVersion = null!;

			var group_tag = (GroupTagDatum?)GroupTags.FindGroupByTag(tagString!);
			return TryGetBlobGroup(group_tag, version, ref group, ref infoForVersion);
		}

		public bool TryGetBlobGroup(uint signature, int binarySize, int version,
			[System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out BlobGroup group,
			[System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out BlobGroupVersionAndBuildInfo infoForVersion)
		{
			if (version.IsNone())
			{
				throw new ArgumentOutOfRangeException(nameof(version));
			}

			Util.MarkUnusedVariable(ref binarySize);

			group = null!;
			infoForVersion = null!;

			var group_tag = GroupTags.FindGroupByTag(signature);
			return TryGetBlobGroup(group_tag, version, ref group, ref infoForVersion);
		}
		public bool TryGetBlobGroup(uint signature, int binarySize, int version,
			out BlobGroup group)
		{
			if (version.IsNone())
			{
				throw new ArgumentOutOfRangeException(nameof(version));
			}

			return TryGetBlobGroup(signature, binarySize, version,
				out group,
				out BlobGroupVersionAndBuildInfo /*infoForVersion*/_);
		}

		public bool TryGetBlobGroup(WellKnownBlob kind,
			[System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out BlobGroup group)
		{
			group = null!;

			foreach (var kvp in Groups)
			{
				if (kvp.Value.KnownAs == kind)
				{
					group = kvp.Value;
					break;
				}
			}

			return group != null;
		}
		#endregion

		#region CreateObject
		BlobObject CreateObjectImpl(Engine.BlamEngineTargetHandle gameTarget,
			BlobGroup blobGroup,
			int version, int binarySize)
		{
			Util.MarkUnusedVariable(ref binarySize);

			BlobObject bobject = blobGroup.KnownAs switch
			{
				WellKnownBlob.ContentHeader => new ContentHeaderBlob(),
				WellKnownBlob.GameVariant => new GameEngineVariantBlob(),
				_ => throw new KSoft.Debug.UnreachableException(blobGroup.GroupTag.Name),
			};

			bobject.Initialize(this, gameTarget, blobGroup, version);

			return bobject;
		}

		public BlobObject CreateObject(Engine.BlamEngineTargetHandle gameTarget,
			BlobGroup blobGroup,
			int version = TypeExtensions.kNone, int binarySize = TypeExtensions.kNone)
		{
			if (gameTarget.IsNone)
			{
				throw new ArgumentNoneException(nameof(gameTarget));
			}
			ArgumentNullException.ThrowIfNull(blobGroup);
			if (!version.IsNoneOrPositive())
			{
				throw new ArgumentOutOfRangeException(nameof(version), version,
					"Version must be None or positive.");
			}

			return CreateObjectImpl(gameTarget, blobGroup, version, binarySize);
		}

		public BlobObject CreateObject(Engine.BlamEngineTargetHandle gameTarget,
			WellKnownBlob knownBlob)
		{
			if (gameTarget.IsNone)
			{
				throw new ArgumentNoneException(nameof(gameTarget));
			}
			if (!gameTarget.Build.IsFullyFormed)
			{
				throw new ArgumentException("Target build needs to be fully formed", nameof(gameTarget));
			}
			if (knownBlob == WellKnownBlob.NotWellKnown)
			{
				throw new ArgumentException(nameof(WellKnownBlob.NotWellKnown), nameof(knownBlob));
			}

			if (!TryGetBlobGroup(knownBlob, out BlobGroup known_blob_group))
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"No blob groups marked to be known as {0} under {1}",
					knownBlob,
					this.Prototype.Engine));
			}

			BlobGroupVersionAndBuildInfo? known_blob_group_version_info =
				known_blob_group.FindMostRelaventVersionInfo(gameTarget.Build);
			if (known_blob_group_version_info == null)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"No {0} blob group versions resolved possibly for {1} under {2}",
					knownBlob,
					gameTarget.Build,
					this.Prototype.Engine));
			}

			return CreateObject(gameTarget, known_blob_group, known_blob_group_version_info.MajorVersion);
		}
		#endregion

		#region ITagElementStreamable<string> Members
		internal static void SerializeGroupsKey<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			BlobSystem system,
			ref GroupTagDatum groupTag)
			where TDoc : class
			where TCursor : class
		{
			string tag_string = s.IsReading
				? null!
				: groupTag.TagString;

			s.StreamAttribute("tag", ref tag_string);

			if (s.IsReading)
			{
				var serialized_group_tag = (GroupTagDatum?)system.GroupTags.FindGroupByTag(tag_string!);

				if (serialized_group_tag == null)
				{
					s.ThrowReadException(new KeyNotFoundException(string.Format(Util.InvariantCultureInfo,
						"The tag '{0}' isn't defined in this system",
						tag_string)));
				}

				groupTag = serialized_group_tag!;
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
		static System.IO.InvalidDataException SerializeObjectFoundBuildIncompatibility(BlobSystem blobSystem,
			BlobGroup blobGroup, int version,
			Engine.EngineBuildHandle buildForBlobVersion, Engine.EngineBuildHandle actualBuild)
		{
			Util.MarkUnusedVariable(ref blobSystem);

			var msg = string.Format(Util.InvariantCultureInfo,
				"Build incompatibility for blob object {0} v{1} which uses build={2} " +
				"but stream uses build={3}",
				blobGroup.GroupTag.TagString, version, buildForBlobVersion.ToDisplayString(),
				actualBuild.ToDisplayString());

			return new System.IO.InvalidDataException(msg);
		}

		readonly struct SerializeObjectContext
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
			ArgumentNullException.ThrowIfNull(s);
			if (s.IsWriting)
			{
				ArgumentNullException.ThrowIfNull(obj);
			}

			const string kAttributeNameSignature = "signature";
			const string kAttributeNameVersion = "version";

			if (s.IsReading)
			{
				string group_tag = null!;
				s.ReadAttribute(kAttributeNameSignature, ref group_tag);

				int version = TypeExtensions.kNone;
				s.ReadAttribute(kAttributeNameVersion, ref version);

				if (ctxt.System.TryGetBlobGroup(group_tag, version,
						out BlobGroup blob_group,
						out BlobGroupVersionAndBuildInfo info_for_version))
				{
					var target_build = ctxt.GameTarget.Build;

					if (!info_for_version.BuildHandle.IsWithinSameBranch(target_build))
					{
						s.ThrowReadException(SerializeObjectFoundBuildIncompatibility(ctxt.System,
							blob_group, version, info_for_version.BuildHandle, target_build));
					}

					obj = ctxt.System.CreateObject(Blam.Engine.BlamEngineTargetHandle.None, blob_group, version);
				}
				else
				{
					string msg = string.Format(Util.InvariantCultureInfo,
						"{0}: No blob matching '{1}'/v{2} exists",
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
			if (gameTarget.IsNone)
			{
				throw new ArgumentNoneException(nameof(gameTarget));
			}

			if (s.IsReading)
			{
				results = new List<BlobObject>();
			}

			var ctxt = new SerializeObjectContext(this, gameTarget);
			s.StreamElements("BlobObject", results, ctxt, SerializeObject,
				_ctxt => null!);

			return results;
		}
		#endregion
	};
}
