using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KSoft.Blam.Blob.Transport
{
	[System.Reflection.Obfuscation(Exclude=false)]
	public partial class BlobTransportStream
		: IDisposable
	{
		#region Constants
		const int kHeaderSize = BlobChunkHeader.kSizeOf + StreamHeader.kSizeOfData;
		const int kFooterSize = BlobChunkHeader.kSizeOf + StreamFooter.kSizeOfDataSansAuthData;
		/// <summary>No blob stream should ever be smaller than this</summary>
		const int kSmallestStreamLength =
			kHeaderSize +
			BlobChunkHeader.kSizeOf + // there should be at least one chunk
			kFooterSize;
		#endregion

		public IO.EndianStream? UnderlyingStream { get; private set; }
		public long StartPosition { get; private set; }
		public long EndPosition { get; private set; }
		long mFooterPosition;
		StreamHeader mHeader;
		StreamFooter mFooter;

		/// <remarks>Required for Enumeration (ie, reading)</remarks>
		public Engine.BlamEngineTargetHandle GameTarget { get; set; }

		/// <summary>Is the <see cref="UnderlyingStream"/> not open?</summary>
		public bool IsClosed => UnderlyingStream == null;
		public Stream? BaseStream => UnderlyingStream?.BaseStream;
		public long AssumedStreamSize => EndPosition - StartPosition;
		public long AssumedBlobSize => mFooterPosition - StartPosition;

		public BlobTransportStream(string fileType = "")
		{
			mHeader = new StreamHeader(fileType);
		}

		#region Open
		[System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(UnderlyingStream))]
		void OpenUnderlyingStream(Stream baseStream, FileAccess permissions, Shell.EndianFormat endian, bool baseStreamOwner = false)
		{
			UnderlyingStream = new IO.EndianStream(baseStream, endian,
				streamOwner: this, name: "BlobStream", permissions: permissions);

			if (!baseStreamOwner)
			{
				UnderlyingStream.BaseStreamOwner = false;
			}
		}
		void OpenStartPosition(Stream baseStream, long startPosition)
		{
			StartPosition = startPosition >= 0 ? startPosition : 0;
			if (baseStream.Position != StartPosition)
			{
				baseStream.Seek(StartPosition, SeekOrigin.Begin);
			}
		}
		void OpenVerifyAssumedStreamSize(ref BlobChunkVerificationResultInfo result)
		{
			if (AssumedStreamSize < kSmallestStreamLength)
			{
				result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.StreamTooSmall,
					BlobChunkVerificationResultContext.Stream, AssumedBlobSize);
			}
		}
		[System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(UnderlyingStream))]
		void VerifyIsOpenForRead()
		{
			if (UnderlyingStream == null)
			{
				throw new InvalidOperationException("Blob transport stream is closed.");
			}
			if (!UnderlyingStream!.CanRead)
			{
				throw new InvalidOperationException("Blob transport stream is not readable.");
			}
		}
		[System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(UnderlyingStream))]
		void VerifyIsOpenForWrite()
		{
			if (UnderlyingStream == null)
			{
				throw new InvalidOperationException("Blob transport stream is closed.");
			}
			if (!UnderlyingStream!.CanWrite)
			{
				throw new InvalidOperationException("Blob transport stream is not writable.");
			}
		}
		void VerifyIsClosedForOpen()
		{
			if (!IsClosed)
			{
				throw new InvalidOperationException("Blob transport stream is already open.");
			}
		}
		static void VerifyOpenArguments(Stream baseStream, FileAccess permissions)
		{
			ArgumentNullException.ThrowIfNull(baseStream);
			if (!baseStream.CanSeek)
			{
				throw new ArgumentException("Stream must support seeking.", nameof(baseStream));
			}
			if (permissions == 0)
			{
				throw new ArgumentException("Why do we have NO permissions?", nameof(permissions));
			}
			if (!baseStream.HasPermissions(permissions))
			{
				throw new ArgumentException("Stream does not have the requested permissions.", nameof(baseStream));
			}
		}
		static void VerifyWriteObjectArray(BlobObject[] objects)
		{
			ArgumentNullException.ThrowIfNull(objects);
			if (objects.Length == 0)
			{
				throw new InvalidOperationException("Need at least one blob object; actual count is 0.");
			}
			if (!Array.TrueForAll(objects, Predicates.IsNotNull))
			{
				throw new ArgumentNullException(nameof(objects), "Blob object in array was null");
			}
		}

		public BlobChunkVerificationResultInfo OpenForWrite(Stream baseStream,
			long startPosition = 0, long length = TypeExtensions.kNoneInt64,
			Shell.EndianFormat endian = Shell.EndianFormat.Big)
		{
			VerifyIsClosedForOpen();
			VerifyOpenArguments(baseStream, FileAccess.Write);
			if (!length.IsNoneOrPositive())
			{
				throw new ArgumentOutOfRangeException(nameof(length), length,
					"Length must be None or positive.");
			}
			var result = BlobChunkVerificationResultInfo.ValidResult;

			OpenUnderlyingStream(baseStream, FileAccess.Write, endian);
			OpenStartPosition(baseStream, startPosition);

			EndPosition = length.IsNotNone()
				? StartPosition + length
				: length;
			mFooterPosition = TypeExtensions.kNoneInt64;
			UnderlyingStream!.StreamMode = FileAccess.Write;

			if (EndPosition.IsNotNone())
			{
				OpenVerifyAssumedStreamSize(ref result);
			}

			return result;
		}
		public BlobChunkVerificationResultInfo Open(Stream baseStream,
			long startPosition = 0, long length = TypeExtensions.kNoneInt64,
			FileAccess permissions = FileAccess.ReadWrite, Shell.EndianFormat endian = Shell.EndianFormat.Big)
		{
			VerifyIsClosedForOpen();
			VerifyOpenArguments(baseStream, permissions);
			if (!length.IsNoneOrPositive())
			{
				throw new ArgumentOutOfRangeException(nameof(length), length,
					"Length must be None or positive.");
			}
			var result = BlobChunkVerificationResultInfo.ValidResult;

			OpenUnderlyingStream(baseStream, permissions, endian);
			OpenStartPosition(baseStream, startPosition);

			EndPosition = length.IsNotNone() ? StartPosition+length : baseStream.Length;
			mFooterPosition = TypeExtensions.kNoneInt64;

			OpenVerifyAssumedStreamSize(ref result);

			return result;
		}
		public BlobChunkVerificationResultInfo OpenRange(Stream baseStream,
			long startPosition = 0, long endPosition = TypeExtensions.kNoneInt64,
			FileAccess permissions = FileAccess.ReadWrite, Shell.EndianFormat endian = Shell.EndianFormat.Big)
		{
			VerifyIsClosedForOpen();
			VerifyOpenArguments(baseStream, permissions);
			if (!endPosition.IsNoneOrPositive())
			{
				throw new ArgumentOutOfRangeException(nameof(endPosition), endPosition,
					"End position must be None or positive.");
			}
			var result = BlobChunkVerificationResultInfo.ValidResult;

			OpenUnderlyingStream(baseStream, permissions, endian);
			OpenStartPosition(baseStream, startPosition);

			EndPosition = endPosition.IsNotNone() ? endPosition : baseStream.Length;
			mFooterPosition = TypeExtensions.kNoneInt64;

			OpenVerifyAssumedStreamSize(ref result);

			return result;
		}
		#endregion

		#region IDisposable Members
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (UnderlyingStream != null)
				{
					UnderlyingStream.Dispose();
					UnderlyingStream = null;
				}
			}
		}
		#endregion

		byte[]? BuildAuthenticationData()
		{
			if (mFooterPosition.IsNone())
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Footer position must be set before building authentication data; actual value is {0}.",
					mFooterPosition));
			}

			System.Security.Cryptography.HashAlgorithm? hash_algo = null;
			switch (mFooter.Authentication)
			{
				case BlobTransportStreamAuthentication.None: break;
				// #TODO_IMPLEMENT:
				case BlobTransportStreamAuthentication.Crc: throw new NotImplementedException();
				case BlobTransportStreamAuthentication.Hash: throw new NotImplementedException();
				case BlobTransportStreamAuthentication.Rsa: throw new NotImplementedException();
			}

			if (hash_algo != null)
			{
				return hash_algo.ComputeHash(BaseStream!, StartPosition, mFooterPosition);
			}

			System.Diagnostics.Debug.Assert(mFooter.Authentication == BlobTransportStreamAuthentication.None);
			return null;
		}

		#region Verification
		BlobChunkVerificationResultInfo VerifyEnoughBytesForChunkOrData(long totalSize)
		{
			var result = BlobChunkVerificationResultInfo.ValidResult;
			if (UnderlyingStream!.IsWriting && EndPosition.IsNone())
			{
				return result;
			}

			long bytes_remaining = BaseStream!.BytesRemaining(EndPosition);
			if (bytes_remaining < totalSize)
			{
				result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.EndOfStream,
					BlobChunkVerificationResultContext.Stream, bytes_remaining);
			}

			return result;
		}
		BlobChunkVerificationResultInfo VerifyStart(bool streamFirst = true)
		{
			var result = streamFirst ? VerifyEnoughBytesForChunkOrData(kHeaderSize) : BlobChunkVerificationResultInfo.ValidResult;

			if (result.IsValid)
			{
				if (streamFirst)
				{
					UnderlyingStream!.Stream(ref mHeader);
				}

				result = mHeader.Verify(out bool requires_byteswap);

				if (result.IsValid && requires_byteswap)
				{
					UnderlyingStream!.ChangeByteOrder(UnderlyingStream!.ByteOrder.Invert());
				}
			}

			if (result.IsInvalid)
			{
				result.Context = BlobChunkVerificationResultContext.Header;
			}

			return result;
		}
		BlobChunkVerificationResultInfo VerifyEof(bool streamFirst = true,
			BlobTransportStreamAuthentication expectedAuthentication = BlobTransportStreamAuthentication.None)
		{
			var result = streamFirst ? VerifyEnoughBytesForChunkOrData(kFooterSize) : BlobChunkVerificationResultInfo.ValidResult;

			if (result.IsValid)
			{
				if (streamFirst)
				{
					mFooterPosition = BaseStream!.Position;
					UnderlyingStream!.Stream(ref mHeader);
				}
				long blob_size = AssumedBlobSize;

				result = mFooter.Verify(expectedAuthentication, blob_size);

				if (streamFirst && result.IsValid)
				{
					mFooter.SerializeAuthenticationData(UnderlyingStream!);
				}
			}

			if (result.IsInvalid)
			{
				result.Context = BlobChunkVerificationResultContext.Footer;
			}

			return result;
		}
		BlobChunkVerificationResultInfo VerifyEofAuthentication()
		{
			var result = BlobChunkVerificationResultInfo.ValidResult;

			byte[]? hash = BuildAuthenticationData();

			if (hash != null)
			{
				bool hashes_equal = hash.EqualsArray(mFooter.AuthenticationData);
				if (!hashes_equal)
				{
					result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.AuthenticationFailed,
						BlobChunkVerificationResultContext.Stream);
				}
			}

			return result;
		}
		#endregion

		/*public*/ bool TryAndFind(BlobChunkHeader signature, long findStartPosition = TypeExtensions.kNone)
		{
			if (!findStartPosition.IsNone() && findStartPosition >= AssumedBlobSize)
			{
				throw new ArgumentOutOfRangeException(nameof(findStartPosition));
			}

			Util.MarkUnusedVariable(ref signature);

			long orig_pos = TypeExtensions.kNone;

			if (findStartPosition.IsNotNone())
			{
				orig_pos = UnderlyingStream!.BaseStream.Position;
				UnderlyingStream!.Seek(findStartPosition + StartPosition, System.IO.SeekOrigin.Begin);
			}

			try
			{
				throw new NotImplementedException("Blob chunk searching is not implemented.");
			}
			finally
			{
				if (orig_pos.IsNotNone())
				{
					UnderlyingStream!.Seek(orig_pos, System.IO.SeekOrigin.Begin);
				}
			}
		}

		#region EnumerateStream
		BlobChunkVerificationResultInfo EnumerateOneChunk<T>(
			out BlobChunkHeader header, ref T resultValue,
			Func<BlobTransportStream, BlobChunkHeader, T> getResultValue, bool getResultValueConsumesChunk,
			out bool isEof)
		{
			header = BlobChunkHeader.Null;
			isEof = false;
			var result = VerifyEnoughBytesForChunkOrData(BlobChunkHeader.kSizeOf);

			if (result.IsValid)
			{
				UnderlyingStream!.Stream(ref header);
				result = header	.VerifyVersionIsPositive()
								.And(header, h => h.VerifyDataSize(0))
								.And(header, h => h.VerifyFlagsIsPostive())
								.And(header, this, (h, s) => s.VerifyEnoughBytesForChunkOrData(h.DataSize));

				isEof = header.Signature == StreamFooter.kSignature.ID;

				if (result.IsValid)
				{
					if (isEof)
					{
						mFooterPosition = BaseStream!.Position - BlobChunkHeader.kSizeOf;
						mFooter.SerializeSansHeader(UnderlyingStream!, header);
						mFooter.SerializeAuthenticationData(UnderlyingStream!);
					}
					else
					{
						resultValue = getResultValue(this, header);

						if (!getResultValueConsumesChunk)
						{
							header.StreamSkipData(BaseStream!);
						}
					}
				}
				else
				{
					if (isEof)
					{
						result.Context = BlobChunkVerificationResultContext.Footer;
					}
				}
			}
			return result;
		}
		BlobChunkVerificationResultInfo EnumerateStream<T>(out IList<KeyValuePair<BlobChunkHeader, T>> results,
			BlobTransportStreamAuthentication expectedAuthentication,
			Func<BlobTransportStream, BlobChunkHeader, T> getResultValue, bool getResultValueConsumesChunk = false)
		{
			results = new List<KeyValuePair<BlobChunkHeader, T>>();
			//var result_info = BlobChunkVerificationResultInfo.ValidResult;

			BlobChunkVerificationResultInfo result_info = VerifyStart();
			bool is_eof = false;
			while (result_info.IsValid)
			{
				T result_value = default!;
				result_info = EnumerateOneChunk(out BlobChunkHeader chunk_header, ref result_value,
					getResultValue, getResultValueConsumesChunk, out is_eof);

				if (result_info.IsValid && !is_eof)
				{
					results.Add(new KeyValuePair<BlobChunkHeader, T>(chunk_header, result_value));
				}
				else
				{
					break;
				}
			}

			if (result_info.IsValid && is_eof)
			{
				result_info = VerifyEof(streamFirst: false, expectedAuthentication: expectedAuthentication);
			}

			return result_info;
		}

		static long GetEnumerateStreamResultDataPosition(BlobTransportStream @this, BlobChunkHeader header)
		{
			Util.MarkUnusedVariable(ref header);

			return @this.BaseStream!.Position; // position at this point will be right after [header]
		}
		static byte[] GetEnumerateStreamResultBytes(BlobTransportStream @this, BlobChunkHeader header)
		{
			byte[] bytes = new byte[header.DataSize];
			@this.UnderlyingStream!.Stream(bytes);

			return bytes;
		}
		#endregion

		#region EnumerateChunks
		BlobObject EnumerateChunksReadObject(BlobSystem blobSystem, BlobGroup blobGroup, BlobGroupVersionAndBuildInfo infoForVersion,
			BlobChunkHeader header, byte[] data)
		{
			if (GameTarget.IsNone)
			{
				throw new InvalidOperationException("Game target must be set before reading blob chunks.");
			}

			var obj = blobSystem.CreateObject(GameTarget, blobGroup, header.Version, header.Size);

			var byte_order = UnderlyingStream!.ByteOrder;
			if (infoForVersion.ForceLittleEndian)
			{
				byte_order = Shell.EndianFormat.Little;
			}

			using (var ms = new System.IO.MemoryStream(data))
			using (var es = new IO.EndianStream(ms, byte_order, this, blobGroup.GroupTag.Name, FileAccess.Read))
			{
				es.StreamMode = FileAccess.Read;

				obj.Serialize(es);
			}

			return obj;
		}

		void EnumerateChunksReadObjectFoundBuildIncompatibility(BlobSystem blobSystem,
			BlobChunkHeader header, BlobGroup blobGroup,
			Engine.EngineBuildHandle buildForBlobVersion, Engine.EngineBuildHandle actualBuild)
		{
			Util.MarkUnusedVariable(ref blobSystem);

			throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
				"Build incompatibility for chunk {0} v{1} sizeof({2}) which uses build={3} " +
				"but we're using build={4} for {5}",
				blobGroup.GroupTag.TagString, header.Version, header.DataSize, buildForBlobVersion.ToDisplayString(),
				actualBuild.ToDisplayString(), UnderlyingStream!.StreamName));
		}
		void EnumerateChunksReadObjectFoundUnknownChunk(BlobSystem blobSystem,
			BlobChunkHeader header, bool throwOnUnhandledChunk)
		{
			const string kUnhandledChunkMessageFormat = " chunk {0} v{1} sizeof({2}) for target={3}";

			Util.MarkUnusedVariable(ref blobSystem);

			Span<char> tag = stackalloc char[sizeof(uint)];
			Values.GroupTagData32.FromUInt(header.Signature, tag);
			var tag_string = new string(tag);

			if (throwOnUnhandledChunk)
			{
				throw new InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"Unhandled" + kUnhandledChunkMessageFormat,
					tag_string, header.Version, header.DataSize, GameTarget.ToDisplayString()));
			}

			Debug.Trace.Blob.TraceInformation("Ignoring" + kUnhandledChunkMessageFormat,
				tag_string, header.Version, header.DataSize, GameTarget.ToDisplayString());
		}
		Task<BlobObject>[] EnumerateChunksReadObjectsAsync(BlobSystem blobSystem,
			bool throwOnUnhandledChunk, IList<KeyValuePair<BlobChunkHeader, byte[]>> chunks)
		{
			var task_list = new List<Task<BlobObject>>(chunks.Count);

			foreach (var kv in chunks)
			{
				BlobChunkHeader header = kv.Key;
				if (blobSystem.TryGetBlobGroup(header.Signature, header.DataSize, header.Version,
						out BlobGroup blob_group,
						out BlobGroupVersionAndBuildInfo info_for_version))
				{
					if (!info_for_version.BuildHandle.IsWithinSameBranch(GameTarget.Build))
					{
						EnumerateChunksReadObjectFoundBuildIncompatibility(blobSystem, header, blob_group,
							info_for_version.BuildHandle, GameTarget.Build);
					}

					var task = Task<BlobObject>.Factory.StartNew(s =>
						(s as BlobTransportStream)!.EnumerateChunksReadObject(blobSystem, blob_group, info_for_version, header, kv.Value),
						this);
					task_list.Add(task);
				}
				else
				{
					EnumerateChunksReadObjectFoundUnknownChunk(blobSystem, header, throwOnUnhandledChunk);
				}
			}

			return task_list.ToArray();
		}
		IEnumerable<BlobObject> EnumerateChunksReadObjectsSync(BlobSystem blobSystem,
			bool throwOnUnhandledChunk, IList<KeyValuePair<BlobChunkHeader, byte[]>> chunks)
		{
			var results = new List<BlobObject>(chunks.Count);

			foreach (var kv in chunks)
			{
				BlobChunkHeader header = kv.Key;
				if (blobSystem.TryGetBlobGroup(header.Signature, header.DataSize, header.Version,
						out BlobGroup blob_group,
						out BlobGroupVersionAndBuildInfo info_for_version))
				{
					if (!info_for_version.BuildHandle.IsWithinSameBranch(GameTarget.Build))
					{
						EnumerateChunksReadObjectFoundBuildIncompatibility(blobSystem, header, blob_group,
							info_for_version.BuildHandle, GameTarget.Build);
					}

					var obj = EnumerateChunksReadObject(blobSystem, blob_group, info_for_version, header, kv.Value);
					results.Add(obj);
				}
				else
				{
					EnumerateChunksReadObjectFoundUnknownChunk(blobSystem, header, throwOnUnhandledChunk);
				}
			}

			return results;
		}

		public async Task<KeyValuePair<BlobChunkVerificationResultInfo, IEnumerable<BlobObject>?>> EnumerateChunksAsync(
			BlobSystem blobSystem,
			bool throwOnUnhandledChunk = true,
			BlobTransportStreamAuthentication expectedAuthentication = BlobTransportStreamAuthentication.None,
			bool authenticateBlob = true)
		{
			ArgumentNullException.ThrowIfNull(blobSystem);
			VerifyIsOpenForRead();

			IEnumerable<BlobObject>? objects = null;

			var result_info = EnumerateStream(out IList<KeyValuePair<BlobChunkHeader, byte[]>> chunks, expectedAuthentication,
				GetEnumerateStreamResultBytes, getResultValueConsumesChunk: true);

			if (result_info.IsValid &&
				mFooter.Authentication > BlobTransportStreamAuthentication.None && authenticateBlob)
			{
				result_info = VerifyEofAuthentication();
			}

			if (result_info.IsValid)
			{
				var tasks = EnumerateChunksReadObjectsAsync(blobSystem, throwOnUnhandledChunk, chunks);
				await Task.WhenAll(tasks).ConfigureAwait(true);
				objects = from task in tasks
						  select task.Result;
			}

			return new KeyValuePair<BlobChunkVerificationResultInfo, IEnumerable<BlobObject>?>(result_info, objects);
		}
		public BlobChunkVerificationResultInfo EnumerateChunks(BlobSystem blobSystem,
			out IEnumerable<BlobObject>? objects, bool throwOnUnhandledChunk = true,
			BlobTransportStreamAuthentication expectedAuthentication = BlobTransportStreamAuthentication.None,
			bool authenticateBlob = true)
		{
			ArgumentNullException.ThrowIfNull(blobSystem);
			VerifyIsOpenForRead();

			objects = null;

			var result_info = EnumerateStream(out IList<KeyValuePair<BlobChunkHeader, byte[]>> chunks, expectedAuthentication,
				GetEnumerateStreamResultBytes, getResultValueConsumesChunk: true);

			if (result_info.IsValid &&
				mFooter.Authentication > BlobTransportStreamAuthentication.None && authenticateBlob)
			{
				result_info = VerifyEofAuthentication();
			}

			if (result_info.IsValid)
			{
				objects = EnumerateChunksReadObjectsSync(blobSystem, throwOnUnhandledChunk, chunks);
			}

			return result_info;
		}
		#endregion

		#region WriteChunks
		BlobChunkVerificationResultInfo WriteChunksWriteStart()
		{
			var result = VerifyEnoughBytesForChunkOrData(kHeaderSize);

			if (result.IsValid)
			{
				mHeader = new StreamHeader(mHeader.FileType);
				mHeader.Serialize(UnderlyingStream!);
			}

			return result;
		}
		BlobChunkVerificationResultInfo WriteChunksWriteEof(BlobTransportStreamAuthentication authentication)
		{
			mFooterPosition = BaseStream!.Position;
			mFooter = new StreamFooter(authentication, AssumedBlobSize);

			var result = VerifyEnoughBytesForChunkOrData(kFooterSize + authentication.GetDataSize());

			if (result.IsValid)
			{
				byte[]? auth_data = BuildAuthenticationData();

				if (auth_data != null)
				{
					if (mFooter.AuthenticationData.Length < auth_data.Length)
					{
						throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
							"Footer authentication buffer length {0} is smaller than authentication data length {1}.",
							mFooter.AuthenticationData.Length,
							auth_data.Length));
					}
					Buffer.BlockCopy(auth_data, 0, mFooter.AuthenticationData, 0, auth_data.Length);
				}

				mFooter.Serialize(UnderlyingStream!);
				mFooter.SerializeAuthenticationData(UnderlyingStream!);
			}

			return result;
		}
		BlobChunkVerificationResultInfo WriteChunksToUnderlyingStream(IEnumerable<KeyValuePair<BlobChunkHeader, byte[]>> chunks)
		{
			var result_info = BlobChunkVerificationResultInfo.ValidResult;

			foreach (var kv in chunks)
			{
				BlobChunkHeader header = kv.Key;
				result_info = VerifyEnoughBytesForChunkOrData(header.Size);
				if (result_info.IsInvalid)
				{
					break;
				}

				header.Serialize(UnderlyingStream!);
				UnderlyingStream!.Writer.Write(kv.Value);
			}

			return result_info;
		}

		KeyValuePair<BlobChunkHeader, byte[]> WriteChunksProcessObject(BlobObject obj)
		{
			byte[] data;

			var byte_order = UnderlyingStream!.ByteOrder;
			if (obj.SystemGroupVersionInfo.ForceLittleEndian)
			{
				byte_order = Shell.EndianFormat.Little;
			}

			var sys_group = obj.SystemGroup;
			// #TODO_IMPLEMENT: support non-fixed length blobs like film streams
			using (var ms = new System.IO.MemoryStream(obj.CalculateFixedBinarySize(this.GameTarget)))
			using (var es = new IO.EndianStream(ms, byte_order, this, sys_group.GroupTag.Name, FileAccess.Write))
			{
				es.StreamMode = FileAccess.Write;

				obj.Serialize(es);

				data = ms.ToArray();
			}

			var header = new BlobChunkHeader(sys_group.GroupTag, obj.Version, data.Length, obj.BlobFlags);

			return new KeyValuePair<BlobChunkHeader, byte[]>(header, data);
		}
		Task<KeyValuePair<BlobChunkHeader, byte[]>>[] WriteChunksProcessObjectsAsync(BlobObject[] objects)
		{
			var tasks = new Task<KeyValuePair<BlobChunkHeader, byte[]>>[objects.Length];
			for (int x = 0; x < objects.Length; x++)
			{
				tasks[x] = Task<KeyValuePair<BlobChunkHeader, byte[]>>.Factory.StartNew(
					obj => WriteChunksProcessObject((BlobObject)obj!),
					objects[x]);
			}

			return tasks;
		}
		async Task<BlobChunkVerificationResultInfo> WriteChunksForObjectsAsync(BlobObject[] objects)
		{
			var result_info = BlobChunkVerificationResultInfo.ValidResult;

			var tasks = WriteChunksProcessObjectsAsync(objects);
			await Task.WhenAll(tasks).ConfigureAwait(true);
			var chunks= from task in tasks
						select task.Result;

			result_info = WriteChunksToUnderlyingStream(chunks);

			return result_info;
		}

		BlobChunkVerificationResultInfo WriteChunksForObjectsSync(BlobObject[] objects)
		{
			//var result_info = BlobChunkVerificationResultInfo.ValidResult;

			var chunks = new KeyValuePair<BlobChunkHeader, byte[]>[objects.Length];
			for (int x = 0; x < objects.Length; x++)
			{
				chunks[x] = WriteChunksProcessObject(objects[x]);
			}

			BlobChunkVerificationResultInfo result_info = WriteChunksToUnderlyingStream(chunks);

			return result_info;
		}

		static BlobObject[] WriteChunksGetObjectsArray(IEnumerable<BlobObject> objects)
		{
			var array = objects is BlobObject[] objectsArray
				? objectsArray
				: objects.ToArray();

			if (array.Length < 1)
			{
				throw new InvalidOperationException("Need at least one blob object");
			}
			else if (!Array.TrueForAll(array, Predicates.IsNotNull))
			{
				throw new ArgumentNullException(nameof(objects), "Blob object in enumeration was null");
			}

			return array;
		}
		public async Task<BlobChunkVerificationResultInfo> WriteChunksAsync(IEnumerable<BlobObject> objects,
			BlobTransportStreamAuthentication authentication = BlobTransportStreamAuthentication.None)
		{
			VerifyIsOpenForWrite();

			var result_info = WriteChunksWriteStart();

			if (result_info.IsValid)
			{
				var array = WriteChunksGetObjectsArray(objects);
				result_info = await WriteChunksForObjectsAsync(array).ConfigureAwait(true);
			}

			if (result_info.IsValid)
			{
				result_info = WriteChunksWriteEof(authentication);
			}

			return result_info;
		}
		public BlobChunkVerificationResultInfo WriteChunks(IEnumerable<BlobObject> objects,
			BlobTransportStreamAuthentication authentication = BlobTransportStreamAuthentication.None)
		{
			VerifyIsOpenForWrite();

			var result_info = WriteChunksWriteStart();

			if (result_info.IsValid)
			{
				var array = WriteChunksGetObjectsArray(objects);
				result_info = WriteChunksForObjectsSync(array);
			}

			if (result_info.IsValid)
			{
				result_info = WriteChunksWriteEof(authentication);
			}

			return result_info;
		}

		public async Task<BlobChunkVerificationResultInfo> WriteChunksSansAuthenticationAsync(params BlobObject[] objects)
		{
			VerifyIsOpenForWrite();
			VerifyWriteObjectArray(objects);

			return await WriteChunksAsync(objects, BlobTransportStreamAuthentication.None).ConfigureAwait(true);
		}
		public BlobChunkVerificationResultInfo WriteChunksSansAuthentication(params BlobObject[] objects)
		{
			VerifyIsOpenForWrite();
			VerifyWriteObjectArray(objects);

			return WriteChunks(objects, BlobTransportStreamAuthentication.None);
		}

		public async Task<BlobChunkVerificationResultInfo> WriteChunksWithAuthenticationAsync(BlobTransportStreamAuthentication authentication,
			params BlobObject[] objects)
		{
			VerifyIsOpenForWrite();
			VerifyWriteObjectArray(objects);

			return await WriteChunksAsync(objects, authentication).ConfigureAwait(true);
		}
		public BlobChunkVerificationResultInfo WriteChunksWithAuthentication(BlobTransportStreamAuthentication authentication,
			params BlobObject[] objects)
		{
			VerifyIsOpenForWrite();
			VerifyWriteObjectArray(objects);

			return WriteChunks(objects, authentication);
		}
		#endregion
	};
}
