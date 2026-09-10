using System.ComponentModel;

namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	partial class WeaponTuningBarrelModifierData
		: INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler? PropertyChanged;

		void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion

		static readonly PropertyChangedEventArgs[] kValueChanged;

		static WeaponTuningBarrelModifierData()
		{
			kValueChanged = [
				kValue0ChangedEventArgs,
				kValue1ChangedEventArgs,
				kValue2ChangedEventArgs,
				kValue3ChangedEventArgs,
				kValue4ChangedEventArgs,
				kValue5ChangedEventArgs,
				kValue6ChangedEventArgs,
				kValue7ChangedEventArgs,
				kValue8ChangedEventArgs,
				kValue9ChangedEventArgs,
				kValue10ChangedEventArgs,
				kValue11ChangedEventArgs,
				kValue12ChangedEventArgs,
				kValue13ChangedEventArgs,
				kValue14ChangedEventArgs,
				kValue15ChangedEventArgs,
				kValue16ChangedEventArgs,
				kValue17ChangedEventArgs,
				kValue18ChangedEventArgs,
				kValue19ChangedEventArgs,
				kValue20ChangedEventArgs,
				kValue21ChangedEventArgs,
				kValue22ChangedEventArgs,
				kValue23ChangedEventArgs,
				kValue24ChangedEventArgs,
				kValue25ChangedEventArgs,
				kValue26ChangedEventArgs,
			];
		}
	};

	partial class WeaponTuningWeaponModifierData
		: INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler? PropertyChanged;

		void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion

		static readonly PropertyChangedEventArgs kIsUnchangedChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((WeaponTuningWeaponModifierData x) => x.IsUnchanged);

		static readonly PropertyChangedEventArgs[] kValueChanged = [
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
		];
	};
}
