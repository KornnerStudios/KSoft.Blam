
namespace KSoft.Blam.Megalo
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptOperationType : byte
	{
		Add,
		Subtract,
		Multiply,
		Divide,
		Set, // Assign
		Modulus,
		And, // BitwiseAnd
		Or, // BitwiseOr
		Xor,
		Not,
		Shl,
		Shr,
		Shl2,
	};
}