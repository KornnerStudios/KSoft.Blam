using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptVariableReferenceType : sbyte
	{
		Undefined = -1,
		Custom,
		Player,
		Object,
		Team,
		Timer,

		/// <remarks>3 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}

namespace KSoft.Blam
{
	using MegaloScriptVariableReferenceType = Blam.Megalo.MegaloScriptVariableReferenceType;
	using MegaloScriptVariableType = Blam.Megalo.MegaloScriptVariableType;

	partial class TypeExtensionsBlam
	{
		[Contracts.Pure]
		internal static MegaloScriptVariableType ToVariableType(this MegaloScriptVariableReferenceType type)
		{
			switch (type)
			{
				case MegaloScriptVariableReferenceType.	Custom:
					return MegaloScriptVariableType.	Numeric;
				case MegaloScriptVariableReferenceType.	Player:
					return MegaloScriptVariableType.	Player;
				case MegaloScriptVariableReferenceType.	Object:
					return MegaloScriptVariableType.	Object;
				case MegaloScriptVariableReferenceType.	Team:
					return MegaloScriptVariableType.	Team;
				case MegaloScriptVariableReferenceType.	Timer:
					return MegaloScriptVariableType.	Timer;

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
	};
}