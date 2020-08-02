
namespace KSoft.Blam.Megalo.Proto
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	public enum MegaloScriptProtoVariableDataType
	{
		Numeric,
		Player,
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		Object,
		Team,
		Timer,

		Bool,
		Byte,

		Unknown, // #TODO_BLAM: shouldn't need this, go away
	};
}
