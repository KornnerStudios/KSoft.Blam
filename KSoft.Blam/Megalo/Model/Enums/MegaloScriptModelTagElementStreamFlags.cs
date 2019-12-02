using System;

namespace KSoft.Blam.Megalo.Model
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum MegaloScriptModelTagElementStreamFlags
	{
		EmbedObjects = 1<<0,
		UseEnumNames = 1<<1,
		UseIndexNames = 1<<2,
		UseConditionTypeNames = 1<<3,
		UseActionTypeNames = 1<<4,
		WriteConditionTypeNames = 1<<5,
		WriteActionTypeNames = 1<<6,
		WriteParamKinds = 1<<7,
		WriteParamSigIds = 1<<8,
		WriteParamTypes = 1<<9,
		WriteParamNames = 1<<10,
		//11
		//12
		// Exceptions: Global objects
		// Adding support for this kind of hacked the code up in some places:
		// eg, MegaloScriptModelObjectHandle's and MegaloScriptValueBase's tag-element streaming code and NewTriggerFromTagStream
		EmbedObjectsWriteSansIds = 1<<13,
		TryToPort = 1<<14,
		IgnoreVersionIds = 1<<15,

		kParamsMask = WriteParamKinds | WriteParamSigIds | WriteParamTypes | WriteParamNames,
	};
}