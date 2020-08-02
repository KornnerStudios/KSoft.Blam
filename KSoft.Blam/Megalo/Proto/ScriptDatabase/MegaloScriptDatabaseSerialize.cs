using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Proto
{
	partial class MegaloScriptDatabase
	{
		/// <summary>Name of the value type to use when there's no value type specified</summary>
		const string kNoValueTypeName = "None";
		static bool ValueTypeNameIsNotNone(string name)
		{
			return !string.IsNullOrEmpty(name) && name != kNoValueTypeName;
		}

		internal bool SerializeValueTypeReference<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref MegaloScriptValueType type, bool isOptional = false)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = true;
			bool reading = s.IsReading;

			string type_name = reading ? null : ValueTypeNames[type.NameIndex];
			if (isOptional)
			{
				streamed = s.StreamAttributeOpt(attributeName, ref type_name, ValueTypeNameIsNotNone);

				if (!streamed && reading)
					type_name = kNoValueTypeName;
			}
			else
				s.StreamAttribute(attributeName, ref type_name);

			if (reading)
				type = GetValueType(type_name);

			return streamed;
		}

		#region SerializeEnumTypeReference
		static readonly Func<MegaloScriptDatabase, string, int> kEnumIdResolver =
			(_db, name) => _db.Enums.FindLastIndex(x => name.Equals(x.Name, StringComparison.Ordinal)); // #NOTE_BLAM: we use FindLastIndex so that it is possible to override builtin enums
		static readonly Func<MegaloScriptDatabase, int, string> kEnumNameResolver =
			(_db, id) => id.IsNotNone() ? _db.Enums[id].Name : null;
		internal bool SerializeEnumTypeReference<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref int enumIndex, bool isOptional = false)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = true;

			if (isOptional)
			{
				streamed = s.StreamAttributeOptIdAsString(attributeName, ref enumIndex, this,
					kEnumIdResolver, kEnumNameResolver, Predicates.IsNotNullOrEmpty);

				if (!streamed && s.IsReading)
					enumIndex = TypeExtensions.kNone;
			}
			else
				s.StreamAttributeIdAsString(attributeName, ref enumIndex, this, kEnumIdResolver, kEnumNameResolver);

			return streamed;
		}
		#endregion

		#region SerializeActionTemplateReference
		static readonly Func<MegaloScriptDatabase, string, MegaloScriptProtoActionTemplate> kActionTemplateResolver =
			(_db, name) => _db.ActionTemplates.Find(x => name.Equals(x.Name, StringComparison.Ordinal));
		static readonly Func<MegaloScriptDatabase, MegaloScriptProtoActionTemplate, string> kActionTemplateNameResolver =
			(_db, id) => id != null ? id.Name : null;
		internal bool SerializeActionTemplateReference<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref MegaloScriptProtoActionTemplate template)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = s.StreamAttributeOptIdAsString(attributeName, ref template, this,
				kActionTemplateResolver, kActionTemplateNameResolver, Predicates.IsNotNullOrEmpty);

			if (!streamed && s.IsReading)
				template = null;

			return streamed;
		}
		#endregion

		#region SerializeProtoActionReference
		static readonly Func<MegaloScriptDatabase, string, IMegaloScriptProtoAction> kProtoActionResolver =
			(_db, name) => (IMegaloScriptProtoAction)
				_db.ActionTemplates.Find(x => name.Equals(x.Name, StringComparison.Ordinal)) ?? _db.Actions.Find(x => name.Equals(x.Name, StringComparison.Ordinal))
			;
		static readonly Func<MegaloScriptDatabase, IMegaloScriptProtoAction, string> kProtoActionNameResolver =
			(_db, id) => id != null ? id.Name : null;
		internal bool SerializeProtoActionReference<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string attributeName, ref IMegaloScriptProtoAction action)
			where TDoc : class
			where TCursor : class
		{
			bool streamed = s.StreamAttributeOptIdAsString(attributeName, ref action, this,
				kProtoActionResolver, kProtoActionNameResolver, Predicates.IsNotNullOrEmpty);

			if (!streamed && s.IsReading)
				action = null;

			return streamed;
		}
		#endregion

		static void SerializeValueType<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			MegaloScriptDatabase db, ref MegaloScriptValueType value)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			var base_type = reading ? MegaloScriptValueBaseType.None : value.BaseType;
			string name = reading ? null : db.ValueTypeNames[value.NameIndex];
			int bit_length = reading ? 0 : value.BitLength;

			s.StreamAttributeEnum("baseType", ref base_type);
			s.StreamAttribute("name", ref name);
			bool has_bit_length = s.StreamAttributeOpt("bitLength", ref bit_length, Predicates.IsNotZero);

			int name_index = reading ? db.ValueTypeNames.Count : -1;
			if (reading) db.ValueTypeNames.Add(name);

			switch (base_type)
			{
				#region MegaloScriptValueBaseType.None
				case MegaloScriptValueBaseType.None:
					if (reading)
					{
						Contract.Assert(name_index == 0, "There should only be one None value type and it should come first");
						Contract.Assert(db.ValueTypeNames[name_index] == kNoValueTypeName, "None value type is using non-standard name");
						value = new MegaloScriptValueType(name_index, base_type, 0);
					}
					break;
				#endregion
				#region MegaloScriptValueBaseType.Single
				case MegaloScriptValueBaseType.Single:
					{
						int encoding_index = reading ? 0 : value.EncodingIndex;

						s.StreamAttributeIdAsString("encoding", ref encoding_index, db,
							(_db, str) => _db.SingleEncodings.FindIndex((x) => str.Equals(x.Name, StringComparison.Ordinal)),
							(_db, id) => _db.SingleEncodings[id].Name);

						if (reading) // #NOTE_BLAM: requires -1 when reading the TypeParam as the encoding index
							value = new MegaloScriptValueType(name_index, base_type, bit_length, (uint)(encoding_index+1));
					} break;
				#endregion
				#region MegaloScriptValueBaseType.Point3d
				case MegaloScriptValueBaseType.Point3d:
					{
						if (!has_bit_length)
						{
							throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
								"Point3d {0} didn't define a bit-length",
								name));
						}

						bool is_signed = reading ? false : value.PointIsSigned;
						s.StreamAttributeOpt("signed", ref is_signed, Predicates.IsTrue);

						if (reading)
						{
							value = new MegaloScriptValueType(name_index, base_type, bit_length,
								0, is_signed ? 1U : 0U);
						}
					} break;
				#endregion
				#region MegaloScriptValueBaseType.Flags
				case MegaloScriptValueBaseType.Flags:
					{
						int enum_index = reading ? 0 : value.EnumIndex;
						MegaloScriptValueEnumTraits enum_traits = reading ? 0 : value.EnumTraits;

						db.SerializeEnumTypeReference(s, "enumType", ref enum_index);

						if (reading)
						{
							if (!has_bit_length)
								bit_length = db.Enums[enum_index].Members.Count;
							value = new MegaloScriptValueType(name_index, base_type, bit_length, (uint)enum_index);
						}
					} break;
				#endregion
				#region MegaloScriptValueBaseType.Enum
				case MegaloScriptValueBaseType.Enum:
					{
						int enum_index = reading ? 0 : value.EnumIndex;
						MegaloScriptValueEnumTraits enum_traits = reading ? 0 : value.EnumTraits;

						db.SerializeEnumTypeReference(s, "enumType", ref enum_index);
						s.StreamAttributeEnumOpt("enumTraits", ref enum_traits, e => e != MegaloScriptValueEnumTraits.None);

						if (reading)
						{
							if (!has_bit_length)
							{
								int max_value = db.Enums[enum_index].Members.Count;
								if (enum_traits == MegaloScriptValueEnumTraits.HasNoneMember)
									max_value++;
								bit_length = Bits.GetMaxEnumBits(max_value);
							}
							value = new MegaloScriptValueType(name_index, bit_length, enum_index, enum_traits);
						}
					} break;
				#endregion
				#region MegaloScriptValueBaseType.Index
				case MegaloScriptValueBaseType.Index:
					{
						MegaloScriptValueIndexTarget index = reading ? MegaloScriptValueIndexTarget.Undefined : value.IndexTarget;
						MegaloScriptValueIndexTraits index_traits = reading ? 0 : value.IndexTraits;

						s.StreamAttributeEnum("indexTarget", ref index);
						s.StreamAttributeEnum("indexTraits", ref index_traits);

						if (reading)
						{
							// #NOTE_BLAM: If we for whatever reason wrote the DB back out, types with implicit bit lengths
							// would have that length written where they didn't before
							if (!has_bit_length)
								bit_length = db.Limits.GetIndexTargetBitLength(index, index_traits);
							value = new MegaloScriptValueType(name_index, bit_length, index, index_traits);
						}
					} break;
				#endregion
				#region MegaloScriptValueBaseType.Var
				case MegaloScriptValueBaseType.Var:
					{
						MegaloScriptVariableType var_type = reading ? 0 : value.VarType;
						MegaloScriptVariableSet var_set = reading ? 0 : value.VarSet;

						s.StreamAttributeEnum("varType", ref var_type);
						s.StreamAttributeEnum("varSet", ref var_set);

						if (reading)
						{
							// #NOTE_BLAM: If we for whatever reason wrote the DB back out, types with implicit bit lengths
							// would have that length written where they didn't before
							if (!has_bit_length)
								bit_length = db.GetVariableIndexBitLength(var_set, var_type);
							value = new MegaloScriptValueType(name_index, bit_length, var_type, var_set);
						}
					} break;
				#endregion
				#region MegaloScriptValueBaseType.VarReference
				case MegaloScriptValueBaseType.VarReference:
					{
						MegaloScriptVarReferenceType var_ref = reading ? 0 : value.VarReference;

						s.StreamAttributeEnum("varReferenceType", ref var_ref);

						if (reading) value = new MegaloScriptValueType(name_index, var_ref);
					} break;
				#endregion
				#region MegaloScriptValueBaseType.Tokens
				case MegaloScriptValueBaseType.Tokens:
					{
						int max_tokens = reading ? 0 : value.MaxTokens;

						s.StreamAttribute("maxTokens", ref max_tokens);

						if (reading)
						{
							bit_length = Bits.GetMaxEnumBits(max_tokens+1);
							value = new MegaloScriptValueType(name_index, base_type, bit_length, (uint)max_tokens);
						}
					} break;
				#endregion
				#region MegaloScriptValueBaseType.ObjectReferenceWithPlayerVarIndex
				case MegaloScriptValueBaseType.ObjectReferenceWithPlayerVarIndex:
					{
						if (reading) value = new MegaloScriptValueType(name_index, MegaloScriptVarReferenceType.Object,
							MegaloScriptValueBaseType.ObjectReferenceWithPlayerVarIndex);
					} break;
				#endregion
				default:
					if (reading) value = new MegaloScriptValueType(name_index, base_type, bit_length);
					break;
			}
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterOwnerBookmark(this))
			{
				s.StreamAttribute("version", ref Version);

				using (s.EnterCursorBookmark("Limits"))
					Limits.Serialize(s);

				using (var bm = s.EnterCursorBookmarkOpt("SingleEncodings", SingleEncodings, Predicates.HasItems)) if (bm.IsNotNull)
					s.StreamableElements("Traits", SingleEncodings);

				using (s.EnterCursorBookmark("EnumTypes"))
					s.StreamableElements("Type", Enums, e => !e.IsCodeEnum);

				if (s.IsReading)
				{
					SingleEncodings.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
					Enums.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
				}

				using (s.EnterCursorBookmark("VariableSets"))
					s.StreamableElements("Set", VariableSets,
						"type", MegaloScriptProtoVariableSet.StreamSetType);
				using (s.EnterCursorBookmark("ValueTypes"))
					s.StreamElements("Type", ValueTypes, this, SerializeValueType);

				if (s.IsReading)
				{
					ValueTypes.ForEach(type => NameToValueType.Add(ValueTypeNames[type.NameIndex], type));
				}

				using (s.EnterCursorBookmark("VariableReferenceTypes"))
					s.StreamableElements("Type", VariableRefTypes,
						"type", MegaloScriptProtoVariableReference.StreamType);

				using (s.EnterCursorBookmark("Conditions"))
					s.StreamableElements("Condition", Conditions);
				using (s.EnterCursorBookmark("ActionTemplates"))
					s.StreamableElements("Template", ActionTemplates);
				using (s.EnterCursorBookmark("Actions"))
					s.StreamableElements("Action", Actions);

				if (s.IsReading)
				{
					mConditionTypeBitLength = Bits.GetMaxEnumBits(Conditions.Count);
					mActionTypeBitLength = Bits.GetMaxEnumBits(Actions.Count);

					Conditions.ForEach(cond => NameToConditionMap.Add(cond.Name, cond));
					Actions.ForEach(action => NameToActionMap.Add(action.Name, action));

					TeamDesignatorValueType = NameToValueType["TeamDesignator"];
					ObjectTypeIndexValueType = NameToValueType["ObjectTypeIndex"];

					ForEachAction = NameToActionMap["for_each"];
					if (Limits.SupportsVirtualTriggers)
						BeginAction = NameToActionMap["begin"];

					ObjectReferenceWithPlayerVarIndex.Initialize(this);
				}
			}
		}
	};
}
