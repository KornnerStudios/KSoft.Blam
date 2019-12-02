using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Proto
{
	/// <summary>
	/// Type parameter data for <see cref="MegaloScriptValueBaseType.Enum"/> and
	/// <see cref="MegaloScriptValueBaseType.Flags"/>
	/// </summary>
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("Name = {Name}, Count = {Members.Count}")]
	public sealed class MegaloScriptEnum
		: IO.ITagElementStringNameStreamable
	{
		public string Name;

		public List<string> Members { get; private set; }
		public Dictionary<string, int> NameToIndex { get; private set; }
		/// <summary>True when we export an Enum defined in the code to the data-driven script model</summary>
		public bool IsCodeEnum { get; private set; }

		#region Ctor
		public MegaloScriptEnum()
		{
			Members = new List<string>();
			NameToIndex = new Dictionary<string, int>();
		}

		void ForEnumPopulateMembers<TEnum>(string[] names, TEnum[] values)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			int index = 0;
			int length = names.Length;
			#region Handle None values
			int last_value = Reflection.EnumUtil<TEnum>.GetHashCodeSignExtended(values[length - 1]);
			Contract.Assert(last_value.IsNoneOrPositive());
			// Enum.GetNames() sorts values by ToUInt64'ing, meaning negative values will appear after positive ones
			if (last_value.IsNone())
			{
				length--;
				Members.Add(names[length]);
				NameToIndex.Add(names[length], index);
			}
			#endregion

			for (; index < length; index++)
			{
				Members.Add(names[index]);
				NameToIndex.Add(names[index], index);
			}
		}
		internal static MegaloScriptEnum ForEnum<TEnum>(string name)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			var result = new MegaloScriptEnum {
				Name = name,
				IsCodeEnum = true
			};
			result.ForEnumPopulateMembers(Reflection.EnumUtil<TEnum>.Names, Reflection.EnumUtil<TEnum>.Values);

			return result;
		}
		#endregion

		internal int ValidBitLengthForFlags(int bitLength)
		{
			int required_bit_count = Members.Count;

			return bitLength - required_bit_count;
		}
		internal int ValidBitLengthForEnum(int bitLength, MegaloScriptValueEnumTraits traits)
		{
			uint max_value = (uint)Members.Count;
			if (traits == MegaloScriptValueEnumTraits.HasNoneMember)
				max_value++;
			int required_bit_count = Bits.GetMaxEnumBits(max_value);

			return bitLength - required_bit_count;
		}

		#region ITagElementStringNameStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("name", ref Name);
			s.StreamElements("Member", Members);

			if (s.IsReading)
			{
				int index = 0;
				foreach (string name in Members)
					NameToIndex.Add(name, index++);
			}

			Contract.Assert(!IsCodeEnum);
		}
		#endregion

		#region EnumNameResolvingContext
		internal static int FromMemberName(MegaloScriptDatabase db, MegaloScriptValueType enumValueType, string name)
		{
			var e = db.Enums[enumValueType.EnumIndex];
			int index = 0;
			if(e.NameToIndex.TryGetValue(name, out index))
				return enumValueType.EnumTraits != Proto.MegaloScriptValueEnumTraits.HasNoneMember ? index : index-1;
			else
				throw new KeyNotFoundException(string.Format("'{0}' is not a valid member name in {1}",
					name, e.Name));
		}
		internal static string ToMemberName(MegaloScriptDatabase db, MegaloScriptValueType enumValueType, int index)
		{
			var e = db.Enums[enumValueType.EnumIndex];
			if (enumValueType.EnumTraits == Proto.MegaloScriptValueEnumTraits.HasNoneMember)
				index += 1;
			return e.Members[index];
		}
		internal struct EnumNameResolvingContext
		{
			readonly MegaloScriptDatabase Db;
			readonly MegaloScriptValueType ValueType;

			public EnumNameResolvingContext(Proto.MegaloScriptDatabase db, MegaloScriptValueType valueType)
			{ Db = db; ValueType = valueType; }

			public static readonly Func<EnumNameResolvingContext, string, int> IdResolver =
				(ctxt, name) => FromMemberName(ctxt.Db, ctxt.ValueType, name);
			public static readonly Func<EnumNameResolvingContext, int, string> NameResolver =
				(ctxt, id) => ToMemberName(ctxt.Db, ctxt.ValueType, id);
		};
		#endregion
		#region FlagsNameResolvingContext
		static readonly char[] kFlagsSeperator = {',', ' '};

		internal static uint FromFlagsName(MegaloScriptDatabase db, MegaloScriptValueType flagsValueType, string names)
		{
			if (string.IsNullOrEmpty(names) || names == "0")
				return 0;

			var e = db.Enums[flagsValueType.EnumIndex];
			uint flags = 0;

			string[] parts = names.Split(kFlagsSeperator, StringSplitOptions.RemoveEmptyEntries);
			for(int x = 0; x < parts.Length; x++)
			{
				int bit;
				if (e.NameToIndex.TryGetValue(parts[x], out bit))
					flags |= 1U << bit;
				else
					throw new KeyNotFoundException(string.Format("'{0}' is not a valid bit name in {1}",
						parts[x], e.Name));
			}

			return flags;
		}
		[System.Diagnostics.Conditional("CONTRACTS_FULL")]
		static void ToFlagsNameValidateFlags(MegaloScriptEnum e, uint flags)
		{
			uint bitmask = Bits.GetBitmaskEnum((uint)e.Members.Count);
			Contract.Assert(Bitwise.Flags.Test(flags, ~bitmask)==false);
		}
		internal static string ToFlagsName(MegaloScriptDatabase db, MegaloScriptValueType flagsValueType, uint flags)
		{
			if (flags == 0)
				return "0";

			var e = db.Enums[flagsValueType.EnumIndex];
			ToFlagsNameValidateFlags(e, flags);

			var sb = new System.Text.StringBuilder();
			bool first_flag = true;
			for (int x = 0; x < e.Members.Count; x++)
			{
				if (!Bitwise.Flags.Test(flags, 1U << x))
					continue;

				if (!first_flag)
					sb.Append(kFlagsSeperator);

				sb.Append(e.Members[x]);
				first_flag = false;
			}

			return sb.ToString();
		}
		internal struct FlagsNameResolvingContext
		{
			readonly MegaloScriptDatabase Db;
			readonly MegaloScriptValueType ValueType;

			public FlagsNameResolvingContext(Proto.MegaloScriptDatabase db, MegaloScriptValueType valueType)
			{ Db = db; ValueType = valueType; }

			public static readonly Func<FlagsNameResolvingContext, string, uint> IdResolver =
				(ctxt, name) => FromFlagsName(ctxt.Db, ctxt.ValueType, name);
			public static readonly Func<FlagsNameResolvingContext, uint, string> NameResolver =
				(ctxt, id) => ToFlagsName(ctxt.Db, ctxt.ValueType, id);
		};
		#endregion
	};
}