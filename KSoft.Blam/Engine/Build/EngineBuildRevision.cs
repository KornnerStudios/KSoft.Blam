using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.Engine
{
	public sealed class EngineBuildRevision
		: IO.ITagElementStringNameStreamable
	{
		#region Constants
		const string kDateFormat = "MMM dd yyyy";
		const string kDateTimeFormat = "HH:mm:ss";
		const string kDateTimeOffsetFormat = "z";

		internal const int kMaxCount = 32 - 1; // 5 bits. per branch
		internal static readonly int kIndexBitCount;
		private static readonly uint kIndexBitMask;

		static EngineBuildRevision()
		{
			kIndexBitMask = Bits.GetNoneableEncodingTraits(kMaxCount,
				out kIndexBitCount);
		}
		#endregion

		public EngineBuildBranch Branch { get; private set; }
		public EngineBuildHandle BuildHandle { get; private set; }

		public string BuildString { get; private set; }
		public EngineBuildStringType BuildStringType { get; private set; }
		public int Version { get; private set; }
		public int ChangeList { get; private set; }
		public EngineProductionStage ProductionStage { get; private set; }

		string mDateString;
		string mTimeString;
		public DateTime Date { get; private set; }

		public string ExportName { get; private set; }

		#region ValidTargetPlatforms
		Collections.BitSet mValidTargetPlatforms;
		/// <summary>Platforms which this revision can target</summary>
		public Collections.IReadOnlyBitSet ValidTargetPlatforms { get {
			if (mValidTargetPlatforms == null)
				return Branch.ValidTargetPlatforms;

			return mValidTargetPlatforms;
		} }
		#endregion

		public EngineBuildRevision()
		{
			BuildHandle = EngineBuildHandle.None;

			BuildString =
				"00000.00.00.00.0000";
			BuildStringType = EngineBuildStringType.Build_DateTime;

			Version = ChangeList =
				TypeExtensions.kNone;

			Date = DateTime.MinValue;
			mDateString = Date.ToString(kDateFormat);
			mTimeString = Date.ToString(kDateTimeFormat + " " + kDateTimeOffsetFormat);

			ExportName = "";
		}

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return object.ReferenceEquals(this, obj);
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on its exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode()
		{
			Contract.Assert(!BuildHandle.IsNone,
				"Requested the hash code before the build data was fully initialized");

			return BuildHandle.GetHashCode();
		}
		/// <summary>Returns <see cref="BuildString"/></summary>
		/// <returns></returns>
		public override string ToString()
		{
			Contract.Ensures(Contract.Result<string>() != null);

			return BuildString;
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var branch = KSoft.Debug.TypeCheck.CastReference<EngineBuildBranch>(s.UserData);
			if (s.IsReading)
				Branch = branch;
			else
				Contract.Assert(branch == Branch);

			s.StreamAttribute("versionId", this, obj => obj.Version);
			s.StreamAttribute("build", this, obj => obj.BuildString);

			if (!s.StreamAttributeOpt("changeListId", this, obj => obj.ChangeList, Predicates.IsNotNone))
				ChangeList = TypeExtensions.kNoneInt32;

			if (!s.StreamAttributeEnumOpt("productionStage", this, obj => obj.ProductionStage))
				ProductionStage = EngineProductionStage.Undefined;

			if (!s.StreamAttributeEnumOpt("buildStringType", this, obj => obj.BuildStringType))
				BuildStringType = EngineBuildStringType.Build_DateTime;

			s.StreamElement("Date", ref mDateString);
			s.StreamElement("Time", ref mTimeString);

			if (!s.StreamAttributeOpt("exportName", this, obj => obj.ExportName, Predicates.IsNotNullOrEmpty))
				ExportName = "";

			EngineTargetPlatform.SerializeBitSet(s, ref mValidTargetPlatforms, "ValidTargetPlatforms");

			if(s.IsReading)
			{
				const string k_date_format_string =
					kDateFormat + " " +
					kDateTimeFormat + " " +
					kDateTimeOffsetFormat;

				string date_time_str = string.Format("{0} {1}", mDateString, mTimeString);
				try { DateTime.ParseExact(date_time_str, k_date_format_string, System.Globalization.CultureInfo.InvariantCulture); }
				catch(FormatException ex)
				{ s.ThrowReadException(ex); }

				if (ValidTargetPlatforms == EngineRegistry.NullValidTargetPlatforms)
					throw new System.IO.InvalidDataException(string.Format(
						"{0}/{1} or its repository parents didn't specify any ValidTargetPlatforms",
						Branch.Name, BuildString));
			}
		}
		#endregion

		#region Bit encoding
		internal static void BitEncodeIndex(ref Bitwise.HandleBitEncoder encoder, int revisionIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(revisionIndex.IsNoneOrPositive());

			encoder.EncodeNoneable32(revisionIndex, kIndexBitMask);
		}
		internal static int BitDecodeIndex(uint handle, int bitIndex)
		{
			var index = Bits.BitDecodeNoneable(handle, bitIndex, kIndexBitMask);

			Contract.Assert(index.IsNoneOrPositive());
			return index;
		}
		#endregion

		#region Index/Id interfaces
		/// <summary>Initialize the <see cref="BuildHandle"/></summary>
		/// <param name="handle"></param>
		internal void InitializeBuildHandle(EngineBuildHandle handle)
		{
			BuildHandle = handle;
		}
		#endregion
	};
}