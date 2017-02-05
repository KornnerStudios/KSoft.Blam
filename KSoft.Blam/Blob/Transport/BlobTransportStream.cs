using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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

		public IO.EndianStream UnderlyingStream { get; private set; }
		public long StartPosition { get; private set; }
		public long EndPosition { get; private set; }
		long mFooterPosition;
		StreamHeader mHeader;
		StreamFooter mFooter;

		/// <remarks>Required for Enumeration (ie, reading)</remarks>
		public Engine.BlamEngineTargetHandle GameTarget { get; set; }

		/// <summary>Is the <see cref="UnderlyingStream"/> not open?</summary>
		public bool IsClosed { get { return UnderlyingStream == null; } }
		public Stream BaseStream { get { return !IsClosed ? UnderlyingStream.BaseStream : null; } }
		public long AssumedStreamSize { get { return EndPosition - StartPosition; } }
		public long AssumedBlobSize { get { return mFooterPosition - StartPosition; } }

		public BlobTransportStream(string fileType = "")
		{
			mHeader = new StreamHeader(fileType);
		}

		#region Open
		void OpenUnderlyingStream(Stream baseStream, FileAccess permissions, Shell.EndianFormat endian, bool baseStreamOwner = false)
		{
			UnderlyingStream = new IO.EndianStream(baseStream, endian,
				streamOwner: this, name: "BlobStream", permissions: permissions);

			if (!baseStreamOwner)
				UnderlyingStream.BaseStreamOwner = false;
		}
		void OpenStartPosition(Stream baseStream, long startPosition)
		{
			StartPosition = startPosition >= 0 ? startPosition : 0;
			if (baseStream.Position != StartPosition)
				baseStream.Seek(StartPosition, SeekOrigin.Begin);
		}
		void OpenVerifyAssumedStreamSize(ref BlobChunkVerificationResultInfo result)
		{
			if (AssumedStreamSize < kSmallestStreamLength)
				result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.StreamTooSmall,
					BlobChunkVerificationResultContext.Stream, AssumedBlobSize);
		}

		public BlobChunkVerificationResultInfo OpenForWrite(Stream baseStream,
			long startPosition = 0, long length = TypeExtensions.kNoneInt64,
			Shell.EndianFormat endian = Shell.EndianFormat.Big)
		{
			Contract.Requires(IsClosed);
			Contract.Requires<ArgumentNullException>(baseStream != null);
			Contract.Requires<ArgumentException>(baseStream.CanSeek);
			Contract.Requires<ArgumentException>(baseStream.HasPermissions(FileAccess.Write));
			Contract.Requires(length.IsNoneOrPositive());
			var result = BlobChunkVerificationResultInfo.ValidResult;

			OpenUnderlyingStream(baseStream, FileAccess.Write, endian);
			OpenStartPosition(baseStream, startPosition);

			EndPosition = length.IsNotNone()
				? StartPosition + length
				: length;
			mFooterPosition = TypeExtensions.kNoneInt64;
			UnderlyingStream.StreamMode = FileAccess.Write;

			if(EndPosition.IsNotNone())
				OpenVerifyAssumedStreamSize(ref result);

			return result;
		}
		public BlobChunkVerificationResultInfo Open(Stream baseStream,
			long startPosition = 0, long length = TypeExtensions.kNoneInt64,
			FileAccess permissions = FileAccess.ReadWrite, Shell.EndianFormat endian = Shell.EndianFormat.Big)
		{
			Contract.Requires(IsClosed);
			Contract.Requires<ArgumentNullException>(baseStream != null);
			Contract.Requires<ArgumentException>(baseStream.CanSeek);
			Contract.Requires<ArgumentException>(permissions != 0, "Why do we have NO permissions?");
			Contract.Requires<ArgumentException>(baseStream.HasPermissions(permissions));
			Contract.Requires(length.IsNoneOrPositive());
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
			Contract.Requires(IsClosed);
			Contract.Requires<ArgumentNullException>(baseStream != null);
			Contract.Requires<ArgumentException>(baseStream.CanSeek);
			Contract.Requires<ArgumentException>(permissions != 0, "Why do we have NO permissions?");
			Contract.Requires<ArgumentException>(baseStream.HasPermissions(permissions));
			Contract.Requires(endPosition.IsNoneOrPositive());
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
			if (UnderlyingStream != null)
			{
				UnderlyingStream.Dispose();
				UnderlyingStream = null;
			}
		}
		#endregion

		byte[] BuildAuthenticationData()
		{
			Contract.Ensures(Contract.Result<byte[]>() != null ||
				mFooter.Authentication == BlobTransportStreamAuthentication.None);
			Contract.Assert(mFooterPosition.IsNotNone());

			System.Security.Cryptography.HashAlgorithm hash_algo = null;
			switch (mFooter.Authentication)
			{
				case BlobTransportStreamAuthentication.None: break;
				// TODO:
				case BlobTransportStreamAuthentication.Crc: throw new NotImplementedException();
				case BlobTransportStreamAuthentication.Hash: throw new NotImplementedException();
				case BlobTransportStreamAuthentication.Rsa: throw new NotImplementedException();
			}

			if (hash_algo != null)
				return hash_algo.ComputeHash(BaseStream, StartPosition, (int)AssumedBlobSize);

			return null;
		}

		#region Verification
		BlobChunkVerificationResultInfo VerifyEnoughBytesForChunkOrData(long totalSize)
		{
			var result = BlobChunkVerificationResultInfo.ValidResult;
			if (UnderlyingStream.IsWriting && EndPosition.IsNone())
				return result;

			long bytes_remaining = BaseStream.BytesRemaining(EndPosition);
			if (bytes_remaining < totalSize)
				result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.EndOfStream,
					BlobChunkVerificationResultContext.Stream, bytes_remaining);

			return result;
		}
		BlobChunkVerificationResultInfo VerifyStart(bool streamFirst = true)
		{
			var result = streamFirst ? VerifyEnoughBytesForChunkOrData(kHeaderSize) : BlobChunkVerificationResultInfo.ValidResult;

			if (result.IsValid)
			{
				if (streamFirst)
					UnderlyingStream.Stream(ref mHeader);

				bool requires_byteswap;
				result = mHeader.Verify(out requires_byteswap);

				if (result.IsValid && requires_byteswap)
					UnderlyingStream.ChangeByteOrder(UnderlyingStream.ByteOrder.Invert());
			}

			if (result.IsInvalid)
				result.Context = BlobChunkVerificationResultContext.Header;

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
					mFooterPosition = BaseStream.Position;
					UnderlyingStream.Stream(ref mHeader);
				}
				long blob_size = AssumedBlobSize;

				result = mFooter.Verify(expectedAuthentication, blob_size);

				if (streamFirst && result.IsValid)
					mFooter.SerializeAuthenticationData(UnderlyingStream);
			}

			if (result.IsInvalid)
				result.Context = BlobChunkVerificationResultContext.Footer;

			return result;
		}
		BlobChunkVerificationResultInfo VerifyEofAuthentication()
		{
			var result = BlobChunkVerificationResultInfo.ValidResult;

			byte[] hash = BuildAuthenticationData();

			if (hash != null)
			{
				bool hashes_equal = hash.EqualsArray(mFooter.AuthenticationData);
				if (!hashes_equal)
					result = new BlobChunkVerificationResultInfo(BlobChunkVerificationResult.AuthenticationFailed,
						BlobChunkVerificationResultContext.Stream);
			}

			return result;
		}
		#endregion

		/*public*/ bool TryAndFind(BlobChunkHeader signature, long findStartPosition = TypeExtensions.kNone)
		{
			Contract.Requires<ArgumentOutOfRangeException>(findStartPosition.IsNone() ||
				findStartPosition < AssumedBlobSize);

			bool blob_found = false;
			long orig_pos = TypeExtensions.kNone;

			if (findStartPosition.IsNotNone())
			{
				orig_pos = UnderlyingStream.BaseStream.Position;
				UnderlyingStream.Seek(findStartPosition + StartPosition, System.IO.SeekOrigin.Begin);
			}

			// TODO
			Contract.Assert(false);

			if (orig_pos.IsNotNone())
			{
				UnderlyingStream.Seek(orig_pos, System.IO.SeekOrigin.Begin);
			}

			return blob_found;
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
				UnderlyingStream.Stream(ref header);
				result = header	.VerifyVersionIsPositive()
								.And(header, h => h.VerifyDataSize(0))
								.And(header, h => h.VerifyFlagsIsPostive())
								.And(header, this, (h, s) => s.VerifyEnoughBytesForChunkOrData(h.DataSize));

				isEof = header.Signature == StreamFooter.kSignature.ID;

				if (result.IsValid)
				{
					if (isEof)
					{
						mFooterPosition = BaseStream.Position - BlobChunkHeader.kSizeOf;
						mFooter.SerializeSansHeader(UnderlyingStream, header);
						mFooter.SerializeAuthenticationData(UnderlyingStream);
					}
					else
					{
						resultValue = getResultValue(this, header);

						if (!getResultValueConsumesChunk)
							header.StreamSkipData(BaseStream);
					}
				}
				else
				{
					if(isEof)
						result.Context = BlobChunkVerificationResultContext.Footer;
				}
			}
			return result;
		}
		BlobChunkVerificationResultInfo EnumerateStream<T>(out IList<KeyValuePair<BlobChunkHeader, T>> results,
			BlobTransportStreamAuthentication expectedAuthentication,
			Func<BlobTransportStream, BlobChunkHeader, T> getResultValue, bool getResultValueConsumesChunk = false)
		{
			results = new List<KeyValuePair<BlobChunkHeader, T>>();
			var result_info = BlobChunkVerificationResultInfo.ValidResult;

			result_info = VerifyStart();
			BlobChunkHeader chunk_header;
			bool is_eof = false;
			while (result_info.IsValid)
			{
				T result_value = default(T);
				result_info = EnumerateOneChunk(out chunk_header, ref result_value,
					getResultValue, getResultValueConsumesChunk, out is_eof);

				if (result_info.IsValid && !is_eof)
					results.Add(new KeyValuePair<BlobChunkHeader, T>(chunk_header, result_value));
				else
					break;
			}

			if (result_info.IsValid && is_eof)
				result_info = VerifyEof(streamFirst: false, expectedAuthentication: expectedAuthentication);

			return result_info;
		}

		static long GetEnumerateStreamResultDataPosition(BlobTransportStream @this, BlobChunkHeader header)
		{
			return @this.BaseStream.Position; // position at this point will be right after [header]
		}
		static byte[] GetEnumerateStreamResultBytes(BlobTransportStream @this, BlobChunkHeader header)
		{
			byte[] bytes = new byte[header.DataSize];
			@this.UnderlyingStream.Stream(bytes);

			return bytes;
		}
		#endregion

		#region EnumerateChunks
		BlobObject EnumerateChunksReadObject(BlobSystem blobSystem, BlobGroup blobGroup,
			BlobChunkHeader header, byte[] data)
		{
			Contract.Requires<InvalidOperationException>(!GameTarget.IsNone);

			var obj = blobSystem.CreateObject(GameTarget, blobGroup, header.Version, header.Size);

			using (var ms = new System.IO.MemoryStream(data))
			using (var es = new IO.EndianStream(ms, UnderlyingStream.ByteOrder, this, blobGroup.GroupTag.Name, FileAccess.Read))
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
			throw new InvalidOperationException(string.Format(
				"Build incompatibility for chunk {0} v{1} sizeof({2}) which uses build={3} " +
				"but we're using build={4} for {5}",
				blobGroup.GroupTag.TagString, header.Version, header.DataSize, buildForBlobVersion.ToDisplayString(),
				actualBuild.ToDisplayString(), UnderlyingStream.StreamName));
		}
		void EnumerateChunksReadObjectFoundUnknownChunk(BlobSystem blobSystem,
			BlobChunkHeader header, bool throwOnUnhandledChunk)
		{
			const string kUnhandledChunkMessageFormat = " chunk {0} v{1} sizeof({2}) for target={3}";

			string tag_string = new string(Values.GroupTagData32.FromUInt(header.Signature));

			if (throwOnUnhandledChunk)
				throw new InvalidDataException(string.Format("Unhandled" + kUnhandledChunkMessageFormat,
					tag_string, header.Version, header.DataSize, GameTarget.ToDisplayString()));

			Debug.Trace.Blob.TraceInformation("Ignoring" + kUnhandledChunkMessageFormat,
				tag_string, header.Version, header.DataSize, GameTarget.ToDisplayString());
		}
		Task<BlobObject>[] EnumerateChunksReadObjectsAsync(BlobSystem blobSystem,
			bool throwOnUnhandledChunk, IList<KeyValuePair<BlobChunkHeader, byte[]>> chunks)
		{
			var task_list = new List<Task<BlobObject>>(chunks.Count);

			foreach (var kv in chunks)
			{
				var header = kv.Key;
				BlobGroup blob_group;
				Engine.EngineBuildHandle build_for_version;
				if (blobSystem.TryGetBlobGroup(header.Signature, header.DataSize, header.Version, out blob_group, out build_for_version))
				{
					if (!build_for_version.IsWithinSameBranch(GameTarget.Build))
						EnumerateChunksReadObjectFoundBuildIncompatibility(blobSystem, header, blob_group,
							build_for_version, GameTarget.Build);

					var task = Task<BlobObject>.Factory.StartNew(s =>
						(s as BlobTransportStream).EnumerateChunksReadObject(blobSystem, blob_group, header, kv.Value),
						this);
					task_list.Add(task);
				}
				else
					EnumerateChunksReadObjectFoundUnknownChunk(blobSystem, header, throwOnUnhandledChunk);
			}

			return task_list.ToArray();
		}
		IEnumerable<BlobObject> EnumerateChunksReadObjectsSync(BlobSystem blobSystem,
			bool throwOnUnhandledChunk, IList<KeyValuePair<BlobChunkHeader, byte[]>> chunks)
		{
			var results = new List<BlobObject>(chunks.Count);

			foreach (var kv in chunks)
			{
				var header = kv.Key;
				BlobGroup blob_group;
				Engine.EngineBuildHandle build_for_version;
				if (blobSystem.TryGetBlobGroup(header.Signature, header.DataSize, header.Version, out blob_group, out build_for_version))
				{
					if (!build_for_version.IsWithinSameBranch(GameTarget.Build))
						EnumerateChunksReadObjectFoundBuildIncompatibility(blobSystem, header, blob_group,
							build_for_version, GameTarget.Build);

					var obj = EnumerateChunksReadObject(blobSystem, blob_group, header, kv.Value);
					results.Add(obj);
				}
				else
					EnumerateChunksReadObjectFoundUnknownChunk(blobSystem, header, throwOnUnhandledChunk);
			}

			return results;
		}

		public async Task<KeyValuePair<BlobChunkVerificationResultInfo, IEnumerable<BlobObject>>> EnumerateChunksAsync(
			BlobSystem blobSystem,
			bool throwOnUnhandledChunk = true,
			BlobTransportStreamAuthentication expectedAuthentication = BlobTransportStreamAuthentication.None,
			bool authenticateBlob = true)
		{
			Contract.Requires<ArgumentNullException>(blobSystem != null);
			Contract.Requires<InvalidOperationException>(!IsClosed);
			Contract.Requires<InvalidOperationException>(UnderlyingStream.CanRead);

			IEnumerable<BlobObject> objects = null;

			IList<KeyValuePair<BlobChunkHeader, byte[]>> chunks;
			var result_info = EnumerateStream(out chunks, expectedAuthentication,
				GetEnumerateStreamResultBytes, getResultValueConsumesChunk: true);

			if (result_info.IsValid &&
				mFooter.Authentication > BlobTransportStreamAuthentication.None && authenticateBlob)
				result_info = VerifyEofAuthentication();

			if (result_info.IsValid)
			{
				var tasks = EnumerateChunksReadObjectsAsync(blobSystem, throwOnUnhandledChunk, chunks);
				await Task.WhenAll(tasks);
				objects = from task in tasks
						  select task.Result;
			}

			return new KeyValuePair<BlobChunkVerificationResultInfo, IEnumerable<BlobObject>>(result_info, objects);
		}
		public BlobChunkVerificationResultInfo EnumerateChunks(BlobSystem blobSystem,
			out IEnumerable<BlobObject> objects, bool throwOnUnhandledChunk = true,
			BlobTransportStreamAuthentication expectedAuthentication = BlobTransportStreamAuthentication.None,
			bool authenticateBlob = true)
		{
			Contract.Requires<ArgumentNullException>(blobSystem != null);
			Contract.Requires<InvalidOperationException>(!IsClosed);
			Contract.Requires<InvalidOperationException>(UnderlyingStream.CanRead);

			objects = null;

			IList<KeyValuePair<BlobChunkHeader, byte[]>> chunks;
			var result_info = EnumerateStream(out chunks, expectedAuthentication,
				GetEnumerateStreamResultBytes, getResultValueConsumesChunk: true);

			if (result_info.IsValid &&
				mFooter.Authentication > BlobTransportStreamAuthentication.None && authenticateBlob)
				result_info = VerifyEofAuthentication();

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
				mHeader.Serialize(UnderlyingStream);
			}

			return result;
		}
		BlobChunkVerificationResultInfo WriteChunksWriteEof(BlobTransportStreamAuthentication authentication)
		{
			mFooterPosition = BaseStream.Position;
			mFooter = new StreamFooter(authentication, AssumedBlobSize);

			var result = VerifyEnoughBytesForChunkOrData(kFooterSize + authentication.GetDataSize());

			if (result.IsValid)
			{
				byte[] auth_data = BuildAuthenticationData();

				if (auth_data != null)
				{
					Contract.Assert(mFooter.AuthenticationData.Length >= auth_data.Length);
					Buffer.BlockCopy(auth_data, 0, mFooter.AuthenticationData, 0, auth_data.Length);
				}

				mFooter.Serialize(UnderlyingStream);
				mFooter.SerializeAuthenticationData(UnderlyingStream);
			}

			return result;
		}
		BlobChunkVerificationResultInfo WriteChunksToUnderlyingStream(IEnumerable<KeyValuePair<BlobChunkHeader, byte[]>> chunks)
		{
			var result_info = BlobChunkVerificationResultInfo.ValidResult;

			foreach (var kv in chunks)
			{
				var header = kv.Key;
				result_info = VerifyEnoughBytesForChunkOrData(header.Size);
				if (result_info.IsInvalid)
					break;

				header.Serialize(UnderlyingStream);
				UnderlyingStream.Writer.Write(kv.Value);
			}

			return result_info;
		}

		KeyValuePair<BlobChunkHeader, byte[]> WriteChunksProcessObject(BlobObject obj)
		{
			byte[] data;

			var sys_group = obj.SystemGroup;
			// TODO: support non-fixed length blobs like film streams
			using (var ms = new System.IO.MemoryStream(obj.CalculateFixedBinarySize(this.GameTarget)))
			using (var es = new IO.EndianStream(ms, UnderlyingStream.ByteOrder, this, sys_group.GroupTag.Name, FileAccess.Write))
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
				tasks[x] = Task<KeyValuePair<BlobChunkHeader, byte[]>>.Factory.StartNew(
					obj => WriteChunksProcessObject((BlobObject)obj),
					objects[x]);

			return tasks;
		}
		async Task<BlobChunkVerificationResultInfo> WriteChunksForObjectsAsync(BlobObject[] objects)
		{
			var result_info = BlobChunkVerificationResultInfo.ValidResult;

			var tasks = WriteChunksProcessObjectsAsync(objects);
			await Task.WhenAll(tasks);
			var chunks= from task in tasks
						select task.Result;

			result_info = WriteChunksToUnderlyingStream(chunks);

			return result_info;
		}

		BlobChunkVerificationResultInfo WriteChunksForObjectsSync(BlobObject[] objects)
		{
			var result_info = BlobChunkVerificationResultInfo.ValidResult;

			var chunks = new KeyValuePair<BlobChunkHeader, byte[]>[objects.Length];
			for (int x = 0; x < objects.Length; x++)
				chunks[x] = WriteChunksProcessObject(objects[x]);

			result_info = WriteChunksToUnderlyingStream(chunks);

			return result_info;
		}

		BlobObject[] WriteChunksGetObjectsArray(IEnumerable<BlobObject> objects)
		{
			var array = objects is BlobObject[]
				? (BlobObject[])objects
				: objects.ToArray();

			if (array.Length < 1)
				throw new InvalidOperationException("Need at least one blob object");
			else if (!Array.TrueForAll(array, Predicates.IsNotNull))
				throw new ArgumentNullException("Blob object in enumeration was null");

			return array;
		}
		public async Task<BlobChunkVerificationResultInfo> WriteChunksAsync(IEnumerable<BlobObject> objects,
			BlobTransportStreamAuthentication authentication = BlobTransportStreamAuthentication.None)
		{
			Contract.Requires<InvalidOperationException>(!IsClosed);
			Contract.Requires<InvalidOperationException>(UnderlyingStream.CanWrite);

			var result_info = WriteChunksWriteStart();

			if (result_info.IsValid)
			{
				var array = WriteChunksGetObjectsArray(objects);
				result_info = await WriteChunksForObjectsAsync(array);
			}

			if (result_info.IsValid)
				result_info = WriteChunksWriteEof(authentication);

			return result_info;
		}
		public BlobChunkVerificationResultInfo WriteChunks(IEnumerable<BlobObject> objects,
			BlobTransportStreamAuthentication authentication = BlobTransportStreamAuthentication.None)
		{
			Contract.Requires<InvalidOperationException>(!IsClosed);
			Contract.Requires<InvalidOperationException>(UnderlyingStream.CanWrite);

			var result_info = WriteChunksWriteStart();

			if (result_info.IsValid)
			{
				var array = WriteChunksGetObjectsArray(objects);
				result_info = WriteChunksForObjectsSync(array);
			}

			if (result_info.IsValid)
				result_info = WriteChunksWriteEof(authentication);

			return result_info;
		}

		public async Task<BlobChunkVerificationResultInfo> WriteChunksSansAuthenticationAsync(params BlobObject[] objects)
		{
			Contract.Requires<InvalidOperationException>(!IsClosed);
			Contract.Requires<InvalidOperationException>(UnderlyingStream.CanWrite);
			Contract.Requires(objects != null && objects.Length > 0);
			Contract.Requires<ArgumentNullException>(Array.TrueForAll(objects, Predicates.IsNotNull));

			return await WriteChunksAsync(objects, BlobTransportStreamAuthentication.None);
		}
		public BlobChunkVerificationResultInfo WriteChunksSansAuthentication(params BlobObject[] objects)
		{
			Contract.Requires<InvalidOperationException>(!IsClosed);
			Contract.Requires<InvalidOperationException>(UnderlyingStream.CanWrite);
			Contract.Requires(objects != null && objects.Length > 0);
			Contract.Requires<ArgumentNullException>(Array.TrueForAll(objects, Predicates.IsNotNull));

			return WriteChunks(objects, BlobTransportStreamAuthentication.None);
		}

		public async Task<BlobChunkVerificationResultInfo> WriteChunksWithAuthenticationAsync(BlobTransportStreamAuthentication authentication,
			params BlobObject[] objects)
		{
			Contract.Requires<InvalidOperationException>(!IsClosed);
			Contract.Requires<InvalidOperationException>(UnderlyingStream.CanWrite);
			Contract.Requires(objects != null && objects.Length > 0);
			Contract.Requires<ArgumentNullException>(Array.TrueForAll(objects, Predicates.IsNotNull));

			return await WriteChunksAsync(objects, authentication);
		}
		public BlobChunkVerificationResultInfo WriteChunksWithAuthentication(BlobTransportStreamAuthentication authentication,
			params BlobObject[] objects)
		{
			Contract.Requires<InvalidOperationException>(!IsClosed);
			Contract.Requires<InvalidOperationException>(UnderlyingStream.CanWrite);
			Contract.Requires(objects != null && objects.Length > 0);
			Contract.Requires<ArgumentNullException>(Array.TrueForAll(objects, Predicates.IsNotNull));

			return WriteChunks(objects, authentication);
		}
		#endregion
	};
}