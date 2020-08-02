#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Blam.Megalo.Model
{
	using Proto;

	// #TODO_BLAM: modify logic to support re-usable virtual triggers?
	[System.Reflection.Obfuscation(Exclude=false)]
	[System.Diagnostics.DebuggerDisplay("ID = {Id}, {VirtualTriggerHandle}")]
	public sealed partial class MegaloScriptVirtualTriggerValue
		: MegaloScriptValueBase
	{
		// Internal: the decompiled trigger in the model
		#region VirtualTriggerHandle
		MegaloScriptModelObjectHandle mVirtualTriggerHandle;
		public MegaloScriptModelObjectHandle VirtualTriggerHandle {
			get { return mVirtualTriggerHandle; }
			set { mVirtualTriggerHandle = value;
				NotifyPropertyChanged(kVirtualTriggerHandleChanged);
		} }
		#endregion

		public MegaloScriptVirtualTriggerValue(MegaloScriptValueType valueType) : base(valueType)
		{
			Contract.Requires(valueType.BaseType == MegaloScriptValueBaseType.VirtualTrigger);

			mVirtualTriggerHandle = MegaloScriptModelObjectHandle.Null;
		}

		public override MegaloScriptValueBase Copy(MegaloScriptModel model)
		{
			var result = (MegaloScriptVirtualTriggerValue)model.CreateValue(ValueType);
			result.VirtualTriggerHandle = VirtualTriggerHandle;

			return result;
		}

		protected override bool ValueEquals(MegaloScriptValueBase other)
		{
			var obj = (MegaloScriptVirtualTriggerValue)other;

			return VirtualTriggerHandle.Equals(obj.VirtualTriggerHandle);
		}

		#region IBitStreamSerializable Members
		public override void Serialize(MegaloScriptModel model, IO.BitStream s)
		{
			MegaloScriptVirtualTrigger trigger;
			if (s.IsReading)
			{
				trigger = model.CreateVirtualTrigger();
				mVirtualTriggerHandle = trigger.Handle;
			}
			else
				trigger = model.VirtualTriggers[VirtualTriggerHandle.Id];

			trigger.Serialize(model, s);
		}
		#endregion
		#region ITagElementStringNameStreamable Members
		protected override void SerializeValue<TDoc, TCursor>(MegaloScriptModel model, IO.TagElementStream<TDoc, TCursor, string> s)
		{
			if ((model.TagElementStreamSerializeFlags & MegaloScriptModelTagElementStreamFlags.EmbedObjects) != 0)
				using (s.EnterCursorBookmark("VT")) // have to nest or MegaloScriptModelObjectHandle will overwrite our Param ID with the VT's
					MegaloScriptModelObjectHandle.SerializeForEmbed(s, model, ref mVirtualTriggerHandle);
			else
			{
				if (s.IsReading)
				{
					int id = TypeExtensions.kNone; s.ReadCursor(ref id);
					if (id < 0)
					{
						throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
							"VirtualTrigger value #{0} has an invalid value {1}", Id, id));
					}

					mVirtualTriggerHandle = model.VirtualTriggers[id].Handle;
				}
				else
					s.WriteCursor(VirtualTriggerHandle.Id);
			}
		}
		#endregion
	};
}
