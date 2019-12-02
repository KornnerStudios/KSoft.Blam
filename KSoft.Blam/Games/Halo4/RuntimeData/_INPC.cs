using System.ComponentModel;

namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	partial class WeaponTuningBarrelModifierData
		: INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion

		static readonly PropertyChangedEventArgs kIsUnchangedChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.IsUnchanged);

		static readonly PropertyChangedEventArgs[] kValueChanged = {
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value0),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value1),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value2),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value3),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value4),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value5),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value6),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value7),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value8),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value9),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value10),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value11),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value12),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value13),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value14),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value15),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value16),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value17),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value18),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value19),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value20),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value21),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value22),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value23),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value24),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value25),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value26),
		};
	};

	partial class WeaponTuningWeaponModifierData
		: INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion

		static readonly PropertyChangedEventArgs kIsUnchangedChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningWeaponModifierData x) => x.IsUnchanged);

		static readonly PropertyChangedEventArgs[] kValueChanged = {
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value0),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value1),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value2),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value3),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value4),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value5),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value6),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value7),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value8),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value9),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value10),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value11),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value12),
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningBarrelModifierData x) => x.Value13),
		};
	};
}