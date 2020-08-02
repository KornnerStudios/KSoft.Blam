using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Localization.StringTables
{
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public partial class LocaleStringTable
		: IO.IBitStreamSerializable
		, IO.ITagElementStringNameStreamable
		, ICollection<LocaleStringTableReference>, System.Collections.ICollection
	{
		static readonly Memory.Strings.StringMemoryPoolSettings kStringPoolConfig =
			new Memory.Strings.StringMemoryPoolSettings(Memory.Strings.StringStorage.CStringUtf8,
				implicitNull:false, addressSize:Shell.ProcessorSize.x32);

		internal LocaleStringTableInfo kInfo;
		readonly GameLanguageTable mEngineLanguageTable;
		readonly List<LocaleStringTableReference> mStringReferences;

		public int Count { get { return mStringReferences.Count; } }
		public int Capacity { get { return kInfo.MaxCount; } }
		internal bool HasStrings { get { return mStringReferences.Count > 0; } }

		internal LocaleStringTable(LocaleStringTableInfo info, Engine.EngineBuildHandle buildHandle)
		{
			kInfo = info;
			mStringReferences = new List<LocaleStringTableReference>();
			mEngineLanguageTable = LanguageSystem.GetGameLanguageTable(buildHandle);
		}

		public LocaleStringTableReference this[int index] { get {
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Count);
			return mStringReferences[index];
		} }

		public void Clear()
		{
			mStringReferences.Clear();
			NotifyItemsInitialized();
		}

		int AddImpl(LocaleStringTableReference sref)
		{
			int index = Count;
			if (kInfo.CodeNameEntries && string.IsNullOrEmpty(sref.CodeName))
				sref.CodeName = "String" + index.ToString(Util.InvariantCultureInfo);

			mStringReferences.Add(sref);
			NotifyItemInserted(index, sref);
			return index;
		}

		internal int FindNameIndex(string codeName)
		{
			return this.mStringReferences.IndexOfByProperty(codeName,
				sref => sref.CodeName,
				x => string.Format(Util.InvariantCultureInfo,
					"Couldn't find a string named {0}", x));
		}

		#region IBitStreamSerializable Members
		void ReadStringsFromBuffer(LocaleStringTableBuffer stringData)
		{
			using (var ms = new System.IO.MemoryStream(stringData.Buffer))
			using (var er = new IO.EndianReader(ms, Shell.EndianFormat.Big))
			{
				foreach (var sref in mStringReferences)
					sref.ReadLanguageStrings(er);
			}
		}
		void WriteStringsToBuffer(LocaleStringTableBuffer stringData)
		{
			var pool = new Memory.Strings.StringMemoryPool(kStringPoolConfig);
			foreach (var sref in mStringReferences)
				sref.WriteLanguageString(pool);

			using (var ms = new System.IO.MemoryStream((int)pool.Size))
			using (var ew = new IO.EndianWriter(ms, Shell.EndianFormat.Big))
			{
				pool.WriteStrings(ew);
				stringData.Buffer = ms.ToArray();
			}

			if (stringData.Buffer.Length > kInfo.BufferMaxSize)
				throw new InvalidOperationException("Exceeded string table buffer size by (bytes): " +
					(stringData.Buffer.Length - kInfo.BufferMaxSize));
		}

		void ReferencesRead(IO.BitStream s, int count)
		{
			if (count > Capacity)
				throw new System.IO.InvalidDataException("String table reference count exceeded max by: " +
					(count - Capacity));

			for (uint x = 0; x < count; x++)
			{
				var sr = new LocaleStringTableReference(mEngineLanguageTable);
				sr.Serialize(s, kInfo.BufferOffsetBitLength);
				mStringReferences.Add(sr);
			}
		}
		void ReferencesWrite(IO.BitStream s)
		{
			for (int x = 0; x < mStringReferences.Count; x++)
				mStringReferences[x].Serialize(s, kInfo.BufferOffsetBitLength);
		}

		void Read(IO.BitStream s)
		{
			int count; s.Read(out count, kInfo.CountBitLength);
			ReferencesRead(s, count);
			if (HasStrings)
			{
				var string_data = new LocaleStringTableBuffer(this);
				s.StreamObject(string_data);
				ReadStringsFromBuffer(string_data);
			}
		}
		void Write(IO.BitStream s)
		{
			LocaleStringTableBuffer string_data = null;
			if (HasStrings)
			{
				string_data = new LocaleStringTableBuffer(this);
				WriteStringsToBuffer(string_data);
			}

			Contract.Assert(Count <= Capacity);
			s.Write(mStringReferences.Count, kInfo.CountBitLength);
			ReferencesWrite(s);

			if (HasStrings)
				s.StreamObject(string_data);
		}

		public void Serialize(IO.BitStream s)
		{
				 if (s.IsReading) Read(s);
			else if (s.IsWriting) Write(s);
		}
		#endregion

		#region ITagElementStringNameStreamable Members
		Exception SerializePostprocessCodeNames()
		{
			var names_set = new HashSet<string>();
			for (int x = 0; x < mStringReferences.Count; x++)
			{
				var sref = this[x];
				if (string.IsNullOrWhiteSpace(sref.CodeName))
				{
					return new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
						"Multilingual string #{0} has an invalid name",
						x));
				}

				if (names_set.Add(sref.CodeName)==false)
				{
					return new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
						"Multilingual string #{0} has a name already in use",
						x));
				}
			}

			return null;
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterOwnerBookmark(this))
			{
				s.StreamableElements("String", mStringReferences,
					this, _me => new LocaleStringTableReference(_me.mEngineLanguageTable));
			}

			if (Count > Capacity)
			{
				var ex = new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"Serialized multilingual string table with {0} strings, which exceeds its maximum of {1}",
					Count, kInfo.MaxCount));

				if (s.IsReading)
					s.ThrowReadException(ex);
				else if (s.IsWriting)
					throw ex;
			}

			if (s.IsReading)
			{
				if (kInfo.CodeNameEntries)
				{
					Exception code_names_ex = SerializePostprocessCodeNames();
					if (code_names_ex != null)
						s.ThrowReadException(code_names_ex);
				}
			}
		}

		internal int AddHack<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, string stringName)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires<InvalidOperationException>(Count < Capacity);
			Contract.Assert(s.IsReading);

			using (s.EnterOwnerBookmark(this))
			using (s.EnterCursorBookmark(stringName))
			{
				var sref = new LocaleStringTableReference(mEngineLanguageTable);
				sref.Serialize(s);

				return AddImpl(sref);
			}
		}
		#endregion

		#region IEnumerable<LocaleStringTableReference> Members
		public IEnumerator<LocaleStringTableReference> GetEnumerator()
		{
			return mStringReferences.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return mStringReferences.GetEnumerator();
		}
		#endregion

		#region ICollection<LocaleStringTableReference> Members
		void ICollection<LocaleStringTableReference>.Add(LocaleStringTableReference item)
		{
			AddImpl(item);
		}
		bool ICollection<LocaleStringTableReference>.Contains(LocaleStringTableReference item)
		{
			return mStringReferences.Contains(item);
		}
		void ICollection<LocaleStringTableReference>.CopyTo(LocaleStringTableReference[] array, int arrayIndex)
		{
			mStringReferences.CopyTo(array, arrayIndex);
		}
		bool ICollection<LocaleStringTableReference>.IsReadOnly
		{
			get { return false; }
		}
		bool ICollection<LocaleStringTableReference>.Remove(LocaleStringTableReference item)
		{
			throw new NotImplementedException();
		}

		void System.Collections.ICollection.CopyTo(Array array, int index)
		{
			(mStringReferences as System.Collections.ICollection).CopyTo(array, index);
		}
		bool System.Collections.ICollection.IsSynchronized	{ get { return false; } }
		object System.Collections.ICollection.SyncRoot		{ get { throw new NotImplementedException(); } }
		#endregion
	};
}
