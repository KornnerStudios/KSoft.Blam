using System;

namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum WeaponTuningBarrelModifiers : ulong // barrel modifiers
	{
		Value0=1<<0,	// 16, 0x4 (rounds per second lower)
		Value1=1<<1,	// 16, 0x8 (rounds per second upper)
		Value2=1<<2,	// 16, 0x178
		Value3=1<<3,	// 16, 0x17C
		Value4=1<<4,	// 7,  0x24 (shots per fire lower)
		Value5=1<<5,	// 7,  0x26 (shots per fire upper)
		Value6=1<<6,	// 16, 0x28 (fire recovery time)
		Value7=1<<7,	// 16, 0x2C (soft recovery fraction)
		Value8=1<<8,	// 16, 0x180 (bloom reset speed)
		Value9=1<<9,	// 16, 0x54 (error angle lower)
		Value10=1<<10,	// 16, 0x58 (error angle upper)
		Value11=1<<11,	// 20	0.0	130.0		true	true, 0x5C
		Value12=1<<12,	// 20	0.0	130.0		true	true, 0x60
		Value13=1<<13,	// 20	0.0	10.0		true	true, 0x64
		Value14=1<<14,	// 20	0.0	6.2831855	true	true, 0x7C
		Value15=1<<15,	// 20	0.0	6.2831855	true	true, 0x80
		Value16=1<<16,	// 20	0.0	6.2831855	true	true, 0x84
		Value17=1<<17,	// 16, 0x15C (heat generated per round)
		Value18=1<<18,	// 16, 0x164 (charge % subtraction)
		Value19=1<<19,	// 20	0.0	10.0		true	true, ?->0x34
		Value20=1<<20,	// 20	0.0	3000.0		true	true, 0x38
		Value21=1<<21,	// 20	0.0	3000.0		true	true, 0x3C
		Value22=1<<22,	// 20	0.0	3000.0		true	true, 0x40
		Value23=1<<23,	// 20	0.0	1000.0		true	true, proj->0x390/0x39C (damage range lower)
		Value24=1<<24,	// 20	0.0	1000.0		true	true, proj->0x394/0x3A0 (damage ranger upper)
		Value25=1<<25,	// 16, proj->0x418->0x4 (buckshot accuracy)
		Value26=1<<26,	// 16, proj->0x418->0x8 (buckshot spread)
	};

	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[Flags]
	public enum WeaponTuningModifiers : ulong // weapon modifiers
	{
		Value0=1<<0,	// 20	0.0	1.0			true	true, 0x31C (heat recovery threshold)
		Value1=1<<1,	// 20	0.0	1.0			true	true, 0x320 (overheated threshold)
		Value2=1<<2,	// 20	0.0	1.0			true	true, 0x32C (heat loss per second)
		Value3=1<<3,	// 20	0.0	1.0			true	true, 0x344 (heat illumination)
		Value4=1<<4,	// 20	0.0	1.0			true	true, 0x348 (overheated heat loss per second)
		Value5=1<<5,	// 20	0.0	1.5706964	true	true, weap->0x420->0x8 (auto aim angle)
		Value6=1<<6,	// 20	0.0	1000.0		true	true, weap->0x420->0xC (auto aim range long)
		Value7=1<<7,	// 20	0.0	1000.0		true	true, weap->0x420->0x10 (auto aim range short)
		Value8=1<<8,	// 20	0.0	1000.0		true	true, weap->0x420->0x14
		Value9=1<<9,	// 20	0.0	1.5706964	true	true, weap->0x420->0x18 (magnetism angle)
		Value10=1<<10,	// 20	0.0	1000.0		true	true, weap->0x420->0x1C (magnetism range long)
		Value11=1<<11,	// 20	0.0	1000.0		true	true, weap->0x420->0x20 (magnetism range short)
		Value12=1<<12,	// 20	0.0	1000.0		true	true, weap->0x420->0x24
		Value13=1<<13,	// 20	0.0	6.2831855	true	true, haven't seen implemented but would wager weap->0x420->0x28 (deviation angle)
	};
}